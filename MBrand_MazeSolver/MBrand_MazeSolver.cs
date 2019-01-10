/*
Maze Solving Application for Gentrack

Created 10/01/2018 by Michael Brand

To execute application, run from command line passing path to file as argument e.g. MBrand_MazeSolver.cs "C:\mymaze.txt"
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;

namespace MBrand_MazeSolver
{
    class MBrand_MazeSolver
    {
        static string[] mazeDimensions;
        static string[] startingCoords;
        static string[] endingCoords;
        static string[ , ] theMaze;

        static void Main(string[] args)
        {
            //Check if any arguments were passed to the application
            if(args.Length == 0)
            {
                Console.WriteLine("Please pass file path for the desired maze");
                Console.WriteLine(" ");
                Console.WriteLine("Press any key to exit");
                Console.ReadKey();
            } else
            {
                //Store the path in a string
                string pathToFile = args[0];

                //Validate the provided file path
                if(!File.Exists(pathToFile))
                {
                    Console.WriteLine("The provided file path is not valid or does not exist");
                    Console.WriteLine(" ");
                    Console.WriteLine("Press any key to exit");
                    Console.ReadKey();
                    return;
                }

                //Generate a maze based on the file path provided
                theMaze = GenerateMaze(pathToFile);

                Console.WriteLine("The following maze has been generated from the file: \n");
                displayMaze(theMaze);
                Console.WriteLine("\nWill now attempt to solve the maze and return the result");

                //Method to solve the maze
                solveMaze(startingCoords, endingCoords);

                Console.WriteLine("\nPress any key to exit");
                Console.ReadKey();
            }
        }

        //This method will read the file from the provided file path and extract all the parameters to generate the maze in a 2-dimensional array
        static string[ , ] GenerateMaze(string filePath)
        {
            string[] mazeFile = File.ReadAllLines(filePath);

            //Get Maze Dimensions, Starting Co-ordinates and Ending Co-ordinates
            mazeDimensions = mazeFile[0].Split(' ');
            startingCoords = mazeFile[1].Split(' ');
            endingCoords = mazeFile[2].Split(' ');

            string[ , ] mazeLayout = new string[int.Parse(mazeDimensions[0]), int.Parse(mazeDimensions[1])];

            //Create maze in 2-dimensional array
            for (int row = 0; row < int.Parse(mazeDimensions[1]); row++)
            {
                for(int column = 0; column < int.Parse(mazeDimensions[0]); column++)
                {
                    //Currently at starting square which will be marked with 2
                    if(column == int.Parse(startingCoords[0]) && row == int.Parse(startingCoords[1]))
                    {
                        mazeLayout[column, row] = "2";
                        continue;
                    }

                    //Currently at ending square which will be marked with 3
                    if (column == int.Parse(endingCoords[0]) && row == int.Parse(endingCoords[1]))
                    {
                        mazeLayout[column, row] = "3";
                        continue;
                    }

                    //Normal square
                    mazeLayout[column, row] = mazeFile[row + 3].Replace(" ", "").ElementAt(column).ToString();
                }               
            }

            return mazeLayout;
        }

        //This method will display the maze on the screen
        static void displayMaze(string[,] mazeLayout)
        {
            for (int row = 0; row < int.Parse(mazeDimensions[1]); row++)
            {
                for (int column = 0; column < int.Parse(mazeDimensions[0]); column++)
                {
                    switch(mazeLayout[column, row])
                    {
                        case "0":
                            Console.Write(" ");
                            break;

                        case "1":
                            Console.Write("#");
                            break;

                        case "2":
                            Console.Write("S");
                            break;

                        case "3":
                            Console.Write("E");
                            break;

                        case "4":
                            Console.Write("X");
                            break;

                        default:
                            break;
                    }
                }
                Console.Write('\n');
            }
        }

        //This method will attempt to solve the maze based on the start and end co-ordinates provided
        static void solveMaze(string[] start, string[] end)
        {
            //This list will contain the direct route to the end
            List<int[]> solutionRoute = new List<int[]>();

            //This list will contain all routes taken to help avoid backtracking
            List<int[]> movesList = new List<int[]>();   
            
            //Stores the current position of the player
            int[] currentPos = {int.Parse(start[0]), int.Parse(start[1])};

            //Stores the start position of the player
            int[] startPos = {int.Parse(start[0]), int.Parse(start[1])};

            //Stores the end position
            int[] endPos = {int.Parse(end[0]), int.Parse(end[1])};

            //Stores the square that will be moved into after a move is made
            int[] newPos = new int[2];

            //The four allowed directions to move in
            string[] directions = { "N", "E", "S", "W" };

            //If backtracking is set to 1 then the moves won't be logged
            int backtracking = 0;

            //This value stores the current direction that the player is moving. Lined up with the indexes of the directions array above
            int x = 0;

            //How many directions have been attempted in the current square
            int failedAttempts = 0;

            //Counts up while failed attempts occur. Resets after a valid move
            int iterations = 1;

            //If the player makes a number of failed moves equal to the maze height * the maze width then it determines that the maze is unsolveable
            int failureCheckCount = int.Parse(mazeDimensions[0]) * int.Parse(mazeDimensions[1]);

            //Endless loop to try combinations
            while (true)
            {
                //If the failure count is reached then stop trying to solve the maze and deem it unsolveable
                if(iterations == failureCheckCount)
                {
                    Console.WriteLine("Maze is not solveable");
                    break;
                }

                //Reset the backtracking variable
                backtracking = 0;

                //4 failed attempts = one whole rotation in the same square
                if(failedAttempts >= 4)
                {
                    //Take this square out of the final solution route because it doesn't lead to the goal
                    for(int y = 0; y < solutionRoute.Count - 1; y++)
                    {
                        if(currentPos[0] == solutionRoute.ElementAt(y)[0] && currentPos[1] == solutionRoute.ElementAt(y)[1])
                        {
                            solutionRoute.RemoveAt(y);
                        }
                    }

                    //Set the player's position back until they get to a square where they can make a valid move again
                    if(iterations <= movesList.Count) {
                        currentPos = movesList.ElementAt(movesList.Count - iterations);
                    }                  
                }

                //If the player has made a full rotation of the directions then reset the x variable
                if(x >= directions.Length)
                {
                    x = 0;
                    iterations++;
                }
                
                //Execute the makeMove method to get the co-ordinates for the next position in the x direction
                newPos = makeMove(currentPos, directions[x]);

                //newPos will be returned as -1, -1 if the square is not valid to move into. In that case we try another direction and up the failed attempts count.
                if (newPos[0] == -1 && newPos[1] == -1)
                {
                    x++;
                    failedAttempts++;
                    continue;
                } else
                {                
                    //If we're in the end position square then we've solved the maze and can exit the loop
                    if(newPos[0] == endPos[0] && newPos[1] == endPos[1])
                    {
                        Console.WriteLine("Maze has been solved. Solution below: \n");
                        break;
                    }

                    //check for any backtracking. If so then we want to try other directions that don't backtrack
                    foreach(int[] arrayItem in movesList)
                    {
                        if(arrayItem[0] == newPos[0] && arrayItem[1] == newPos[1])
                        {                    
                            x++;
                            failedAttempts++;
                            backtracking = 1;
                            break;
                        }
                    }
                    
                    //If we aren't currently backtracking then add the new moves to the lists and set our current position as the new position
                    if(backtracking != 1) {                      
                        movesList.Add(newPos);
                        solutionRoute.Add(newPos);
                        currentPos = new List<int>(newPos).ToArray();
                        failedAttempts = 0;
                        backtracking = 0;
                        iterations = 1;
                    }
                }
            }

            //Draw out the solution route on the maze
            foreach (int[] arrayItem in solutionRoute)
            {          
                theMaze[arrayItem[0], arrayItem[1]] = "4";         
            }

            //Display the maze with the solution on it
            displayMaze(theMaze);
        }

        //Will try to move the player in the provided direction
        static int[] makeMove(int[] coords, string direction)
        {
            int[] newCoords = new List<int>(coords).ToArray(); 

            //Set co-ordinates accordingly based on the direction provided. Either "N", "E", "S" or "W"
            switch(direction)
            {
                case "N":
                    if(newCoords[1] <= 0)
                    {
                        newCoords[1] = int.Parse(mazeDimensions[1])-1;
                    } else
                    {
                        newCoords[1]--;
                    }                
                    break;

                case "E":
                    if (newCoords[0] == int.Parse(mazeDimensions[0])-1)
                    {
                        newCoords[0] = 0;
                    }
                    else
                    {
                        newCoords[0]++;
                    }           
                    break;

                case "S":
                    if (newCoords[1] == int.Parse(mazeDimensions[1])-1)
                    {
                        newCoords[1] = 0;
                    }
                    else
                    {
                        newCoords[1]++;
                    }
                    break;

                case "W":
                    if (newCoords[0] <= 0)
                    {
                        newCoords[0] = int.Parse(mazeDimensions[0])-1;
                    }
                    else
                    {
                        newCoords[0]--;
                    }
                    break;

                default:
                    newCoords[0] = 0;
                    newCoords[1] = 0;
                    break;
            }

            //This switch checks what value is at the new co-ordinate to see if it's a valid move. Returns -1, -1 if not.
            //0=floor, 1=wall, 2=start square, 3=end square, 4=solution route
            switch (theMaze[newCoords[0], newCoords[1]])
            {
                case "0":                
                    break;

                case "1":
                    newCoords[0] = -1;
                    newCoords[1] = -1;
                    break;

                case "2":               
                    break;

                case "3":
                    break;

                case "4":
                    break;

                default:
                    newCoords[0] = -1;
                    newCoords[1] = -1;
                    break;
            }

            //returns the new co-ordinates. Either valid or -1, -1
            return newCoords;
        }
    }
}
