using System;
using System.Collections.Generic;
using UnityEngine;

public class Popup
{
    static int popupListHash = "PopupList".GetHashCode();
    private static string currentPort = "";
    // Delegate
    public delegate void ListCallBack();



    public static bool List(Rect position, ref bool showList, ref int listEntry, GUIContent buttonContent, List<string> list,
                             GUIStyle listStyle, ListCallBack callback, ref bool firstSelected)
    {



        return List(position, ref showList, ref listEntry, buttonContent, list, "button", "box", listStyle, callback, ref firstSelected);
    }

    public static bool List(Rect position, ref bool showList, ref int listEntry, GUIContent buttonContent, List<string> list,
                             GUIStyle buttonStyle, GUIStyle boxStyle, GUIStyle listStyle, ListCallBack callback, ref bool firstSelected)
    {

        if (!firstSelected)
        {
            currentPort = list[listEntry];
            firstSelected = true;
        }
        int controlID = GUIUtility.GetControlID(popupListHash, FocusType.Passive);
        bool done = false;
        switch (Event.current.GetTypeForControl(controlID))
        {
            case EventType.mouseDown:
                if (position.Contains(Event.current.mousePosition))
                {
                    buttonContent.text = "";
                    GUIUtility.hotControl = controlID;
                    showList = true;
                }
                break;
            case EventType.mouseUp:
                if (showList)
                {
                    done = true;
                    // Call our delegate method
                    callback();
                }
                break;
        }

        GUI.Label(new Rect(position.x, position.y, position.width, 20), currentPort, buttonStyle);
        Rect listRect = new Rect(position.x, position.y, position.width, list.Count * 20);
        if (showList)
        {
            currentPort = "";
            //GUI.Box(listRect, "", boxStyle);
            var text = list.ToArray();
            listEntry = GUI.SelectionGrid(listRect, listEntry, text, 1, listStyle);
        }
        if (done)
        {
            currentPort = list[listEntry];
            showList = false;
        }
        return done;
    }
}