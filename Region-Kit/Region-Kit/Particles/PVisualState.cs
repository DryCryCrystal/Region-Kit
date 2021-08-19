﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RegionKit.Utils;
using UnityEngine;

namespace RegionKit.Particles
{
    public struct PVisualState
    {
        public string aElm;
        public string shader;
        public ContainerCodes container;
        public Color sCol;
        public Color lCol;
        public float lInt;
        public float lRadMax;
        public float lRadMin;
        public float affByDark;

        public PVisualState(string aElm, string shader, ContainerCodes container, Color sCol, Color lCol, float lInt, float lRadMax, float lRadMin, float affByDark)
        {
            this.aElm = aElm;
            this.shader = shader;
            this.container = container;
            this.sCol = sCol;
            this.lCol = lCol;
            this.lInt = lInt;
            this.lRadMax = lRadMax;
            this.lRadMin = lRadMin;
            this.affByDark = affByDark;
        }
    }
}