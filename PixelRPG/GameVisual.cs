using System;
using System.Data.Common;

namespace PixelRPG
{
	public class GameVisual
	{
		private GameModel game;
		public const int ViewFieldSize = 20;
		public GameVisual(GameModel game)
		{
			this.game = game;
		}

		public event Action<int,int,WorldCell> ChangeWorldCell;

		public WorldCell[,] GetWorldVisual(Point playerPosition)
		{
			var table = new WorldCell[ViewFieldSize,ViewFieldSize];
            var viewStartPoint = new Point(playerPosition.X - ViewFieldSize / 2, playerPosition.Y - ViewFieldSize / 2);
			for (int i = 0; i < ViewFieldSize; i++)
				for (int j = 0; j < ViewFieldSize; j++)
				{
					var row = i;
					var column = j;
					if (game.InBounds(new Point(viewStartPoint.X + i, viewStartPoint.Y + j)))
						table[i, j] = game.World[viewStartPoint.X + i, viewStartPoint.Y + j];
					else
						table[i, j] = WorldCell.OutOfBounds;
					if (ChangeWorldCell!=null) ChangeWorldCell(row, column, table[row, column]);
                }
            if (ChangeWorldCell != null) ChangeWorldCell(ViewFieldSize / 2, ViewFieldSize / 2, WorldCell.Character);
            return table;
        }
	}
}
