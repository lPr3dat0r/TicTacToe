﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Automation.Peers;
using System.Windows.Controls;
using System.Windows.Automation.Provider;
using NeuralNet;
using NeuralNet.NeuralNet;

namespace TicTacToe
{
    public class Player
    {
        public Player(bool isHuman, char marker)
        {
            IsHuman = isHuman;
            Marker = marker;
        }
        private int wins = 0;
        public int Wins { get {return wins; } set{ this.wins = value; } }

        private Board boardForComp = new Board();
        public Board BoardForComp { get { return this.boardForComp;} set { this.boardForComp = value; } }

        private bool isHuman;
        private char marker;
        private bool playersTurn = false;
        

        public bool PlayersTurn
        {
            get { return this.playersTurn; }
            set { this.playersTurn = value; }
        }

        public char Marker
        {
            get { return this.marker; }
            set { this.marker = value; } 
        }

        public bool IsHuman 
        { 
            get { return this.isHuman; }
            set { this.isHuman = value; }
        }

        private int countButtonsAvailable;
        private int lenghtOfList;
        private int randInt;
        private List<Button> buttonListAfterRemove = new List<Button>();

        private bool performAction(int bestAction, List<Button> buttonList)
        {
            if (buttonList[bestAction].IsEnabled == true)
            {
                ButtonAutomationPeer peer =
                new ButtonAutomationPeer(buttonList[bestAction]);
                IInvokeProvider invokeProv = peer.GetPattern(PatternInterface.Invoke) as IInvokeProvider;
                invokeProv.Invoke();
            }
            else
            {
                generateRandomComputerMove(buttonList);
            }
            return true;
        }

        public int generateRandomComputerMove(List<Button> buttonList)
        {
            Random random = new Random();
            for (int i = 0; i < buttonList.Count; i++)
            {
                if (buttonList[i].IsEnabled == true)
                {
                    countButtonsAvailable++;
                    buttonListAfterRemove.Add(buttonList.ElementAt(i));
                }

            }

            lenghtOfList = buttonListAfterRemove.Count;
            randInt = random.Next(0, lenghtOfList);

            string numberOfButtonClicked = buttonListAfterRemove[randInt].Name;//Um zu übergeben welcher button gedrückt wird;

            //MessageBox.Show(buttonListAfterRemove[randInt].Name);
            ButtonAutomationPeer peer =
            new ButtonAutomationPeer(buttonListAfterRemove[randInt]);
            IInvokeProvider invokeProv = peer.GetPattern(PatternInterface.Invoke) as IInvokeProvider;
            invokeProv.Invoke();

            buttonListAfterRemove = new List<Button>();
            //MessageBox.Show("random");
            
            return (int)Char.GetNumericValue(numberOfButtonClicked[3])-1;
        }

        public void generateSmartComputerMove()
        {
            boardForComp.nameToBeChanged();
        }

        
        public NeuralNetwork nn = new NeuralNetwork(25.5, new int[] { 9, 4, 9});
        public List<memPart> memory = new List<memPart>();
        public double[] runNN(List<double> input)
        {
            return nn.Run(input);
        }

        public void trainNN()
        {
            List<double> inputList = new List<double>();
            List<double> outputList = new List<double>();
            for (int i = 0; i < memory.Count; i++)
            {
                inputList.Clear();
                outputList.Clear();
                double[] input = new double[9];
                for (int m = 0; m < memory[i].boardInfo.Count; m++)
                {
                    input[m] = memory[i].boardInfo[m];
                }
                for (int outp = 0; outp < 9; outp++)
                {
                    for (int act = 0; act < 9; act++)
                    {
                        if (act == memory[i].action)
                        {
                            outputList.Add(memory[i].action);
                        }
                        else outputList.Add(0);
                    }
                }

                nn.Train(inputList, outputList);
            }
            
        }

        public void makeDecision(Board board, List<Button> buttonList)
        {
            List<double> boardInfoH = getBoardTOInputForNN(board);
            double[] output = nn.Run(boardInfoH);
            memPart memP = new memPart();
            memP.action = OutputToAction(output);
            memP.boardInfo = boardInfoH;
            memory.Add(memP);
            performAction(memP.action, buttonList);
        }

        public int RandMove(Board board, List<Button> buttonList)
        {
            memPart memP = new memPart();
            memP.action = generateRandomComputerMove(buttonList);
            memP.boardInfo = getBoardTOInputForNN(board);
            memory.Add(memP);
            
            return memP.action;
        }

        public List<double> getBoardTOInputForNN(Board board)
        {
            char[,] chArray = board.GameBoard;
            List<double> input = new List<double>();
            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    if (chArray[i, j] == Marker)
                    {
                        input.Add(10);
                    }
                    else if (chArray[i, j] == ' ')
                    {
                        input.Add(1);
                    }
                    else
                    {
                        input.Add(0);
                    }
                }
            }
            return input;
        }
        private int OutputToAction(double[] output)
        {
            double highestValue = output[0];
            int indexOfHV = 0;
            for (int i = 1; i < output.Length; i++)
            {
                if (output[i]>highestValue)
                {
                    highestValue = output[i];
                    indexOfHV = i;
                }
            }
            return indexOfHV;
        }
    }

    public struct memPart{
        public List<double> boardInfo;
        public int action;
    }

    
}
