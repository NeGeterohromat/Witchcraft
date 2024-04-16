using System;
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
            var newPosition = new Point(game.PlayerPosition.X,game.PlayerPosition.Y-1);
            if (AreCharsEqual(e.KeyChar, (char)Keys.A))
            {
                Move(newPosition);
                game.SetPlayerPosition(newPosition);
            }
        };
        keyBar.KeyPress += (sender, e) =>
        {
            var newPosition = new Point(game.PlayerPosition.X+1, game.PlayerPosition.Y);
            if (AreCharsEqual(e.KeyChar, (char)Keys.S))
            {
                Move(newPosition);
                game.SetPlayerPosition(newPosition);
            }
        };
        keyBar.KeyPress += (sender, e) =>
        {
            var newPosition = new Point(game.PlayerPosition.X, game.PlayerPosition.Y+1);
            if (AreCharsEqual(e.KeyChar, (char)Keys.D))
            {
                Move(newPosition);
                game.SetPlayerPosition(newPosition);
            }
        };
        keyBar.KeyPress += (sender, e) =>
        {
            var newPosition = new Point(game.PlayerPosition.X-1, game.PlayerPosition.Y);
            if (AreCharsEqual(e.KeyChar, (char)Keys.W))
            {
                Move(newPosition);
                game.SetPlayerPosition(newPosition);
            }
        };
    }

    public static bool AreCharsEqual(char char1, char char2) =>
        char1 == char2 || char.ToUpper(char1) == char2 || char1==char.ToUpper(char2);
}
