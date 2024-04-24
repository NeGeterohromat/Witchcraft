using System;
using System.Data.Common;

namespace PixelRPG
{
	public class GameVisual
	{
		private GameModel game;
		public const int ViewFieldSize = 20;
		private WorldElement[,] CurrentView = new WorldElement[ViewFieldSize, ViewFieldSize];

        public GameVisual(GameModel game)
		{
			this.game = game;
		}

		public event Action<int,int,WorldElement> ChangeWorldCellView;
		public event Action<int, int, Player> ChangePlayerAvatarView;

        public void ChangeOneCell(int row, int column, WorldElement cell)=>
			ChangeWorldCellView(row, column, cell);

        public void ChangeOneCell(int row, int column, Player player) =>
            ChangePlayerAvatarView(row, column, player);


        public WorldElement[,] GetWorldVisual(Point playerPosition)
		{
			var table = CurrentView;
            var viewStartPoint = new Point(playerPosition.X - ViewFieldSize / 2, playerPosition.Y - ViewFieldSize / 2);
			for (int i = 0; i < ViewFieldSize; i++)
				for (int j = 0; j < ViewFieldSize; j++)
				{
					var row = i;
					var column = j;
					var newCell = game.InBounds(new Point(viewStartPoint.X + i, viewStartPoint.Y + j)) ? 
						game.World[viewStartPoint.X + i, viewStartPoint.Y + j] : game.OutOfBounds;
					if (newCell != CurrentView[i, j])
					{
						table[i, j] = newCell;
                        if (ChangeWorldCellView != null) ChangeWorldCellView(row, column, table[row, column]);
                    }				
				}
            if (ChangePlayerAvatarView != null) ChangePlayerAvatarView(ViewFieldSize / 2, ViewFieldSize / 2, game.Player);
            return table;
        }
	}
}
