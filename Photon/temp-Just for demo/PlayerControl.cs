using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using DG.Tweening;
using Photon.Pun;
using Photon.Realtime;
//玩家控制
public class PlayerControl : MonoBehaviour
{
    public static event Action<int> OnCreate;
    public static event Action<int> OnDestory;
    public float speed = 5;
    public float jumpForce = 10;
    [HideInInspector]
    public HitableObj hitable;

    public ActionController.mAction idle, walk, hurt, jump_start, jumping, falling, jump_end, doubleJump, dash, duck, stop, hurt_falling, revive, die;
    public ActionController.mAction landing, land_end;

    /*control keys*/
    public string horizontal_axis_name = "Horizontal";  //default
    public string vertical_axis_name = "Vertical";      //default
    //public string jump_axis_name = "Jump";            //default
    public KeyCode jump_key = KeyCode.Space;
    public KeyCode dash_key = KeyCode.LeftShift;
    public KeyCode duck_key = KeyCode.DownArrow;
    public KeyCode attack_key = KeyCode.Z;
    public KeyCode defense_key = KeyCode.X;

    public Head head;
    public Body body;

    public float dashForce = 15;
    public float healAmount = 50;
    [HideInInspector]
    public PhotonView _pv;
    //[HideInInspector]
    public int dataIndex = 0;

    [SerializeField]
    private int jumpCount = 0; //跳躍次數 (for 2段跳)

    [SerializeField]
    private Ease easeType;

    [SerializeField]
    private LayerMask obsticalLayerMask;

    private Rigidbody2D rigid;
    private ActionController actionController;
    private PhysicsControlListeners listeners;
    [SerializeField]
    private Collider2D _mainCollider;

    private Coroutine c_heal;
    private Coroutine cCreateImageTrail;
    private bool _isOnline = false;
    private Transform _lastHitSource;
    public Player[] _otherPlayers;

    private bool _hasInited = false;
    public string GCKey
    {
        get
        {
            return "Player" + dataIndex;
        }
    }

    [SerializeField]
    private bool _isHurting = false;
    private Vector2 _footPosition
    {
        get
        {
            if (listeners == null)
            {
                return transform.position;
            }
            else
            {
                return listeners.footPositon.transform.position;
            }
        }
    }
    private bool isGrounded
    {
        get
        {
            if (listeners != null)
            {
                return listeners.isGrounded;
            }
            else
            {
                return Physics2D.Raycast(_footPosition, -transform.up, 0.2f, LayerMask.NameToLayer("Ground"));
            }
        }
    }

    private void SetUpLocalEvent()
    {
        hitable = gameObject.GetComponent<HitableObj>();
        listeners = gameObject.GetComponent<PhysicsControlListeners>();
        if (actionController != null)
        {
            actionController.eActionQueueCleared += AddDefault;
        }
        if (hitable != null)
        {
            hitable.Die_event += AddDie;
            //hitable.gotHit_event += Hurt;
            //hitable.gotHit_event += OnHurt;
            hitable.HitBy_event += OnHurt;
            //hitable.HitBy_event += HurtDirectionCheck;
        }
        if (listeners != null)
        {
            listeners.eOnTouchGround += ResetJumpCount;
            listeners.eOnTouchGround += OnJumpEnd;
        }
    }

    private void Start()
    {
        //GCManager.RegisterObject(GCKey, gameObject, true);
    }
    private void OnDestroy()
    {
        if (actionController != null)
        {
            actionController.eActionQueueCleared -= AddDefault;
        }
        if (hitable != null)
        {
            hitable.Die_event -= AddDie;
            //hitable.gotHit_event -= OnHurt;
            hitable.HitBy_event -= OnHurt;
            //hitable.HitBy_event -= HurtDirectionCheck;
        }
        if (listeners != null)
        {
            listeners.eOnTouchGround -= OnJumpEnd;
            listeners.eOnTouchGround -= ResetJumpCount;
        }
    }
    [PunRPC]
    public void SetUpOnline(int _playerIndex)
    {
        OnCreate?.Invoke(_playerIndex);
        _pv = GetComponent<PhotonView>();
        hitable = gameObject.GetComponent<HitableObj>();
        if (_pv == null)
        {
            return;
        }
        _isOnline = true;
        dataIndex = _playerIndex;
        Debug.Log("player index " + dataIndex);

        if (!_pv.IsMine)
        {
            //Destroy(GetComponent<PhysicsControlListeners>());
            Destroy(GetComponent<Rigidbody2D>());
            Destroy(GetComponent<ActionController>());
            //Destroy(body.GetComponent<PlayerAttackControl>());
            //Destroy(this);
        }
        else
        {
            //if (_hasInited){return;}
            //_hasInited = true;

            rigid = gameObject.GetComponent<Rigidbody2D>();
            listeners = gameObject.GetComponent<PhysicsControlListeners>();
            actionController = gameObject.GetComponent<ActionController>();
            _otherPlayers = GetOtherPlayer();
            SetUpLocalEvent();

            string _head_path = "Prefab/Online/Head/" + PlayerSlot.heads_res[(int)PhotonNetwork.LocalPlayer.CustomProperties[CustomPropertyCode.HEAD_CDOE]].name;
            string _body_path = "Prefab/Online/Body/" + PlayerSlot.body_res[(int)PhotonNetwork.LocalPlayer.CustomProperties[CustomPropertyCode.BODY_CODE]].name;
            Head _newHead =
                             PhotonNetwork.Instantiate(
                             _head_path,
                             //Instantiate(
                             //Resources.Load<Head>(_head_path),
                             head.transform.position,
                             Quaternion.identity
                             ).GetComponent<Head>();
            Body _newBody =
                            PhotonNetwork.Instantiate(
                            _body_path,
                            //Instantiate(
                            //Resources.Load<Body>(_body_path),
                            body.transform.position,
                            Quaternion.identity
                            ).GetComponent<Body>();

            //_newBody.transform.SetParent(body.transform.parent);
            _newBody.transform.SetParent(transform);
            _newHead.transform.SetParent(transform);
            //_newHead.transform.SetParent(head.transform.parent);
            Destroy(head.gameObject);
            Destroy(body.gameObject);

            head = _newHead;
            body = _newBody;
            RpcSetUPParent();
            _pv.RPC("RpcSetupTeam", RpcTarget.All,
                                    (int)PhotonNetwork.LocalPlayer.CustomProperties[CustomPropertyCode.TEAM_CODE],
                                    (int)PhotonNetwork.LocalPlayer.CustomProperties[CustomPropertyCode.TEAM_LAYER]);

            SetKey(0);

            SetupRpcFunction();
            //temp
            //AddLanding();
        }
    }
    public void RpcSetUPParent()
    {
        head.GetComponent<PhotonView>().RPC("RpcSetParent", RpcTarget.All, dataIndex);
        body.GetComponent<PhotonView>().RPC("RpcSetParent", RpcTarget.All, dataIndex);
    }
    [PunRPC]
    public void RpcSetupTeam(int _team_code, int _layerCode)
    {
        gameObject.layer = LayerMask.NameToLayer("Player" + _layerCode);
        Debug.Log("set Team color " + CustomPropertyCode.TEAMCOLORS[_team_code] + " " + _team_code);

        //set team color        
        head.GetComponent<SpriteRenderer>().color = CustomPropertyCode.TEAMCOLORS[_team_code];
        body.GetComponent<SpriteRenderer>().color = CustomPropertyCode.TEAMCOLORS[_team_code];
    }
    private void SetupRpcFunction()
    {
        Debug.Log("pv " + _pv.ViewID + " set up act ");
        walk.action.AddListener(delegate { DoRpcOnAllOtherPlayers("Walk_animation"); });
        idle.action.AddListener(delegate { DoRpcOnAllOtherPlayers("Idle"); });

        hurt.action.AddListener(delegate
        {
            DoRpcOnAllOtherPlayers("Hurt");
            DoRpcOnAllOtherPlayers("SyncHPOnHurt", hitable.HP);
        });
        hurt.callbackEvent.AddListener(delegate
        {
            DoRpcOnAllOtherPlayers("ResetHitCombo");
        });
        hurt_falling.action.AddListener(delegate
        {
            DoRpcOnAllOtherPlayers("Hurt_Fly");
        });
        hurt_falling.callbackEvent.AddListener(delegate
        {
            DoRpcOnAllOtherPlayers("ResetHitCombo");
            DoRpcOnAllOtherPlayers("Jump_start");
        });

        jump_start.action.AddListener(delegate
        {
            //DoRpcOnAllOtherPlayers("Move");
            DoRpcOnAllOtherPlayers("Jump_start");
        });
        jumping.action.AddListener(delegate
        {
            //DoRpcOnAllOtherPlayers("Move");
            DoRpcOnAllOtherPlayers("Jumping");
        });
        falling.action.AddListener(delegate
        {
            //DoRpcOnAllOtherPlayers("Move");
            DoRpcOnAllOtherPlayers("Falling");
        });
        jump_end.action.AddListener(delegate
        {
            //DoRpcOnAllOtherPlayers("Move");
            DoRpcOnAllOtherPlayers("Jump_End");
        });

        dash.action.AddListener(delegate
        {
            DoRpcOnAllOtherPlayers("Dash");
        });
        dash.callbackEvent.AddListener(delegate
        {
            //DoRpcOnAllOtherPlayers("Stop");
            DoRpcOnAllOtherPlayers("DashCallBack");
        });

        duck.action.AddListener(delegate
        {
            DoRpcOnAllOtherPlayers("Duck");
        });
        duck.callbackEvent.AddListener(delegate
        {
            DoRpcOnAllOtherPlayers("DuckCallback");
        });

        landing.action.AddListener(delegate
        {
            DoRpcOnAllOtherPlayers("Landing");
        });

        land_end.action.AddListener(delegate
        {
            DoRpcOnAllOtherPlayers("LandingEnd");
        });
        revive.action.AddListener(delegate
        {
            DoRpcOnAllOtherPlayers("ReviveAnimation");
        });
        die.action.AddListener(delegate
        {
            DoRpcOnAllOtherPlayers("Die");
        });
    }
    public void SetUp(LocalPlayerProperty _data, int _i)
    {
        OnCreate?.Invoke(_i);

        rigid = gameObject.GetComponent<Rigidbody2D>();
        listeners = gameObject.GetComponent<PhysicsControlListeners>();
        actionController = gameObject.GetComponent<ActionController>();
        SetUpLocalEvent();
        dataIndex = _i;

        Head _newHead =
            Instantiate(
                Head.LoadHead(PlayerSlot.heads_res[(int)_data.playerProperty[CustomPropertyCode.HEAD_CDOE]].name).gameObject,
                head.transform.position,
                Quaternion.identity,
                head.transform.parent
                ).GetComponent<Head>();
        Body _newBody =
            Instantiate(
                //Body.LoadBody(_data.playerProperty[CustomPropertyCode.BODY_CODE] as string).gameObject,
                Body.LoadBody(PlayerSlot.body_res[(int)_data.playerProperty[CustomPropertyCode.BODY_CODE]].name).gameObject,
                body.transform.position,
                Quaternion.identity,
                body.transform.parent
                ).GetComponent<Body>();

        Destroy(head.gameObject);
        Destroy(body.gameObject);

        head = _newHead;
        body = _newBody;
        body.GetComponent<PlayerAttackControl>()._player = this;
        head.ApplyBuff();

        //********Set Keys *****************
        SetKey(_i);


        //set team Layer
        //gameObject.layer = LayerMask.NameToLayer("Player" + _data.playerProperty[CustomPropertyCode.TEAM_CODE]);
        gameObject.layer = LayerMask.NameToLayer("Player" + _data.playerProperty[CustomPropertyCode.TEAM_LAYER]);

        int _team_code = (int)_data.playerProperty[CustomPropertyCode.TEAM_CODE];
        Debug.Log("set Team color " + CustomPropertyCode.TEAMCOLORS[_team_code] + " " + _team_code);

        //set team color        
        head.GetComponent<SpriteRenderer>().color = CustomPropertyCode.TEAMCOLORS[_team_code];
        body.GetComponent<SpriteRenderer>().color = CustomPropertyCode.TEAMCOLORS[_team_code];

        //Landing animation  (called by manager)
        //AddLanding();
        //Test: 
        //AddRevive();
    }
    private void SetKey(int index)
    {
        int _i = index % 2;
        horizontal_axis_name = "h" + _i.ToString();
        vertical_axis_name = "v" + _i.ToString();
        //jump_axis_name = "j" + _i.ToString();
        jump_key = CustomPropertyCode.JumpKeys[_i];
        dash_key = CustomPropertyCode.DashKyes[_i];
        duck_key = CustomPropertyCode.DuckKyes[_i];
        attack_key = CustomPropertyCode.AttackKyes[_i];
        defense_key = CustomPropertyCode.DefenseKyes[_i];
    }

    private void Update()
    {
        if (_pv != null && !_pv.IsMine)
        {
            return;
        }

        //跳躍
        //Duck and jump down
        if (Input.GetKey(duck_key) && Input.GetKeyDown(jump_key))
        {
            //check ground is pass throughable?
            GameObject __ground = listeners.currentStandingGround;
            if (__ground != null && __ground.tag == "ThinWall")
                transform.DOMoveY(transform.position.y - 3.5f, 0.1f);
        }
        else if (Input.GetKeyDown(jump_key) && (jumpCount < 2))
        {
            Debug.Log("jump");
            if (jumpCount == 0)
            {
                Debug.Log("Jump start");
                Jump_start();
                AddJumpForce();
                actionController.AddAction(jump_start);
            }
            //else if (!listeners.isGrounded)
            else if (jumpCount == 1)
            {
                Debug.Log("Jump double");
                actionController.AddAction(doubleJump);
            }
            jumpCount++;
            Debug.Log("jump count" + jumpCount);
        }

        //Dash
        //if (Input.GetKeyDown(KeyCode.LeftShift))
        if (Input.GetKeyDown(dash_key))
        {
            actionController.AddAction(dash);
        }

        //Duck
        if (Input.GetKeyDown(duck_key))
        {
            actionController.AddAction(duck);

        }

        //Duck Finish
        if (Input.GetKeyUp(duck_key))
        {
            actionController.AddAction(stop);
            //StopCoroutine(c_heal);
        }

    }
    public void AddJumpForce()
    {
        rigid.velocity = new Vector2(rigid.velocity.x, jumpForce);
        //rigid.AddForce(transform.up*jumpForce*100);
        Debug.Log("jump force " + rigid.velocity);
    }
    void AddDefault()
    {
        //actionController.AddAction(idle);
    }
    private void FixedUpdate()
    {
        if (_pv != null && !_pv.IsMine)
        {
            return;
        }

        //動畫判定:
        if (!listeners.isGrounded) //跳躍
        {
            if (rigid.velocity.y < -0.2f)
            {
                Debug.Log("jump: falling");
                actionController.AddAction(falling);
            }
            else //(rigid.velocity.y > 0.1f)
            {
                actionController.AddAction(jumping);
            }
            /*
            else if (rigid.velocity.normalized.y < 0.01f)
            {
                //actionController.AddAction(jumpEpic);
                Debug.Log("epic");
            }*/
            return;
        }
        //else if (Mathf.Abs(rigid.velocity.x) > 1f) //移動
        //else if (Mathf.Abs(Input.GetAxis("Horizontal")) > 0.15f) //移動
        else if (Mathf.Abs(Input.GetAxis(horizontal_axis_name)) > 0.15f) //移動
        {
            Debug.Log("add walk " + listeners.isGrounded);
            actionController.AddAction(walk);

            //animator.Play("Walk");
            //jumpCount = 0; //TODO:暫時的

        }
        else
        {
            actionController.AddAction(idle);
        }

    }
    [PunRPC]
    void OnJumpEnd()
    {
        actionController.AddAction(jump_end);
    }

    [PunRPC]
    public void Move()
    {
        rigid.velocity = new Vector2(Input.GetAxis(horizontal_axis_name) * speed, rigid.velocity.y);
        if (MapControl.instance != null)
        {
            transform.position = MapControl.instance.GetMoveablePosition(transform.position);

        }
        //左右翻轉:
        //if (_x_input> 0)
        if (rigid.velocity.x > 0)
        {
            transform.eulerAngles = new Vector3(0, 180, 0);
        }
        //else if (_x_input< 0)
        else if (rigid.velocity.x < 0)
        {
            transform.eulerAngles = new Vector3(0, 0, 0);
        }
    }

    [PunRPC]
    public void Hurt()
    {
        PlayAniamtion("Hurt");
    }
    [PunRPC]
    public void SyncHPOnHurt(float _hp)
    {
        hitable.SetHP(_hp);
    }
    public void OnHurt(GameObject _source)
    {
        //_lastHitSource = _source.transform;
        Debug.Log(gameObject.name + " onHurt");
        SFXManager.instance.PlaySoundInstance(SFXManager.HURT);
        //Defense Check
        bool _isInBack = IsInBack(_source);
        if (body.attackControl.isDefending && !_isInBack)
        {
            //no damage
            return;
        }
        else if (body.attackControl.isDefending && _isInBack)
        {
            Debug.Log("back attack!");
            //back attack
            HurtDirectionCheck(_source);
            actionController.AddAction(stop);
            actionController.AddAction(hurt_falling);
            return;
        }
        //Normal situation Check
        HurtDirectionCheck(_source);
        if (hitable.isHitable && !_isHurting)
        {
            if (isGrounded)
                actionController.AddAction(hurt);
            else
                actionController.AddAction(hurt_falling);
            _isHurting = true;
            Debug.Log("HURT!");
        }
        else if (_isHurting)
        {
            //got hit when playing hurt animation
            actionController.AddAction(hurt_falling);
        }
    }
    [PunRPC]
    public void ResetHitCombo()
    {
        _isHurting = false;
        hitable.hit_combo = 0;
    }
    [PunRPC]
    public void Walk_animation()
    {
        head.PlayAnimation("Walk");
        body.PlayAnimation("Walk");
    }
    [PunRPC]
    public void Idle()
    {
        head.PlayAnimation("Idle");
        body.PlayAnimation("Idle");
    }
    [PunRPC]
    public void Jump_start()
    {
        head.PlayAnimation("Jump Falling");
        body.PlayAnimation("Jump Falling");
        SFXManager.instance.PlaySoundInstance(SFXManager.JUMP);
        Effect("Jump Smoke", "jump smoke");
    }
    [PunRPC]
    public void Jumping()
    {
        head.PlayAnimation("Jumping");
        body.PlayAnimation("Jumping");
    }
    [PunRPC]
    public void Falling()
    {
        head.PlayAnimation("Jump Falling");
        body.PlayAnimation("Jump Falling");
    }
    [PunRPC]
    public void Jump_End()
    {
        head.PlayAnimation("Jump-End");
        body.PlayAnimation("Jump-End");
    }
    [PunRPC]
    public void Landing()
    {
        PlayAniamtion("Landing");
    }
    [PunRPC]
    public void LandingEnd()
    {
        head.PlayAnimation("Landing Fall");
        body.PlayAnimation("Landing Fall");
        Effect("landing effect", "Landing");
    }
    [PunRPC]
    public void Dash()
    {
        head.PlayAnimation("Dash");
        body.PlayAnimation("Dash");

        hitable.isHitable = false;
        RaycastHit2D hit = Physics2D.Raycast(transform.position, -transform.right, dashForce, obsticalLayerMask);
        Vector2 _endPos;

        if (hit.collider != null)
        {
            _endPos = hit.point;
        }
        else
        {
            _endPos = transform.position - transform.right * dashForce;
        }
        Effect("dash smoke", "dash smoke", transform.rotation);

        DOTween.Sequence().
            Append(transform.DOMove(_endPos, dash.duration)).SetEase(easeType);

        StartCreateTrail();

    }
    [PunRPC]
    public void DashCallBack()
    {
        //reset
        hitable.isHitable = true;
        StopCreateTrail();
    }
    [PunRPC]
    public void Duck()
    {
        head.PlayAnimation("Duck");
        body.PlayAnimation("Duck");
        if (c_heal == null)
            c_heal = StartCoroutine(HealCoro());
    }
    [PunRPC]
    public void DuckCallback()
    {
        Debug.Log("duck call back");
        StopCoroutine(c_heal);
        c_heal = null;
    }

    //first time to join the game
    [PunRPC]
    public void AddLanding()
    {
        if (actionController == null)
            actionController = GetComponent<ActionController>();
        actionController.AddAction(landing);
        listeners.eOnTouchGround += AddLandEnd;
        cCreateImageTrail = StartCoroutine(CreateTrailCoro());
    }
    public void AddLandEnd()
    {
        actionController.AddAction(land_end);
        listeners.eOnTouchGround -= AddLandEnd;

        StopCoroutine(cCreateImageTrail);
        //PlayAniamtion("landing fall");
    }
    public void CreateImageTrail()
    {
        head.CreateImageTrail();
        body.CreateImageTrail();
    }
    [PunRPC]
    public void ReviveAnimation()
    {
        head.PlayAnimation("Revive");
        body.PlayAnimation("Revive");
    }
    public void AddRevive()
    {
        actionController.AddAction(revive);
        SetCollider(false);
    }

    public void ReviveMove()
    {
        rigid.Sleep();
        Vector3 _move = new Vector3(Input.GetAxisRaw(horizontal_axis_name),
                                                                Input.GetAxisRaw(vertical_axis_name));
        transform.position = transform.position + _move * Time.deltaTime * speed;

        if (_move.x > 0)
        {
            transform.eulerAngles = new Vector3(0, 180, 0);

        }
        else if (_move.x < 0)
        {
            transform.eulerAngles = new Vector3(0, 0, 0);
        }
    }
    [PunRPC]
    public void SetCollider(bool _isEnable)
    {
        _mainCollider.enabled = _isEnable;
    }


    IEnumerator HealCoro()
    {
        WaitForSeconds _oneSec = new WaitForSeconds(0.5f);
        float _prewait = 1.5f;
        while (_prewait > 0)
        {
            _prewait -= 0.5f;
            yield return _oneSec;
        }
        while (true)
        {
            Animator _effect = GCManager.Instantiate("Heal Effect", _position: _footPosition).GetComponent<Animator>();
            _effect.Play("heal");
            hitable.Heal(healAmount);
            yield return _oneSec;
        }
    }


    private void StartCreateTrail()
    {
        if (cCreateImageTrail == null)
            cCreateImageTrail = StartCoroutine(CreateTrailCoro());
    }
    private void StopCreateTrail()
    {
        if (cCreateImageTrail != null)
        {
            StopCoroutine(cCreateImageTrail);
            cCreateImageTrail = null;
        }
    }
    IEnumerator CreateTrailCoro()
    {
        WaitForFixedUpdate _wait = new WaitForFixedUpdate();
        while (true)
        {
            CreateImageTrail();
            yield return _wait;
        }
    }
    public void Stop()
    {
        actionController.AddAction(stop);
        rigid.velocity = Vector2.zero;
    }

    public void Effect(string _gc_key, string _clip_name, Quaternion _rotation = default, Vector3 _position = default)
    {
        GameObject _effect = GCManager.Instantiate(_gc_key, _rotation: _rotation, _position: _position);
        if (_effect != null)
        {
            _effect.GetComponent<Animator>().Play(_clip_name);

            if (_position == default)
                _effect.transform.position = _footPosition;
            //_effect.transform.rotation = _rotation;
            Debug.Log(_rotation + " " + _effect.transform.rotation + " ");
        }
    }

    public void HurtDirectionCheck(GameObject _hitBy)
    {
        //Check back attack or front attack
        bool _isBackAttack = IsBackAttack(_hitBy);

        Vector3 dir = transform.position - _hitBy.transform.position;
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        Quaternion _rotateQ = Quaternion.AngleAxis(angle, Vector3.forward);

        if (_isBackAttack)
        {
            DoBackHitEffect(_rotateQ, transform.TransformPoint(0f, 2f, 0));
            DoRpcOnAllOtherPlayers("DoBackHitEffect", _rotateQ, transform.TransformPoint(0f, 2f, 0));
        }
        else
        {
            DoFrontHitEffect(_rotateQ, transform.TransformPoint(0f, 2f, 0));
            DoRpcOnAllOtherPlayers("DoFrontHitEffect", _rotateQ, transform.TransformPoint(0f, 2f, 0));
        }
    }
    [PunRPC]
    private void DoBackHitEffect(Quaternion _rotation, Vector3 _position)
    {
        Effect("back hit effect", "back hit effect", _rotation, _position);
        Effect("back hit blood effect", "back hit blood effect", _rotation, _position);
    }
    [PunRPC]
    private void DoFrontHitEffect(Quaternion _rotation, Vector3 _position)
    {
        Effect("front hit effect", "front hit effect", _rotation, _position);
        Effect("front hit blood effect", "front hit blood effect", _rotation, _position);
    }

    private bool IsInBack(GameObject _source)
    {
        return Vector2.Dot(transform.right, _source.transform.right) > 0;
    }
    private bool IsBackAttack(GameObject _source)
    {
        //Turn to hit sources
        if (_source != null && IsInBack(_source))
        {
            transform.eulerAngles = new Vector3(0, transform.eulerAngles.y + 180, 0);
            return true;
        }
        else
        {
            return false;
        }
    }
    //hit to sky
    [PunRPC]
    public void Hurt_Fly()
    {
        if (rigid != null)
        {
            const float _dashForceMultipier = 100;
            //add force to sky 
            Vector2 _dir = new Vector2(transform.right.x * 0.5f, 1);
            rigid.AddForce(_dir * dashForce * _dashForceMultipier);
        }
        PlayAniamtion("Hurt Falling");
    }
    public void AddDie()
    {
        actionController.AddAction(die);
        //disable control
        hitable.isHitable = false;
        actionController.StopAllCoroutines();
    }
    [PunRPC]
    public void Die()
    {
        Debug.Log("玩家死亡");
        SFXManager.instance.PlaySoundInstance(SFXManager.EXPLODE);
        Effect("die disappear", "die disappear");
        SlowMotionEffector.instance?.DoSlowMotion();

        PlayAniamtion("Die");
        Invoke("DestoryObject", 3);
    }

    [PunRPC]
    private void DestoryObject()
    {
        //TODO: Use GC
        /*
        if (actionController != null)
        {
            //actionController.ClearCurrent();
            actionController.AddAction(stop);
            _mainCollider.enabled = true;
        }
        GCManager.Destory(GCKey, gameObject);*/

        OnDestory?.Invoke(dataIndex);
        Destroy(gameObject);
        Destroy(actionController);
        this.enabled = false;
    }
    //create new player 3sec after die
    void RecreatePlayer()
    {
        LocalRoomManager.instance.Revive(dataIndex);
        //Destroy(gameObject);
    }

    void ResetJumpCount()
    {
        Debug.Log("jump: TouchGround");
        jumpCount = 0;
        actionController.AddAction(jump_end);
        //animator.Play("Idle");
    }
    [PunRPC]
    public void PlayAniamtion(string _clipName)
    {
        head.PlayAnimation(_clipName);
        body.PlayAnimation(_clipName);
    }
    private Player[] GetOtherPlayer()
    {
        return PhotonNetwork.PlayerListOthers;
    }
    [PunRPC]
    public void DoRpcOnAllOtherPlayers(string _functionName)
    {
        if (!PhotonNetwork.IsConnected)
        {
            return;
        }
        for (int i = 0; i < _otherPlayers.Length; i++)
        {
            _pv.RPC(_functionName, _otherPlayers[i]);
        }
    }
    [PunRPC]
    public void DoRpcOnAllOtherPlayers(string _functionName, PhotonView _sourcePv)
    {
        if (!PhotonNetwork.IsConnected)
        {
            return;
        }
        for (int i = 0; i < _otherPlayers.Length; i++)
        {
            _sourcePv.RPC(_functionName, _otherPlayers[i]);
        }
    }
    [PunRPC]
    public void DoRpcOnAllOtherPlayers(string _functionName, params object[] _objs)
    {
        if (!PhotonNetwork.IsConnected)
        {
            return;
        }
        for (int i = 0; i < _otherPlayers.Length; i++)
        {
            _pv.RPC(_functionName, _otherPlayers[i], _objs);
        }
    }

    public static PlayerControl FindPlayerControlByIndex(int _i)
    {
        foreach (PlayerControl _p in FindObjectsOfType<PlayerControl>())
        {
            if (_p.dataIndex == _i)
                return _p;

            Debug.Log("FindPlayerControlByIndex " + _p.dataIndex);
        }
        return null;
    }
}
