using System;
using System.Security.Policy;
using PixelRPG;

public class GameControls
{
    private GameModel game;
    private GameVisual visual;
    public GameControls( GameModel game, GameVisual visual)
	{
        this.game = game;
        this.visual = visual;
	}

    public void Move(Point newPosition)=>
        visual.GetWorldVisual(newPosition);


    public void SetKeyCommands(TextBox keyBar)
    {
        keyBar.KeyPress += (sender, e) =>
        {
            Point newPosition = new Point(-1,-1);
            var newView = game.PlayerView;
            if (AreCharsEqual(e.KeyChar, (char)Keys.A))
            {
                newView = Characters.BaseLeft;
                newPosition = new Point(game.PlayerPosition.X, game.PlayerPosition.Y - 1);
            }
            if (AreCharsEqual(e.KeyChar, (char)Keys.S))
            {
                newView = Characters.BaseDown;
                newPosition = new Point(game.PlayerPosition.X + 1, game.PlayerPosition.Y);
            }
            if (AreCharsEqual(e.KeyChar, (char)Keys.D))
            {
                newView = Characters.BaseRight;
                newPosition = new Point(game.PlayerPosition.X, game.PlayerPosition.Y + 1);
            }
            if (AreCharsEqual(e.KeyChar, (char)Keys.W))
            {
                newView = Characters.BaseUp;
                newPosition = new Point(game.PlayerPosition.X - 1, game.PlayerPosition.Y);
            }
            if (newView != game.PlayerView)
            {
                game.SetPlayerView(newView);
                visual.ChangeOneCell(GameVisual.ViewFieldSize / 2, GameVisual.ViewFieldSize / 2, newView);
            }
            else if (game.InBounds(newPosition) && game.IsStepablePoint(newPosition))
            {
                Move(newPosition);
                game.SetPlayerPosition(newPosition);
            }
        };
    }

    public static bool AreCharsEqual(char char1, char char2) =>
        char1 == char2 || char.ToUpper(char1) == char2 || char1==char.ToUpper(char2);
}
