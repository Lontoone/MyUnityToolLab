# Scripts info:

## SaveAndLoad.cs

### How to use:

Make sure your own class is tagged with [System.Serializable].

Function name ends with "_bin" means binary. The data is saved in binary format. Otherwise the data is saved in Json format.


### Save:
  ```
  SaveAndLoad.Save_bin<myType>(myData, myPath);
  
  SaveAndLoad.Save<myType>(myData, myPath);
  ```
  
### Load:
  ```
  myType mydate = SaveAndLoad.Load(myPath);
  
  myType mydate = SaveAndLoad.Load_bin(myPath);
  ```

If you want to save dictionery type, simple use the binary one.

<hr>

## BasicAI.cs (2D)

  It is for auto-move game objects.
  
  The script contains basic events that will automatically triggered, so you can have your own class to do the effect, such as "Walk","Jump" and "Idle". And also will chase the objects of enemy layer that is in sight(a 2D collider).
  
  The script work fine by its' own.
  ![image](https://i.imgur.com/0FKD53i.gif)
  
  ## How to use:  
   In the demo, I have a player and a monster with basicAI. The monster also have a monster class which handles the attack function and defined what Walk event would do ,etc.
   
   to use the script, for example in my monster class:
   ```
   BasicAI mAI;
    void Start()
    {
        mAI = gameObject.GetComponent<BasicAI>();
        mAI.jump_event += Jump;
        mAI.idle_event += Idle;
        mAI.walk_event += Walk;
    }
    
    void OnDestroy()
    {
        mAI.jump_event -= Jump;
        mAI.idle_event -= Idle;
        mAI.walk_event -= Walk;
    }
    
    void Jump(){}
    void Walk(){}
    void Idle(){}
   ```
  You can write your own Jump Walk Idle method, In my case, I only play animation, like:
  ```
  void Idle(){animator.Play("Idle");}
  ```
  
  If you want this monster to chase player, select player's layer to `attack_target_layer`, append a collider 2D to monster gameobject ,ckeck `is Trigger` and set to `Enemy_detect_range` parameter.
<hr>

## ScreenObjPicker.cs and PickableObject.cs

By attaching ScreenObjPicker.cs to any scene object, you can select object with type PickableObject from the screen view.

The script basically gives the idea on how to interact with world objects by converting the point on screneview.

![image](https://i.imgur.com/3Kh0b1P.gif)

### How to use:

If you want the selected objects to move to the selected point, make sure to append the NavMeshAgent to the object and set the scene NavMeshSurface. Otherwise you can delete the "void SelectPath()" function or rewrite your own.

The script ScreenObjPicker is better to go with the script PickableObject which provied functions to overwrite.


<hr>

# 腳本資訊:

## SaveAndLoad.cs

### 用法

使用前記得將自訂類別標上 [System.Serializable]。

結尾是 "_bin" 的方法代表示存成2進位檔. 其他則是Json格式。


### 存檔:
```
  SaveAndLoad.Save_bin<myType>(myData, myPath);
  
  SaveAndLoad.Save<myType>(myData, myPath);
```
  
### 讀檔:
```
  myType mydate = SaveAndLoad.Load(myPath);
  
  myType mydate = SaveAndLoad.Load_bin(myPath);
```

如果想存字典型態，可以用二進位的方法存。(Json不支援字典型態)

<hr>
## BasicAI.cs (2D)
 自動行走的基本AI。
  
  腳本帶有基本的Walk,Idle,Jump 事件。 也會偵測並追隨範圍內的指定圖層物件。
  
  腳本可獨立執行。
  
  ![image](https://i.imgur.com/0FKD53i.gif)
  
  ## 用法:  
   在demo裡，我有一個玩家操作的物件和一隻有basicAI的怪物，怪物自己本身也有"怪物"的類別，負責攻擊玩家和播放動畫。
   
   舉個例子，在我的"怪物"腳本裡:
   ```
   BasicAI mAI;
    void Start()
    {
        mAI = gameObject.GetComponent<BasicAI>();
        mAI.jump_event += Jump;
        mAI.idle_event += Idle;
        mAI.walk_event += Walk;
    }
    
    void OnDestroy()
    {
        mAI.jump_event -= Jump;
        mAI.idle_event -= Idle;
        mAI.walk_event -= Walk;
    }
    
    void Jump(){}
    void Walk(){}
    void Idle(){}
   ```
  你可以把要執行的事寫在Jump, Walk, Idle 方法裡面, 在這個例子中，我只有播放特定動作的動畫。
  ```
  void Idle(){animator.Play("Idle");}
  ```
  
  如果要怪物追著範圍內的敵人，`attack_target_layer`選擇玩家的layer，給怪物掛個collider 2D ,勾 `is Trigger` 然後指定給 `Enemy_detect_range` 參數。

<hr>


## ScreenObjPicker.cs and PickableObject.cs

將ScreenObjPicker.cs 掛在場景中的任一物件上, 即可選取掛有PickableObject元件的物件.

此腳本基本上是為了提供從screneView轉換至world position的作法。

![image](https://i.imgur.com/3Kh0b1P.gif)

### 用法:

如果想移動已選取的物件，請先在物件上掛NavMeshAgent元件，並在場景準備好NavMeshSurface。不然可以直接刪除void SelectPath()方法，或重寫自己的移動方法。

ScreenObjPicker腳本建議搭配PickableObject使用，因為PickableObject提供可覆寫的方法，方便操作物件有自己的屬性。

