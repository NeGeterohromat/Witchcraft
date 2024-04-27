using System;
using System.Data.Common;

namespace PixelRPG
{
	public class GameVisual
	{
		private GameModel game;
		public const int ViewFieldSize = 20;
		private WorldElement[,] CurrentWorldView = new WorldElement[ViewFieldSize, ViewFieldSize];

        public GameVisual(GameModel game)
		{
			this.game = game;
		}

		public event Action<int,int,WorldElement,Player> ChangeOneCellView;
		public event Action<int,WorldElement> ChangeInventoryCellView;

        public void ChangeInventoryCell(int number, WorldElement cell) =>
			ChangeInventoryCellView(number, cell);
		

		public void ChangeOneCellByWorldCoords(int row, int column, WorldElement cell = null, Player player = null)
		{
			var viewRow =row - game.Player.Position.X + ViewFieldSize/2;
			var viewColumn =column- game.Player.Position.Y + ViewFieldSize/2;
			if (viewRow >= 0 && viewRow < ViewFieldSize && viewColumn >= 0 && viewColumn < ViewFieldSize)
			{
                if (cell != null)
                    CurrentWorldView[viewRow, viewColumn] = cell;
                ChangeOneCellView(viewRow, viewColumn, cell, player);
			}
			else
				throw new Exception("OutOfViewBounds");
		}

		public void ChangeOneCell(int row, int column, WorldElement cell = null, Player player = null)
		{
			if (cell!=null)
				CurrentWorldView[row, column] = cell;
			ChangeOneCellView(row, column, cell, player);
		}


        public WorldElement[,] GetWorldVisual(Point playerPosition)
		{
			var table = CurrentWorldView;
            var viewStartPoint = new Point(playerPosition.X - ViewFieldSize / 2, playerPosition.Y - ViewFieldSize / 2);
			for (int i = 0; i < ViewFieldSize; i++)
				for (int j = 0; j < ViewFieldSize; j++)
				{
					var row = i;
					var column = j;
					var newCell = game.InBounds(new Point(viewStartPoint.X + i, viewStartPoint.Y + j)) ? 
						game.World[viewStartPoint.X + i, viewStartPoint.Y + j] : game.OutOfBounds;
					if (newCell != CurrentWorldView[i, j])
					{
						table[i, j] = newCell;
                        if (ChangeOneCellView != null) ChangeOneCellView(row, column, table[row, column],null);
                    }				
				}
            if (ChangeOneCellView != null) ChangeOneCellView(ViewFieldSize / 2, ViewFieldSize / 2,null, game.Player);
            return table;
        }
	}
}
