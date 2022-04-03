using Godot;
using TeamFactory.Items;
using TeamFactory.Util.Multiplayer;
using TeamFactory.Game;

public class UnlockItemContainer : VBoxContainer
{
    private ItemResource item;

    private ShaderMaterial mat;

    public ItemResource UnlockItem
    {
        get {
            return item;
        }
        set {
            item = value;
            initItem();
        }
    }

    public bool IsUnlocked
    {
        set {
            mat.SetShaderParam("Unlocked", value ? 1 : 0);
        }
    }

    public bool IsUnlockable
    {
        set {
            mat.SetShaderParam("Unlockable", value ? 1 : 0);
        }
    }

    public void initItem()
    {
        GetNode<Label>("Name").Text = item.Name;
        GetNode<Label>("GridContainer/Cost").Text = $"{item.UnlockCost}";
        GetNode<Label>("GridContainer/Value").Text = $"{item.PointValue}";
        GetNode<Label>("GridContainer/Power").Text = $"{item.PowerCost}";
        GetNode<Label>("GridContainer/PV").Text = $"{item.PowerValue}";

        mat = (ShaderMaterial)GD.Load<ShaderMaterial>("res://materials/UnlockResourceBtn.tres").Duplicate();
        mat.SetShaderParam("ButtonTexture", item.Texture);

        TextureButton btn = GetNode<TextureButton>("UnlockBtn");
        btn.Material = mat;
        btn.Connect("pressed", this, nameof(OnRequestUnlock));
    }

    public void OnRequestUnlock()
    {
        NetState.RpcId(GetNode<GameServer>("/root/Game/GameServer"), 1, "RequestUnlock", item.Name);
    }
}
