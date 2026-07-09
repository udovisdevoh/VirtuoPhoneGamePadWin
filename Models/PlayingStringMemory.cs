using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Drawing;

namespace VirtuoPhone.Models;

/**
* Remember which string is being played
* @author Guillaume Lacasse
*/
public class PlayingStringMemory
{
    private int[] stringStreamList;

    public PlayingStringMemory(int stringCount)
    {
        stringStreamList = new int[stringCount];
    }

    public void remember(int stringId, int streamId)
    {
        stringStreamList[stringId] = streamId;
    }

    public int getStreamIdFromString(int stringId)
    {
        return stringStreamList[stringId];
    }

}
