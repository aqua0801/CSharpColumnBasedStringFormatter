# CSharpColumnBasedStringFormatter
<pre> ``` 
A C# class that aims at column-based string formatting. Including aligning ,algorithm selection ,padding-space ... 

Example usage :

var sf = new StringFormattingObjects.StringFormatter(
    StringFormattingObjects.StringFormatter.FormattingAlgorithm.Fixed,
    StringFormattingObjects.StringFormatter.PadAlign.Center,
    100
    );
sf.PaddingSpace = 2;

sf.AddStringTemps("Hello", 0);
sf.AddStringTemps("World", 1);
sf.AddStringTemps("!", 2);
sf.NewLine();
sf.AddSeparator(StringFormattingObjects.StringFormatter.Separator.Major);
sf.AddStringTemps("This", 0);
sf.AddStringTemps("is", 1);
sf.AddStringTemps("made", 2);
sf.AddStringTemps("for", 3);
sf.AddStringTemps("string", 4);
sf.AddStringTemps("formatting", 5);
sf.NewLine();
sf.AddSeparator(StringFormattingObjects.StringFormatter.Separator.Minor);
sf.AddStringTemps("!!", 0);
sf.AddStringTemps("@", 1);
sf.AddStringTemps("?!#", 2);
sf.AddStringTemps("--", 3);
sf.AddStringTemps("peko", 4);
sf.AddStringTemps(">:(", 5);

sf.NewLine();
sf.AddStringTemps("Aqua my waifu !");
Debug.WriteLine(sf.ToFormattedString());

/* 
Result 
(PadAlign.Left)

  Hello  World     !
=============================================
   This     is  made  for  string  formatting
---------------------------------------------
     !!      @   ?!#   --    peko         >:(
Aqua my waifu !

(PadAlign.Center)

 Hello  World   !  
=============================================
 This    is    made  for  string  formatting
---------------------------------------------
  !!      @    ?!#   --    peko      >:(    
Aqua my waifu !

(PadAlign.Right)

Hello  World  !    
=============================================
This   is     made  for  string  formatting 
---------------------------------------------
!!     @      ?!#   --   peko    >:(        
Aqua my waifu !

*/

``` </pre>



