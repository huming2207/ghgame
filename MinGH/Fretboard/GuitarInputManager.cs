﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using MinGH.ChartImpl;
using MinGH.Config;
using MinGH.EngineExtensions;
using MinGH.Interfaces;

namespace MinGH.GameScreen.SinglePlayer
{
    /// <summary>
    /// Encompasses most of the logic for interpreting the user's input during a
    /// single player session via guitar.
    /// </summary>
    class GuitarInputManager : IInputManager
    {
        public Point processPlayerInput(Note[,] physicalNotes,
                                       NoteParticleEmitters noteParticleEmitters,
                                       HorizontalHitBox hitBox, PlayerInformation playerInformation,
                                       IKeyboardInputManager keyboardInputManager,
                                       Notechart inputNotechart)
        {
            if (playerInformation.HOPOState)
            {
                if (keyboardInputManager.getHighestHeldKey() != Keys.None)
                {
                    // Strums are ignored when the user is in the HOPO state (i.e. GH5 style)
                    return triggerInput(physicalNotes, noteParticleEmitters, hitBox, keyboardInputManager, playerInformation, inputNotechart, false);
                }
            }
            else
            {
                if (keyboardInputManager.keyIsHit(KeyboardConfiguration.upStrum) || keyboardInputManager.keyIsHit(KeyboardConfiguration.downStrum))
                {
                    return triggerInput(physicalNotes, noteParticleEmitters, hitBox, keyboardInputManager, playerInformation, inputNotechart, true);
                }
            }
            return new Point(0, 0);
        }

        /// <summary>
        /// Figures out whether the user missed, or hit a note.
        /// </summary>
        /// <param name="physicalNotes">The 2D array of drawable notes.</param>
        /// <param name="noteParticleExplosionEmitters">The project mercury explosion emitters.</param>
        /// <param name="hitBox">The current hit window.</param>
        /// <param name="keyboardInputManager">The current state of the keyboard.</param>
        /// <param name="playerInformation">The player's current status.</param>
        /// <param name="inputNotechart">The Notechart currently being played.</param>
        /// <param name="wasStrummed">Whether a strum was executed.</param>
        private static Point triggerInput(Note[,] physicalNotes,
                                         NoteParticleEmitters noteParticleExplosionEmitters,
                                         HorizontalHitBox hitBox, IKeyboardInputManager keyboardInputManager, 
                                         PlayerInformation playerInformation,
                                         Notechart inputNotechart, bool wasStrummed)
        {
            Vector3 currentCenterPoint = new Vector3();
            int farthestNoteIndex = -1;
            float farthestNoteDistance = -10000;
            
            Keys currentKey = keyboardInputManager.getHighestHeldKey();
            int hitNote = KeyboardConfiguration.getGuitarNumberFromKey(currentKey);

            // Scan just the physical notes that apply to the key the player hit
            for (int j = 0; j < physicalNotes.GetLength(1); j++)
            {
                if ((hitNote > -1) && (physicalNotes[hitNote, j].alive))
                {
                    currentCenterPoint = physicalNotes[hitNote, j].getCenterPosition();

                    // If the current physical note is alive and inside the hitbox...
                    if (hitBox.Contains(currentCenterPoint.Z))
                    {
                        // and has the farthest distance from the top
                        if (currentCenterPoint.Z >= farthestNoteDistance)
                        {
                            // set it to be the note to explode
                            farthestNoteDistance = currentCenterPoint.Z;
                            farthestNoteIndex = j;
                        }
                    }
                }
            }

            // If a note was found, process the players input.
            if (farthestNoteIndex != -1)
            {

                // Dont hit the note if the player was holding prior to passing the hit window's center
                // unless they strummed or explicitly hit the note (i.e. he hammered on too early)
                if (keyboardInputManager.keyIsHit(currentKey) ||
                   (farthestNoteDistance > -hitBox.centerLocation) ||
                   (wasStrummed))
                {
                    // If true, we hit a chord...
                    if (physicalNotes[hitNote, farthestNoteIndex].isChordStart)
                    {
                        int chordDegree = 1;
                        Point currentRoot = new Point(hitNote, farthestNoteIndex);
                        NoteType chordToHit = new NoteType();
                        NoteType chordYouHit = new NoteType();

                        // Scan backwards and add every note within the chord to the chordToHit variable.
                        while (currentRoot != new Point(-1, -1))
                        {
                            // The X value of a "root" note is the same as the numerical type
                            // (i.e. 0->green, 4->orange).
                            switch (KeyboardConfiguration.getGuitarKeyFromNumber(currentRoot.X))
                            {
                                case KeyboardConfiguration.greenFret:
                                    chordToHit.Green = true;
                                    break;
                                case KeyboardConfiguration.redFret:
                                    chordToHit.Red = true;
                                    break;
                                case KeyboardConfiguration.yellowFret:
                                    chordToHit.Yellow = true;
                                    break;
                                case KeyboardConfiguration.blueFret:
                                    chordToHit.Blue = true;
                                    break;
                                case KeyboardConfiguration.orangeFret:
                                    chordToHit.Orange = true;
                                    break;
                            }
                            currentRoot = physicalNotes[currentRoot.X, currentRoot.Y].rootNote;
                            chordDegree++;
                        }

                        // Now create a noteType variable (noteYouHit) from what the player has held down
                        // at this point in time.
                        for (int i = 0; i < 5; i++)
                        {
                            if (keyboardInputManager.keyIsHeld(KeyboardConfiguration.getGuitarKeyFromNumber(i)))
                            {
                                switch (i)
                                {
                                    case 0:
                                        chordYouHit.Green = true;
                                        break;
                                    case 1:
                                        chordYouHit.Red = true;
                                        break;
                                    case 2:
                                        chordYouHit.Yellow = true;
                                        break;
                                    case 3:
                                        chordYouHit.Blue = true;
                                        break;
                                    case 4:
                                        chordYouHit.Orange = true;
                                        break;
                                }
                            }
                        }

                        // If the user is holding the proper buttons, then explode the note...
                        if (chordToHit.isEqual(chordYouHit))
                        {
                            // Apply the miss penalty to every note past the one the user hit,
                            // and kill them, but do not break the combo.
                            foreach (Note currNote in physicalNotes)
                            {
                                if ((currNote.getCenterPosition().Y > physicalNotes[hitNote, farthestNoteIndex].getCenterPosition().Y) &&
                                    (currNote.alive == true))
                                {
                                    currNote.alive = false;
                                    playerInformation.missNote();
                                }
                            }

                            noteParticleExplosionEmitters.emitterList[hitNote].Trigger(noteParticleExplosionEmitters.explosionLocations[hitNote]);
                            physicalNotes[hitNote, farthestNoteIndex].alive = false;

                            currentRoot = physicalNotes[hitNote, farthestNoteIndex].rootNote;
                            while (currentRoot != new Point(-1, -1))
                            {
                                noteParticleExplosionEmitters.emitterList[currentRoot.X].Trigger(noteParticleExplosionEmitters.explosionLocations[currentRoot.X]);
                                physicalNotes[currentRoot.X, currentRoot.Y].alive = false;
                                currentRoot = physicalNotes[currentRoot.X, currentRoot.Y].rootNote;
                            }

                            if (physicalNotes[hitNote, farthestNoteIndex].precedsHOPO)
                            {
                                playerInformation.hitNote(true, Note2D.pointValue * chordDegree);
                            }
                            else
                            {
                                playerInformation.hitNote(false, Note2D.pointValue * chordDegree);
                            }
                        }
                        // Calculate how far the hit note is from the center of the hitbox and return
                        return new Point(1, (int)(physicalNotes[hitNote, farthestNoteIndex].position3D.Z + hitBox.centerLocation));
                    }

                    // If true, we tried to hit a chord, but did not hit the highest note
                    else if (physicalNotes[hitNote, farthestNoteIndex].isPartOfChord)
                    {
                        playerInformation.missNote();
                    }

                    // If we get here, we hit a single note
                    else
                    {
                        // Apply the miss penalty to every note past the one the user hit,
                        // and kill them, but do not break the combo.
                        foreach (Note currNote in physicalNotes)
                        {
                            if ((currNote.getCenterPosition().Z > physicalNotes[hitNote, farthestNoteIndex].getCenterPosition().Z) && 
                                (currNote.alive == true))
                            {
                                currNote.alive = false;
                                playerInformation.missNote();
                            }
                        }

                        noteParticleExplosionEmitters.emitterList[hitNote].Trigger(noteParticleExplosionEmitters.explosionLocations[hitNote]);
                        physicalNotes[hitNote, farthestNoteIndex].alive = false;

                        if (physicalNotes[hitNote, farthestNoteIndex].precedsHOPO)
                        {
                            playerInformation.hitNote(true, physicalNotes[hitNote, farthestNoteIndex].pointValue);
                        }
                        else
                        {
                            playerInformation.hitNote(false, physicalNotes[hitNote, farthestNoteIndex].pointValue);
                        }

                        // Calculate how far the hit note is from the center of the hitbox and return
                        return new Point(1, (int)(physicalNotes[hitNote, farthestNoteIndex].position3D.Z + hitBox.centerLocation));
                    }
                }
            }
            else
            {
                // Only miss if the player strummed and is NOT in a hopo state (or was in A HOPO state)
                if (wasStrummed && !playerInformation.HOPOState && !playerInformation.leftHOPOState)
                {
                    playerInformation.missNote();
                }
            }
            return new Point(0, 0);
        }
    }
}