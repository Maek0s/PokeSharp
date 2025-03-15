using Godot;
using System;

public partial class FpsDisplay : Label
{
	public override void _PhysicsProcess(double delta)
    {
        showFPS();
    }

    private void showFPS()
    {
        var fps = Engine.GetFramesPerSecond();

        Text = "FPS: " + fps;
    }
}
