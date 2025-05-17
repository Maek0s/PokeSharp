using Godot;
using System;
using System.Collections.Generic;
using Godot.Collections;

public partial class Entrenador : CharacterBody2D
{
    [Export] public string Nombre = "Entrenador";
    [Export] public Godot.Collections.Array equipoResources = new Godot.Collections.Array();
    public List<Pokemon> equipoPokemon { get; set; } = new List<Pokemon>();
    [Export] public string[] Dialogos;
    [Export] private int condicion;
    [Export] public SpriteFrames spriteFrames;
    [Export] private int dificultad;
    [Export] private int movesTutor;
    [Export] private int movesType;
    [Export] private int movesRandom;

}
