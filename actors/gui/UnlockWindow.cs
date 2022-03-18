using Godot;
using Godot.Collections;
using TeamFactory.Game;
using TeamFactory.Util.Multiplayer;

public class UnlockWindow : WindowDialog
{
    private TextureButton sandBtn;

    private TextureButton glassBtn;

    private TextureButton siliconBtn;

    private TextureButton chipBtn;

    public override void _Ready()
    {
        sandBtn = GetNode<TextureButton>("SandBtn");
        sandBtn.Connect("pressed", this, nameof(OnUnlock), new Array(){"Sand"});

        glassBtn = GetNode<TextureButton>("GlassBtn");
        glassBtn.Connect("pressed", this, nameof(OnUnlock), new Array(){"Glass"});
        
        siliconBtn = GetNode<TextureButton>("SiliconBtn");
        siliconBtn.Connect("pressed", this, nameof(OnUnlock), new Array(){"Silicon"});

        chipBtn = GetNode<TextureButton>("ChipBtn");
        chipBtn.Connect("pressed", this, nameof(OnUnlock), new Array(){"Chip"});

        Connect("about_to_show", this, nameof(OnShow));
    }

    public void OnShow()
    {
        // load currently unlocked items
        
        Array<string> unlocks = GetNode<GameNode>("/root/Game").PlayerUnlocks;
        // use these to feed unlock state to shaders of buttons
        if (unlocks.Contains("Sand"))
            ((ShaderMaterial)(sandBtn.Material)).SetShaderParam("Unlocked", 1);

        if (unlocks.Contains("Glass"))
            ((ShaderMaterial)(glassBtn.Material)).SetShaderParam("Unlocked", 1);

        if (unlocks.Contains("Chip"))
            ((ShaderMaterial)(chipBtn.Material)).SetShaderParam("Unlocked", 1);

        if (unlocks.Contains("Silicon"))
            ((ShaderMaterial)(siliconBtn.Material)).SetShaderParam("Unlocked", 1);
    }

    public void OnUnlock(string itemName)
    {
        NetState.RpcId(GetNode<GameServer>("/root/Game/GameServer"), 1, "RequestUnlock", itemName);
    }
}
