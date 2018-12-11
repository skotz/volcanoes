﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Volcano.Game
{
    class VolcanoGame
    {
        public Board CurrentState { get; set; }

        public VolcanoGame()
        {
            CurrentState = new Board();
        }
        
        public void MakeMove(int tileIndex)
        {
            CurrentState.MakeMove(new Move(tileIndex, MoveType.SingleGrow));
        }
    }
}
