### Get the package [here](https://github.com/Lontoone/MyUnity-tool-LevelFlowEditor/blob/main/README.md)

# MyUnity-tool-LevelFlowEditor

![image](https://i.imgur.com/vYl1hAi.png)

Tutorial video (click the image):

[![IMAGE ALT TEXT HERE](https://img.youtube.com/vi/YZwyQ-2FZbA/0.jpg)](https://www.youtube.com/watch?v=YZwyQ-2FZbA)

## Data :

Create data asset: `Create > Level > levelFlow`

Double click to open the editor windo.


## Tool Bar:

![toolBar](https://i.imgur.com/LqMjrbR.png)

- `Save / Load`

  Save/Load the data.
  
 - `Update current scene`
 
   If there is a corespond node of current scene on the graph view, it'll sample the gameobjects of type `ConnectPoint`, and project them onto level node.
  
   If node is not showing, please press `save` and `load` again.
  
- `Load all scene`

  Load all scenes on the graph view onto game scene.
  
- `Close scenes`

  Unload other previesing scenes.


  
  


## Node:

- `Start Node`

  ![startNode](https://i.imgur.com/hLUAiQT.png)

  - `name`
  
    A string to specified a enterence point.
  
    To choise a start point, call:  
  
  ```C#
  
   LevelFlowManager.LoadFromStart("StartPoint");
   
  ```

- `Level Node`

  ![startNode](https://i.imgur.com/LkGfQMy.png)

  - `[ Async | Sync ]`

    Load scene type in async or sync way.
    
    Async: Load in background.
    
    Sync: The game will wait for the scene to finish loading.
    
    
  - `[ Single | Additive ]`

    How the scene is going to add to current game.
    
    Single: Close other scene, load this scene and keep the main scene if `MainScene` is set.
    
    Additive: Add this scene to current game.



## API:

You don't need to instance the manager.

### `Load From Start`
```C#

public class LoadFirstSceneControl : MonoBehaviour
{
    public LevelMapSO data;
    private void Start()
    {
        //NOTE: You can do a check-point loading to 
        //load to the certain scene.
        LevelFlowManager.flowData = data;
        LevelFlowManager.LoadFromStart("StartPoint");
    }
}


```

### `Load Next`

```C#
public class MyPoint : ConnectPoint
{
	public void LoadNext(){
    		LevelFlowManager.LoadNextScene(portSetId);
    	}
}
```

### Event:
- `LevelFlowManager.cs`

  - `public static event Action<string> OnConnectPointEntered;`
 
     Called when finish loading scene. Return the `portSetGuid` of the matching enternce point gameObject.
	
     in demo: `RangeTriggerConnectPoint.cs`. 
    
	
	```C#
	public void OnEnable()
    {
        LevelFlowManager.OnConnectPointEntered += MovePlayerHere;
    }
	public void OnDisable()
    {
        LevelFlowManager.OnConnectPointEntered -= MovePlayerHere;
    }
	private void MovePlayerHere(string _enterPoint)
    {
        if (_enterPoint == portSetId)
        {
            GameObject.FindGameObjectWithTag("Player").transform.position = transform.position;
            leaveLock = true;
        }
    }
	
	```
