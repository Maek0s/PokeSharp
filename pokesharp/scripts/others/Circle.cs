using Godot;
using System;

public partial class Circle : Control
{
    [Export]
    public Color CircleColor = new Color(0.2f, 0.6f, 1.0f);

    [Export]
    public float Radius = 20.0f;

    public override void _Ready()
    {
        QueueRedraw();
    }

    public override void _Draw()
    {
        Vector2 center = Size / 2;
        DrawCircle(center, Radius, CircleColor);
    }

    public override void _Notification(int what)
    {
        if (what == NotificationResized)
        {
            QueueRedraw();
        }
    }
}
