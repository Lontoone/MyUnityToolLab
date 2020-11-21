The main part are `RPG Core.cs`, `StoryReader.cs` ,`OptionButton.cs`.


# RPG Core.cs:

Contains regexs , in charge of spliting nesting tags and other basic funtions.

## Parameters

- lang

Add your own language mark at `public enum Lang`. It is used to sperate different language textfile folders.

- StoryRecord and temp_StoryRecord 

 Used to record custum variables. If variable start with `temp_`, then it is record into `temp_StoryRecord` and will be clear out while dialog end. Otherwise it is saved into `StoryRecord` and last the whole game. Keep in mind that you should write a script to save and load this parameter before you close or open the game.
 
 - ESC_CHAR
 
 I am not a regex pro, this is just a workaround to allow me typing `<` or `>` in the dialog.
 
 
 # Story Reader.cs:
 
 Controling UI. It reads tags, args texts and applying them to the UI.
 
 To start a conversation, simply call `StartConversation` function. see [StoryTrigger.cs](https://github.com/Lontoone/MyUnityToolLab/blob/master/RPG%20DialogSystem/StoryTrigger.cs) for example.
 
 - Add your own tag
 
 At `ReadArgs` funtuion, there is a place marked `YOUR OWN SCRIPT FUNCTION`. for example, `myFunc(v1,v2,v3,v4....)`,then `myFunc` is the `functionName` and parameters are splited by`,`.
