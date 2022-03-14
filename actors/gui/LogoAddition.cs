using Godot;
using Godot.Collections;
using System;

public class LogoAddition : Label
{
    private Array<string> texts = new Array<string>();

    public override void _Ready()
    {
        texts.Add("No");
        texts.Add("It was");

        RandomNumberGenerator randGen = new RandomNumberGenerator();
        randGen.Seed = (ulong)(DateTime.Now.Ticks);
        int index = randGen.RandiRange(0, texts.Count - 1);
        Text = texts[index];
    }
}
