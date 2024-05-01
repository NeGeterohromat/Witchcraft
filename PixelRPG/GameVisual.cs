using System;
using System.Data.Common;

namespace PixelRPG
{
	public class GameVisual
	{
		private GameModel game;
		public const int ViewFieldSize = 20;
		private WorldElement[,] CurrentWorldView = new WorldElement[ViewFieldSize, ViewFieldSize];
		public Dictionary<Point, Mob> CurrentViewedMobs { get; private set; }
        public GameVisual(GameModel game)
		{
			this.game = game;
            CurrentViewedMobs = new Dictionary<Point, Mob>();
        }

		public event Action<int,int,WorldElement,Player,Mob> ChangeOneCellView;
		public event Action<int,WorldElement> ChangeInventoryCellView;
        public event Action<WorldElement> ChangeCurrentInventorySlotView;
        public event Action OpenInventoryView;
        public event Action CloseInventoryView;
        public event Action OpenMenuView;
        public event Action<Inventory> ChangeCraftImagesView;

		public void OpenInventory() => OpenInventoryView();
        public void CloseInventory() => CloseInventoryView();
        public void OpenMenu() => OpenMenuView();
        public void ChangeCraftImages(Inventory inv) => ChangeCraftImagesView(inv);

        public void ChangeInventoryCell(int number, WorldElement cell) =>
			ChangeInventoryCellView(number, cell);

		public void ChangeCurrentInventorySlot(WorldElement el) =>
			ChangeCurrentInventorySlotView(el);


        public void ChangeOneCellByWorldCoords(int row, int column, WorldElement cell = null, Player player = null,Mob mob = null)
		{
			var viewRow =row - game.Player.Position.X + ViewFieldSize/2;
			var viewColumn =column- game.Player.Position.Y + ViewFieldSize/2;
			if (viewRow >= 0 && viewRow < ViewFieldSize && viewColumn >= 0 && viewColumn < ViewFieldSize)
			{
                if (cell != null)
                    CurrentWorldView[viewRow, viewColumn] = cell;
                ChangeOneCellView(viewRow, viewColumn, cell, player,mob);
			}
			else
				throw new Exception("OutOfViewBounds");
		}

		public void ChangeOneCell(int row, int column, WorldElement cell = null, Player player = null,Mob mob=null)
		{
			if (cell!=null)
				CurrentWorldView[row, column] = cell;
			ChangeOneCellView(row, column, cell, player,mob);
		}

		public Dictionary<Point, Mob> ViewMobs(Point viewStartPoint)
		{
			foreach (var mob in game.Mobs.Values)
				if (mob.Position.X >= viewStartPoint.X && mob.Position.Y >= viewStartPoint.Y
				&& mob.Position.X < viewStartPoint.X + ViewFieldSize && mob.Position.Y < viewStartPoint.Y + ViewFieldSize)
				{
					CurrentViewedMobs[new Point(mob.Position.X - viewStartPoint.X, mob.Position.Y - viewStartPoint.Y)]=mob;
					if (ChangeOneCellView != null)
						ChangeOneCellView(mob.Position.X - viewStartPoint.X, mob.Position.Y - viewStartPoint.Y, null, null, mob);
				}	
			return CurrentViewedMobs;
		}

        public (WorldElement[,] Elements, Dictionary<Point, Mob> Mobs) GetWorldVisual(Point playerPosition)
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
					if (!newCell.Equals(CurrentWorldView[i, j]) || CurrentViewedMobs.ContainsKey(new Point(i, j)))
					{
						table[i, j] = newCell;
                        if (ChangeOneCellView != null) ChangeOneCellView(row, column, table[row, column],null,null);
                    }				
				}
			CurrentViewedMobs.Clear();
			var mobList = ViewMobs(viewStartPoint);
            if (ChangeOneCellView != null) ChangeOneCellView(ViewFieldSize / 2, ViewFieldSize / 2,null, game.Player,null);
            return (table,mobList);
        }
	}
}
