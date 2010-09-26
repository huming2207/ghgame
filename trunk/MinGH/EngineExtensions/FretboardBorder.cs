﻿using GameEngine;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MinGH.EngineExtensions
{
    /// <summary>
    /// The common abstract class that defines the borders of the fretboard.
    /// </summary>
    public abstract class FretboardBorder : GameObject
    {
        public FretboardBorder(Texture2D loadedTex, Rectangle spriteRect, Effect effectToUse, GraphicsDevice device)
            : base(loadedTex, effectToUse, device)
        { }

        public abstract void initalizeFretboardBorders(int laneSize, int fretboardBorderSize, int laneBorderSize, float laneDepth);
    }
}
