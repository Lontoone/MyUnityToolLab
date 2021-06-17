# ActionController.cs
	A tool to sort action.

## How To Use
	Make it a component of a gameobject. And for example, have a PlayerControl script to declear some actions.
	![example Image](https://i.imgur.com/sZoD8UC.png)
	
	In this example project, I have decleared 16 actions on `PlayerControl.cs`. 
	
	Each mAction class contains these properties.
	example:
	![example Image](https://i.imgur.com/LztlBZl.png)
	

### Propreties

` Description ` a string. Just for better recognized when debug.

`Is in gap time lock` a private bool. True when this action is in cooldown time. You don't need to do anything to it. I exposed it just for better debug.

`Action` Assign the public function to do.

`CallbackEvent` Assign the public function to be called when `Action` is finished or interrupted by other action.

`Priority` The priority of this action.

`Force` Is this action able to interrupt other action with lower priority?

`Duration` The duration for this action.

`Is Loop` Do this function in loop of fixedUpdatedTime? or do it once.

`Time out` How long can this action to wait in a queue.


### Example
```
public class PlayerControl : MonoBehaviour
{
	private ActionController actionController;
	public ActionController.mAction idle, walk, hurt, jump_start, jumping, falling, jump_end, doubleJump, dash, duck, stop, hurt_falling, revive, die;
	
	 private void Start()
    {
        actionController = gameObject.GetComponent<ActionController>();
    }
	private void Update()
    {    
        if (Input.GetKeyDown(jump_key) && (jumpCount < 2))
        {
            Debug.Log("jump");
            if (jumpCount == 0)
            {
                Debug.Log("Jump start");
                actionController.AddAction(jump_start);
            }
            else if (jumpCount == 1)
            {
                Debug.Log("Jump double");
                actionController.AddAction(doubleJump);
            }
            jumpCount++;
        }
        //Dash
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
        }

    }
}

```
