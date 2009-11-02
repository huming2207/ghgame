﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GH_Game.Chart
{
    class ChartInfo
    {
        // Default Constructor
        public ChartInfo()
        {
            songName = "default";
            artistName = "default";
            offset = 0.0f;
        }

        // Various chart info variables
        public string songName;  // The name of the associated song
        public string artistName;  // The name of the assiciated artist
        public float offset;  // Stores the offset of the chart
    }
}
