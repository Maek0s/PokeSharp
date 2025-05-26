using Godot;
using System;
using System.Diagnostics;

public partial class Door : Area2D
{
    [Export] public string SceneToLoad = "";
    private bool _canEnter = false;
    private Label _label;
    private bool isADoor = true;
    private bool _isInterior = false;
    private float _xSpawnPoint = 0.0f;
    private float _ySpawnPoint = 0.0f;

    public override void _Ready()
    {
        BodyEntered += OnBodyEntered;
        BodyExited += OnBodyExited;

        _label = GetNodeOrNull<Label>("Label");
        if (_label != null)
        {
            _label.Visible = false;
        }

        DoorData doorData = GetNode<DoorData>("/root/DoorData");
        if (doorData.DoorDestinations.TryGetValue(Name, out var doorInfo))
        {
            SceneToLoad = doorInfo["scene"];
            var isADoortxt = doorInfo["isADoor"];

            if (isADoortxt == "false")
                isADoor = false;
            else
                isADoor = true;

            if (_label != null)
            {
                _label.Text = doorInfo["label"];
            }

            if (doorInfo["interior"].Equals("true"))
            {
                _isInterior = true;
                _xSpawnPoint = float.Parse(doorInfo["xSpawnPoint"]);
                _ySpawnPoint = float.Parse(doorInfo["ySpawnPoint"]);
            }
            else
            {
                _isInterior = false;
                _xSpawnPoint = float.Parse(doorInfo["xSpawnPoint"]);
                _ySpawnPoint = float.Parse(doorInfo["ySpawnPoint"]);
            }
        }
        else
        {
            GD.PrintErr($"❌ No data from the door: {Name} / No data from the door: {Name} ❌");
        }
    }

    private void OnBodyEntered(Node body)
    {
        if (body.IsInGroup("player"))
        {
            _canEnter = true;
            _label.Visible = true;
        }
    }

    private void OnBodyExited(Node body)
    {
        if (body.IsInGroup("player"))
        {
            _canEnter = false;
            _label.Visible = false;
        }
    }

    public override void _Input(InputEvent @event)
    {
        if (@event.IsActionPressed("interact") && _canEnter)
        {
            _canEnter = false;
            EnterDoorAsync();
        }
    }

    private async void EnterDoorAsync()
    {
        GetTree().CurrentScene.Call("ChangeMap", SceneToLoad, _isInterior, _xSpawnPoint, _ySpawnPoint, isADoor);
        await ToSignal(GetTree().CreateTimer(1.0), SceneTreeTimer.SignalName.Timeout);
        _canEnter = true;
    }

}
