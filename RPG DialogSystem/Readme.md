The main part are `RPG Core.cs`, `StoryReader.cs` ,`OptionButton.cs`.
This tool is still in testing.

Example

![tags demo](https://i.imgur.com/kEKO6uk.png?1)

![Preview gif](https://i.imgur.com/znRKfdU.gif)

# How to use

Check out [this demo project.](https://github.com/Lontoone/RPGCore_demo) or watch the [demo video](https://youtu.be/urDFmhzHlts).
All the story condition checks are done by this tool [(example)](https://github.com/Lontoone/RPGCore_demo/blob/master/RPG_Demo/Assets/Resources/Story/EN/Kid.xml), other scripts only deal with player movement.

I use .xml as file extension only because it is easier to edit on vscode.

## Rules for typing tags.

>You can check out regexs at `RPG Core.cs`.

-Start by `<story>` and end with `</story>` tag.

-Add `\` before `<` and `>` if it is in the content.
```
EXAMPLE
<l > \<color=red\> Red content text! \</color\> </l>
```
---
### ```<l> </l>```

 `l` refers to line. It wraps dialog contents.
 
 ---
 
### ```<if> </if>```

```
Example

    <if(temp_a ==1)||(temp_a ==1)></if>
    
    <if (temp_a<=1) && (temp_b==2) && (temp_b\>=2) && (temp_a!=1)>
    
    <if(temp_a ==1)></if>
```

---

### ```<Select> </Select>```

Giving selections. Comes with `<opt> </opt>` tags.  *Can not use other tags in it.*

```
Example

 <select>
 
	<opt set("opt"=1) >  dialog content </opt>
 
	<opt set("opt"=2) > dialog content2 </opt>
 
	</select>

```

---

## Rules for Arguments


### animation
```
EXAMPLE
 animation(gameobjectName,clipName)
```
Will find gameobject with `gameobjectName` in scene and play `clipName` if aniamtor is attached.

---

### audio
```
EXAMPLE
 audio(gameobjectName, stop)
 audio(gameobjectName, play)
 audio(gameobjectName, pause)
```
Will find gameobject with `gameobjectName` in scene and set its audioSource's state.

---

### title
```
EXAMPLE
 title ="Jack"
```

Set the title.

---

### by
```
EXAMPLE
 by ="gameobjectname"
```
If you are using the style that dialog will show above the gameobject in scene, use it to find gameobject with the name `gameobjectname` .

---

### img
```
EXAMPLE

img(left,"img1")
img(middle,'img2')
img(right,/imgfolder/img1)
```
If you are using the style that similar to Janapens galgame, use it to load the picture under `Resources` folider and define which position to place. left,middle and right are just image variables in `StoryReader.cs`.


---

### set
```
EXAMPLE
        set(" temp_a"=1)
        set("temp_a"= "temp_b")
        set("temp_a"="temp_b" + "temp_a")
```
Set a variables, or create one if it is not defined.

If variable's name start with`temp_`, then it will be clear out after the dialog end. Otherwise it will last the whole game. Keep in mind that you should write a script to save and load `StoryRecord` in `RPGCore.cs` before you close or open the game.

---

### $[]
Read a variable's value.
```
EXAMPLE
$[key]
```

It is the same if you type it in the argument:
```
EXAMPLE
<select>
	 <opt set("temp_a"="temp_b") > set temp_a= b</opt>
	 <opt set("temp_a"=$[temp_c]) > set temp_a= c </opt>
	</select>
```

But you can type it in the dialog content.
```
EXAMPLE
<l> temp_a = $[temp_a]</l>
```

```
WON'T WORK
<l> temp_a = "temp_a"</l>
```

---

### Other

Any argument that doesn't match regex above also can be catched. Add your own argument function by following the rule:
```
functionName( arg1, arg2 ,arg3,arg4.......);
```
  At `ReadArgs` funtuion in `StoryReader.cs`, there is a place marked `YOUR OWN SCRIPT FUNCTION`. for example, `myFunc(v1,v2,v3,v4....)`,then `myFunc` is the `functionName` and parameters are splited by`,`.

---

Argments are not limited to one in one line!
```
EXAMPLE
 	<l title="Dog" 
        by="Dog"
        img(right,"Img/Portrait/p1")  
	effect(shake) 
        animation(obj1,clip_hi)
        animation(obj2,clip_ya)
        animation(obj3,Idle)
        > woof woof
	</l>
```

---


# RPG Core.cs:

Contains regexs , in charge of spliting nesting tags and other basic funtions.

## Parameters

- lang

> Add your own language mark at `public enum Lang`. It is used to separate different language textfile folders.

- StoryRecord and temp_StoryRecord 

> Used to record custum variables. If variable start with `temp_`, then it is record into `temp_StoryRecord` and will be clear out while dialog end. Otherwise it is saved into `StoryRecord` and last the whole game. Keep in mind that you should write a script to save and load this parameter before you close or open the game.
 
 - ESC_CHAR
 
 > I am not a regex pro, this is just a workaround to allow me typing `<` or `>` in the dialog.
 
 ---
 
 # Story Reader.cs:
 
 Controling UI. It reads tags, args texts and applying them to the UI.
 
 To start a conversation, simply call `StartConversation` function. see [StoryTrigger.cs](https://github.com/Lontoone/MyUnityToolLab/blob/master/RPG%20DialogSystem/StoryTrigger.cs) for example.
 
 ---
 
 # Story Trigger.cs:
 
Asign your defult lanuage dialog text file to ```TextAsset text```. While converstation start, it will find the file with same name as `text` under other `lang` folder.
