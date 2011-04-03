﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MinGH.ChartImpl;
using MinGH.Config;
using MinGH.GameScreen;

namespace MinGH.Fretboard
{
    public interface IFretboard
    {
        void loadContent(GameConfiguration gameConfiguration, Texture2D laneSeparatorTexture, Texture2D hitMarkerTexture,
                                Effect effect, Matrix viewMatrix, Matrix projectionMatrix, int noteSpriteSheetSize,
                                GraphicsDeviceManager graphics, Game game);

        void unloadContent();

        void update(IKeyboardInputManager keyboardInputManager, Rectangle viewportRectangle,
                    GameConfiguration gameConfiguration, Effect effect, uint currentMsec,
                    GraphicsDeviceManager graphics, int noteSpriteSheetSize, GameTime gameTime,
                    float cameraYRotation);

        void draw(GraphicsDeviceManager graphics, Matrix viewMatrix, Matrix projectionMatrix);

        ChartInfo getChartInfo();

        PlayerInformation getPlayerInfo();

        //Vector3 position { get; set; }
    }
}