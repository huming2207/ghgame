﻿using System;
using GameEngine.FMOD;
using GameEngine.GameStringImpl;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MinGH.ChartImpl;
using MinGH.Config;
using MinGH.EngineExtensions;
using MinGH.Enum;
using ProjectMercury.Emitters;
using ProjectMercury.Modifiers;
using ProjectMercury.Renderers;

namespace MinGH.GameScreen.SinglePlayer
{
    /// <summary>
    /// Contains all functionality and data to display a single player session of MinGH.
    /// </summary>
    class SinglePlayerScreen : DrawableGameComponent
    {
        MinGHMain gameReference;  // A reference to the game itself, allows for game state changing.
        GraphicsDeviceManager graphics;
        ChartSelection chartSelection;
        GameConfiguration gameConfiguration;
        SpriteBatch spriteBatch;  // Draws the shapes
        Rectangle viewportRectangle;  // The window itself
        Texture2D backgroundTex;  // The background texture
        Texture2D spriteSheetTex;
        SpriteFont gameFont;  // The font the game will use
        Note[,] Notes;  // Will hold every note currently on the screen
        const int maxNotesOnscreen = 50;  // Maximum amount of a single note (i.e. how many reds per frame)
        int noteIterator;  // These iterators are used to keep track of which note to observe next

        // Sprite Sheet Variables
        const int noteSpriteSheetSize = 100;  // How large each sprite is in the spritesheet (including the offset padding)
        float noteScaleValue = 0.0f;
        
        // Variables unique to this game screen
        NoteUpdater noteUpdater = new NoteUpdater();
        IKeyboardInputManager keyboardInputManager = new KeyboardInputManager();
        IInputManager inputManager;
        HorizontalHitBox hitBox;
        PlayerInformation playerInformation = new PlayerInformation();

        Chart mainChart;  // Create the chart file
        GameStringManager strManager = new GameStringManager();  // Stores each string and its position on the screen

        // Variables related to the audio playing and note syncing
        private GameEngine.FMOD.System system = new GameEngine.FMOD.System();
        private GameEngine.FMOD.Channel musicChannel = new GameEngine.FMOD.Channel();
        private GameEngine.FMOD.Channel guitarChannel = new GameEngine.FMOD.Channel();
        private GameEngine.FMOD.Channel bassChannel = new GameEngine.FMOD.Channel();
        private GameEngine.FMOD.Channel drumChannel = new GameEngine.FMOD.Channel();
        private GameEngine.FMOD.Sound musicStream = new GameEngine.FMOD.Sound();
        private GameEngine.FMOD.Sound guitarStream = new GameEngine.FMOD.Sound();
        private GameEngine.FMOD.Sound bassStream = new GameEngine.FMOD.Sound();
        private GameEngine.FMOD.Sound drumStream = new GameEngine.FMOD.Sound();
        RESULT result = new RESULT();
        uint currentMsec = 0;
        bool audioIsPlaying = false;  // So we don't play the song again every single update

        // Project Mercury Particle Engine related variables
        NoteParticleEmitters noteParticleExplosionEmitters = new NoteParticleEmitters();
        PointSpriteRenderer renderer = new PointSpriteRenderer();
        ColorModifier modifier = new ColorModifier();

        public SinglePlayerScreen(MinGHMain game, GraphicsDeviceManager graph, ChartSelection inputLocation)
            : base(game)
        {
            gameReference = game;
            graphics = graph;
            chartSelection = inputLocation;
        }

        public override void Initialize()
        {
            // Setup the strings
            strManager =  SinglePlayerStringInitializer.initializeStrings(graphics.GraphicsDevice.Viewport.Width,
                                                graphics.GraphicsDevice.Viewport.Height);
            // Initialize some variables
            noteIterator = 0;
            Notes = new Note[5, maxNotesOnscreen];

            // Create the sprite bacth
            spriteBatch = new SpriteBatch(graphics.GraphicsDevice);

            gameConfiguration = new GameConfiguration("./config.xml");

            noteScaleValue = gameConfiguration.themeSetting.laneSize / (float)noteSpriteSheetSize;

            if (gameConfiguration.useDrumStyleInputForGuitarMode)
            {
                inputManager = new DrumInputManager();
            }
            else
            {
                inputManager = new GuitarInputManager();
            }

            // Create the hitbox
            hitBox = new HorizontalHitBox(new Rectangle(0, 0,
                                          graphics.GraphicsDevice.Viewport.Width,
                                          graphics.GraphicsDevice.Viewport.Height),
                                          gameConfiguration.speedModValue);
            base.Initialize();
        }

        protected override void LoadContent()
        {
            gameFont = Game.Content.Load<SpriteFont>("Arial");  // Load the font
            mainChart = new Chart(chartSelection);

            string backgroundFilename = "";
            spriteSheetTex = Texture2D.FromFile(graphics.GraphicsDevice, gameConfiguration.themeSetting.noteSkinFile);

            if (mainChart.noteCharts[0].instrument == "Drums")
            {
                // Set up the particle explosions
                noteParticleExplosionEmitters.initalizeEmittersDrumsSingle(gameConfiguration.themeSetting);
                noteParticleExplosionEmitters.initializeLocationsDrumsSingle(gameConfiguration.themeSetting, hitBox.centerLocation);

                backgroundFilename = "DrumsSingle.png";

                Notes = NoteInitializer.InitializeNotesDrumSingle(noteSpriteSheetSize, Notes, spriteSheetTex, gameConfiguration);
            }
            else
            {
                // Set up the particle explosions
                noteParticleExplosionEmitters.initalizeEmittersGuitarSingle();
                noteParticleExplosionEmitters.initializeLocationsGuitarSingle(gameConfiguration.themeSetting, hitBox.centerLocation);

                backgroundFilename = "GuitarSingle.png";

                Notes = NoteInitializer.InitializeNotesGuitarSingle(noteSpriteSheetSize, Notes, spriteSheetTex, gameConfiguration);
            }

            foreach (Emitter emitter in noteParticleExplosionEmitters.emitterList)
            {
                emitter.Initialize();
            }

            backgroundTex = Texture2D.FromFile(graphics.GraphicsDevice,
                                gameConfiguration.themeSetting.backgroundDirectory + "\\" + backgroundFilename);

            // Add the "Song Title" and "Artist Name" to the string manager
            string songInformation = "Song Title:\n" + mainChart.chartInfo.songName + "\n\n" +
                                     "Artist Name:\n" + mainChart.chartInfo.artistName + "\n\n" +
                                     "Instrument:\n" + mainChart.noteCharts[0].instrument + "\n\n" +
                                     "Difficulty:\n" + mainChart.noteCharts[0].difficulty;
            strManager.SetString(2, songInformation);
            
            // Setup the window
            viewportRectangle = new Rectangle(0, 0,
                graphics.GraphicsDevice.Viewport.Width,
                graphics.GraphicsDevice.Viewport.Height);
            
            renderer.GraphicsDeviceService = graphics;
            renderer.BlendMode = SpriteBlendMode.Additive;
            renderer.LoadContent(Game.Content);

            foreach (Emitter emitter in noteParticleExplosionEmitters.emitterList)
            {
                emitter.LoadContent(Game.Content);
            }

            base.LoadContent();
        }

        protected override void UnloadContent()
        {
            system.close();
        }

        public override void Update(GameTime gameTime)
        {
            // Start the song immediately
            if (audioIsPlaying == false)
            {
                GameEngine.FMOD.Factory.System_Create(ref system);
                system.init(32, INITFLAGS.NORMAL, (IntPtr)null);

                // Load up the audio tracks.
                // NOTE: I assume the audio tracks for midi files are lowercase (e.g. song.ogg)
                //       so if a file is not found, I also assume it is cause the audio track is
                //       capatalized (e.g. Song.ogg).  
                if (mainChart.chartInfo.musicStream != null)
                {
                    string musicLocation = chartSelection.directory + "\\" + mainChart.chartInfo.musicStream;
                    result = system.createStream(musicLocation, MODE.CREATESTREAM, ref musicStream);
                    if (result == RESULT.ERR_FILE_NOTFOUND)
                    {
                        result = system.createStream(chartSelection.directory + "\\Song.ogg", MODE.HARDWARE, ref musicStream);
                    }
                }
                if (mainChart.chartInfo.guitarStream != null)
                {
                    string guitarLocation = chartSelection.directory + "\\" + mainChart.chartInfo.guitarStream;
                    result = system.createStream(guitarLocation, MODE.CREATESTREAM, ref guitarStream);
                    if (result == RESULT.ERR_FILE_NOTFOUND)
                    {
                        result = system.createStream(chartSelection.directory + "\\Guitar.ogg", MODE.CREATESTREAM, ref guitarStream);
                    }
                }
                if (mainChart.chartInfo.bassStream != null)
                {
                    string bassLocation = chartSelection.directory + "\\" + mainChart.chartInfo.bassStream;
                    result = system.createStream(bassLocation, MODE.CREATESTREAM, ref bassStream);
                    if (result == RESULT.ERR_FILE_NOTFOUND)
                    {
                        result = system.createStream(chartSelection.directory + "\\Rhythm.ogg", MODE.CREATESTREAM, ref bassStream);
                    }
                }
                if (mainChart.chartInfo.drumStream != null)
                {
                    string drumLocation = chartSelection.directory + "\\" + mainChart.chartInfo.drumStream;
                    result = system.createStream(drumLocation, MODE.CREATESTREAM, ref drumStream);
                    if (result == RESULT.ERR_FILE_NOTFOUND)
                    {
                        result = system.createStream(chartSelection.directory + "\\Drums.ogg", MODE.CREATESTREAM, ref drumStream);
                    }
                }

                result = system.playSound(CHANNELINDEX.FREE, musicStream, false, ref musicChannel);
                result = system.playSound(CHANNELINDEX.FREE, guitarStream, false, ref guitarChannel);
                result = system.playSound(CHANNELINDEX.FREE, bassStream, false, ref bassChannel);
                result = system.playSound(CHANNELINDEX.FREE, drumStream, false, ref drumChannel);

                audioIsPlaying = true;
            }

            if (keyboardInputManager.keyIsHit(Keys.Escape))
            {
                UnloadContent();
                gameReference.ChangeGameState(GameStateEnum.MainMenu, null);
            }

            musicChannel.getPosition(ref currentMsec, TIMEUNIT.MS);

            // Update the FMOD system
            system.update();

            // Get the current keyboard state
            keyboardInputManager.processKeyboardState(Keyboard.GetState());

            // The distance each note must step to be in sync with this current update
            float currStep = (float)(gameTime.ElapsedGameTime.TotalMilliseconds * gameConfiguration.speedModValue.noteVelocityMultiplier);
            
            inputManager.processPlayerInput(Notes, noteParticleExplosionEmitters, hitBox,
                                                  playerInformation, keyboardInputManager,
                                                  mainChart.noteCharts[0]);

            NoteUpdater.updateNotes(mainChart.noteCharts[0], ref noteIterator, Notes, viewportRectangle,
                                    currStep, gameConfiguration.themeSetting,
                                    currentMsec + gameConfiguration.speedModValue.milisecondOffset,
                                    noteSpriteSheetSize, playerInformation, hitBox);

            // Update varous strings
            strManager.SetString(0, "Current MSEC:\n" + currentMsec);
            strManager.SetString(1, "End MSEC:\n" + mainChart.chartInfo.chartLengthMiliseconds.ToString());
            strManager.SetString(3, "Score: " + playerInformation.currentScore.ToString() + "\n\n" +
                                     "Multiplier : " + playerInformation.currentMultiplier.ToString() + "\n\n" +
                                     "Combo :" + playerInformation.currentCombo.ToString());
            strManager.SetString(4, "Health: " + playerInformation.currentHealth.ToString());

            // Stop playing music when chart is over or when the screen goes inactive
            if (currentMsec > mainChart.chartInfo.chartLengthMiliseconds)
            {
                musicChannel.stop();
                bassChannel.stop();
                guitarChannel.stop();
            }

            // Update every particle explosion
            foreach (Emitter emitter in noteParticleExplosionEmitters.emitterList)
            {
                emitter.Update((float)gameTime.TotalGameTime.TotalSeconds, (float)gameTime.ElapsedGameTime.TotalSeconds);
            }

            base.Update(gameTime);
        }

        public override void Draw(GameTime gameTime)
        {
            spriteBatch.Begin(SpriteBlendMode.AlphaBlend);

            //Draw the background
            spriteBatch.Draw(backgroundTex, viewportRectangle, Color.White);

            // Draw every string in str_manager
            strManager.DrawStrings(spriteBatch, gameFont);

            //Draw the notes
            for (int i = 0; i < Notes.GetLength(0); i++)
            {
                for (int j = 0; j < Notes.GetLength(1); j++)
                {
                    if (Notes[i, j].alive)
                    {
                        spriteBatch.Draw(Notes[i, j].spriteSheet, Notes[i, j].position,
                                         Notes[i, j].spriteSheetRectangle, Color.White,
                                         Notes[i, j].rotation, new Vector2(0, 0), noteScaleValue, SpriteEffects.None,
                                         0f);
                        //spriteBatch.Draw(Notes[i, j].spriteSheet, Notes[i, j].position, Notes[i, j].spriteSheetRectangle, Color.White);
                    }
                }
            }

            spriteBatch.End();

            foreach (Emitter emitter in noteParticleExplosionEmitters.emitterList)
            {
                renderer.RenderEmitter(emitter);
            }

            base.Draw(gameTime);
        }
    }
}
