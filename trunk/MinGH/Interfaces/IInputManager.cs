﻿using MinGH.ChartImpl;
using MinGH.EngineExtensions;
using MinGH.GameScreen.SinglePlayer;
using MinGH.GameScreen;

namespace MinGH.Interfaces
{
    public interface IInputManager
    {
        /// <summary>
        /// Uses the current keyboard state to figure out whether the user attempted to
        /// hit a note or not.
        /// </summary>
        /// <param name="physicalNotes">The 2D array of drawable notes.</param>
        /// <param name="noteParticleExplosionEmitters">The project mercury explosion emitters.</param>
        /// <param name="hitBox">The current hit window.</param>
        /// <param name="playerInformation">The player's current status.</param>
        /// <param name="keyboardInputManager">The current state of the keyboard.</param>
        /// <param name="inputNotechart">The Notechart currently being played.</param>
        void processPlayerInput(Note[,] physicalNotes,
                                NoteParticleEmitters noteParticleEmitters,
                                HorizontalHitBox hitBox, PlayerInformation playerInformation,
                                IKeyboardInputManager keyboardInputManager,
                                Notechart inputNotechart);
    }
}