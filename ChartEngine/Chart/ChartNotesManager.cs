﻿using System;
using System.IO;
using System.Text.RegularExpressions;

namespace ChartEngine.Chart
{
    /// <summary>
    /// A manager class that reads in all notes from a specific notechart within a *.chart file.
    /// </summary>
    class ChartNotesManager
    {
        /// <summary>
        /// Creates a notechart from the specified file and the actual charttype
        /// (i.e. ExpertSingle from Freebird.chart)
        /// </summary>
        /// <param name="chartSelection">
        /// The information on which particular notechart to use.
        /// </param>
        /// <returns>
        /// A filled out Notechart containing the needed information from the *.chart file
        /// </returns>
        public static Notes GenerateNotechartFromChart(ChartSelection chartSelection)
        {
            string chartname = chartSelection.difficulty + chartSelection.instrument;
            StreamReader inputStream = new StreamReader(chartSelection.chartPath);
            string inputString = inputStream.ReadToEnd();

            // Single out the specified section via regular expressions
            string pattern = Regex.Escape("[") + chartname + "]\\s*" + Regex.Escape("{") + "[^}]*";
            Match matched_section = Regex.Match(inputString, pattern);

            // Create the stream from the singled out section of the input string
            StringReader pattern_stream = new StringReader(matched_section.ToString());
            string current_line = "";
            string[] parsed_line;

            // Create the resulting notechart and prep for input
            Notes notechartToReturn = new Notes(chartSelection.difficulty, chartSelection.instrument);

            //If specific notechart is not found, return a generic one
            if (!(matched_section.Success))
            {
                notechartToReturn.notes.Add(new Note());
            }

            // Else, read in all the chart information
            else
            {
                int i = 0;
                while ((current_line = pattern_stream.ReadLine()) != null)
                {
                    // Trim and split the line to retrieve information
                    current_line = current_line.Trim();
                    parsed_line = current_line.Split(' ');

                    // If a valid note is found, add it to the list
                    if (parsed_line.Length == 5)
                    {
                        if (parsed_line[2] == "N")
                        {
                            if ((notechartToReturn.notes.Count > 0) &&
                                (Convert.ToUInt32(parsed_line[0]) == notechartToReturn.notes[notechartToReturn.notes.Count - 1].tickValue))
                            {
                                switch (Convert.ToInt32(parsed_line[3]))
                                {
                                    case 0:
                                        notechartToReturn.notes[notechartToReturn.notes.Count - 1].addNote(0);
                                        break;

                                    case 1:
                                        notechartToReturn.notes[notechartToReturn.notes.Count - 1].addNote(1);
                                        break;

                                    case 2:
                                        notechartToReturn.notes[notechartToReturn.notes.Count - 1].addNote(2);
                                        break;

                                    case 3:
                                        notechartToReturn.notes[notechartToReturn.notes.Count - 1].addNote(3);
                                        break;

                                    case 4:
                                        notechartToReturn.notes[notechartToReturn.notes.Count - 1].addNote(4);
                                        break;

                                    default:
                                        Console.WriteLine("ERROR: Invalid Note Detcted.  Skipping...");
                                        break;
                                }
                            }
                            else
                            {
                                // Find out which note the current line is, and add it to the respective list
                                switch (Convert.ToInt32(parsed_line[3]))
                                {
                                    case 0:
                                        notechartToReturn.notes.Add(new Note(Convert.ToUInt32(parsed_line[0]),
                                                                                  Convert.ToInt32(parsed_line[4]),
                                                                                  0));
                                        break;

                                    case 1:
                                        notechartToReturn.notes.Add(new Note(Convert.ToUInt32(parsed_line[0]),
                                                                                  Convert.ToInt32(parsed_line[4]),
                                                                                  1));
                                        break;

                                    case 2:
                                        notechartToReturn.notes.Add(new Note(Convert.ToUInt32(parsed_line[0]),
                                                                                  Convert.ToInt32(parsed_line[4]),
                                                                                  2));
                                        break;

                                    case 3:
                                        notechartToReturn.notes.Add(new Note(Convert.ToUInt32(parsed_line[0]),
                                                                                  Convert.ToInt32(parsed_line[4]),
                                                                                  3));
                                        break;

                                    case 4:
                                        notechartToReturn.notes.Add(new Note(Convert.ToUInt32(parsed_line[0]),
                                                                                  Convert.ToInt32(parsed_line[4]),
                                                                                  4));
                                        break;

                                    default:
                                        Console.WriteLine("ERROR: Invalid Note Detcted.  Skipping...");
                                        break;
                                }
                            }
                        }
                        // Also check for SP notes
                        else if (parsed_line[2] == "S")
                            notechartToReturn.SPNotes.Add(new Note(Convert.ToUInt32(parsed_line[0]),
                                                                        Convert.ToInt32(parsed_line[4]),
                                                                        5));
                    }
                    i++;
                }
            }

            // Close the string stream
            pattern_stream.Close();
            return notechartToReturn;
        }
    }
}
