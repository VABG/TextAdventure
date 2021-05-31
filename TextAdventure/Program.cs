using System;
using System.Collections.Generic;
using System.IO;

namespace TextAdventure
{
    class Node
    {
        public string text;
        public List<Choice> choices;
        //public string Item;
        public int ID;
    }

    class Choice
    {
        public string text;
        public int nodeID;
        public Node node;
    }

    class Program
    {
        static List<Node> choices;

        static void Main(string[] args)
        {
            //Menu
            while (true)
            {
                // Try to load a file
                Console.WriteLine("Please input an adventure file name:");
                string s = Console.ReadLine();
                if (s == "quit" || s == "exit") return;

                //Read adventure
                if (File.Exists(s))
                {
                    string[] adventure;
                    Console.Clear();
                    adventure = File.ReadAllLines(s);

                    if (ParseAdventure(adventure, out choices))
                    {
                        //Start game
                        Game();
                    }
                    else
                    {
                        Console.WriteLine("Errors found, game not started! Press any key to restart!");
                        Console.ReadKey();
                    }
                }
                else
                {
                    Console.WriteLine("Adventure called '" + s + "' not found!");
                }
            }

        }

        static void Game()
        {
            Node currentNode = choices[0];
            while (currentNode.choices.Count > 0)
            {
                Console.WriteLine(currentNode.text);
                for (int i = 0; i < currentNode.choices.Count; i++)
                {
                    Console.WriteLine((i + 1) + ". " + currentNode.choices[i].text);
                }
                while (true)
                {
                    string c = Console.ReadKey().KeyChar.ToString();
                    if (Char.IsDigit(c[0]))
                    {
                        int.TryParse(c, out int cInt);
                        if (cInt > 0 && cInt <= currentNode.choices.Count)
                        {
                            currentNode = currentNode.choices[cInt - 1].node;
                            Console.WriteLine();
                            break;
                        }
                    }
                    else
                    {
                        Console.WriteLine("Invalid input!");
                    }
                }
            }
            // Write last message and game over
            Console.WriteLine('\n' + currentNode.text);
            Console.WriteLine("\nGame Over!");
        }

        static bool ParseAdventure(string[] a, out List<Node> adventure)
        {
            List<Choice> nodeChoices = new List<Choice>();
            List<Node> nodes = new List<Node>();
            Node currentNode = null;
            int nodeID = -1;
            string nodeText = "";
            bool noErrors = true;

            for (int i = 0; i < a.Length; i++)
            {
                string s = a[i];
                //Trim string just in case
                s = s.Trim();
                // If empty, continue
                if (s.Length == 0) continue;
                //Check for #
                if (s[0] == '#')
                {
                    // See if empty
                    if (currentNode == null)
                    {
                        currentNode = new Node();
                    }
                    else
                    {
                        //Set values
                        currentNode.text = nodeText;
                        if (nodeChoices.Count < 1)
                        {
                            //TODO: Make sure this is an end node.
                        }
                        currentNode.choices = nodeChoices;
                        if (nodeID == -1)
                        {
                            Console.WriteLine("Error! No node ID found!");
                            noErrors = false;
                        }
                        currentNode.ID = nodeID;
                        // Add node to list if not empty(only empty in start)
                        nodes.Add(currentNode);

                        nodeText = "";
                        nodeID = -1;
                        nodeChoices = new List<Choice>();
                        currentNode = new Node();
                    }

                    // Get node ID
                    if (int.TryParse(s[1..], out int pOut))
                    {
                        nodeID = pOut;
                    }
                    else
                    {
                        Console.Write("Error! Bad data on row " + i + "! Check node ID.");
                        noErrors = false;
                    }
                    continue;
                }

                if (s[0] == '|')
                {
                    if (nodeText != "")
                    {
                        // new line + additional text
                        nodeText += '\n' + s[1..].Trim();
                    }
                    else
                    {
                        nodeText = s[1..].Trim();
                    }
                    continue;
                }

                if (s[0] == '-')
                {
                    Choice c = new Choice();
                    string[] sSplit = GetFirstIntAndSplitAfter(s);
                    c.nodeID = int.Parse(sSplit[0]);
                    c.text = sSplit[1];

                    //Add to choices
                    nodeChoices.Add(c);
                    if (c.nodeID == -1)
                    {
                        Console.WriteLine("Error, bad data @ row " + i + "! No choice node ID.");
                        noErrors = false;
                    }
                    continue;
                }
            }

            // Add last node to list
            currentNode.choices = nodeChoices;
            currentNode.ID = nodeID;
            currentNode.text = nodeText;

            nodes.Add(currentNode);

            // Loop through nodes
            for (int i = 0; i < nodes.Count; i++)
            {
                // Loop through choices
                for (int j = 0; j < nodes[i].choices.Count; j++)
                {
                    // Check what node to look for
                    int findID = nodes[i].choices[j].nodeID;
                    bool foundID = false;
                    for (int k = 0; k < nodes.Count; k++)
                    {
                        if (nodes[k].ID == findID)
                        {
                            nodes[i].choices[j].node = nodes[k];
                            foundID = true;
                            continue;
                        }
                    }
                    if (!foundID)
                    {
                        Console.WriteLine("Error! No node nr: " + findID + " in Nodes for Node " + nodes[i].ID + " choice " + j);
                        noErrors = false;
                    }
                }
            }
            // Output results
            adventure = nodes;
            // Return success/fail
            return noErrors;
        }

        static string[] GetFirstIntAndSplitAfter(string s)
        {
            int start = -1;
            int end = -1;
            for (int i = 0; i < s.Length; i++)
            {
                if (Char.IsDigit(s[i]))
                {
                    if (start == -1) start = i;
                    else end = i;
                    continue;
                }
                else if (start != -1)
                {
                    end = i;
                    break;
                }
            }

            if (start == -1 || end == -1)
            {
                return null;
            }
            else
            {
                string[] strings = new string[2];
                strings[0] = s[start..end];
                strings[1] = s[end..].Trim();

                return strings;
            }
        }
    }

}
