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

        public void ChangeOneCell(int row, int column, WorldElement cell=null, Player player=null)=>
			ChangeOneCellView(row, column, cell,player);


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
