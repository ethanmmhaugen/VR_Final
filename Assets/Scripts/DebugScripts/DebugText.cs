using System;
using System.Collections;
using System.Collections.Generic;
// using System.Diagnostics;
using UnityEngine;

public static class DebugText
{
    public static string TheText {get;private set;} = "";
    
    // Start is called before the first frame update
    public static void log(string newText) {
        
        string finalText = String.Concat(TheText,"\n",newText);
        int numLines = finalText.Split("\n").Length -1;
        
        if (numLines > 10) {

            string[] lines = finalText.Split("\n");

            string updatedString = lines[0];
            for (int i=2; i < lines.Length; i ++) {
                updatedString = String.Concat(updatedString,"\n",lines[i]);
            }
            TheText = updatedString;

        }
        else {
            TheText = finalText;
        }
    }
}
