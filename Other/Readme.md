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


## ScreenObjPicker.cs and PickableObject.cs

將ScreenObjPicker.cs 掛在場景中的任一物件上, 即可選取掛有PickableObject元件的物件.

此腳本基本上是為了提供從screneView轉換至world position的作法。

![image](https://i.imgur.com/3Kh0b1P.gif)

### 用法:

如果想移動已選取的物件，請先在物件上掛NavMeshAgent元件，並在場景準備好NavMeshSurface。不然可以直接刪除void SelectPath()方法，或重寫自己的移動方法。

ScreenObjPicker腳本建議搭配PickableObject使用，因為PickableObject提供可覆寫的方法，方便操作物件有自己的屬性。

