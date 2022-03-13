using Godot;
using Godot.Collections;

namespace TeamFactory.Map 
{
    public class InfraType
    {
        public enum TypeIdentifier : int
        {
            Factory,
            MultiFactory,
            Input,
            Output,
            Splitter,
            Merger,
        }

        private InfraType()
        {

        }

        public static InfraType GetByIdentifier(TypeIdentifier identifier)
        {
            InfraType typeObj;
            switch(identifier)
            {
                case TypeIdentifier.Factory:
                    typeObj = new InfraType();
                    typeObj.isProducer = true;
                    typeObj.Identifier = TypeIdentifier.Factory;
                    typeObj.Script = GD.Load<Reference>("res://actors/factory/FactoryNode.cs");
                    typeObj.Texture = GD.Load<Texture>("res://actors/factory/SimpleFactory.png");
                    typeObj.Inputs = new Array<GridManager.Direction>();
                    typeObj.Inputs.Add(GridManager.Direction.Left);
                    typeObj.Outputs = new Array<GridManager.Direction>();
                    typeObj.Outputs.Add(GridManager.Direction.Right);
                    return typeObj;

                case TypeIdentifier.MultiFactory:
                    typeObj = new InfraType();
                    typeObj.isProducer = true;
                    typeObj.Identifier = TypeIdentifier.MultiFactory;
                    typeObj.Script = GD.Load<Reference>("res://actors/factory/FactoryNode.cs");
                    typeObj.Texture = GD.Load<Texture>("res://actors/factory/MultiFactory.png");
                    typeObj.Inputs = new Array<GridManager.Direction>();
                    typeObj.Inputs.Add(GridManager.Direction.Left);
                    typeObj.Inputs.Add(GridManager.Direction.Up);
                    typeObj.Inputs.Add(GridManager.Direction.Down);
                    typeObj.Outputs = new Array<GridManager.Direction>();
                    typeObj.Outputs.Add(GridManager.Direction.Right);
                    return typeObj;

                case TypeIdentifier.Input:
                    typeObj = new InfraType();
                    typeObj.Identifier = TypeIdentifier.Input;
                    typeObj.Script = GD.Load<Reference>("res://actors/input/InputNode.cs");
                    typeObj.Texture = GD.Load<Texture>("res://actors/input/InputNode.png");
                    typeObj.Inputs = new Array<GridManager.Direction>();
                    typeObj.Outputs = new Array<GridManager.Direction>();
                    typeObj.Outputs.Add(GridManager.Direction.Right);
                    return typeObj;

                case TypeIdentifier.Output:
                    typeObj = new InfraType();
                    typeObj.Identifier = TypeIdentifier.Output;
                    typeObj.Script = GD.Load<Reference>("res://actors/output/OutputNode.cs");
                    typeObj.Texture = GD.Load<Texture>("res://actors/output/OutputNode.png");
                    typeObj.Inputs = new Array<GridManager.Direction>();
                    typeObj.Inputs.Add(GridManager.Direction.Left);
                    typeObj.Outputs = new Array<GridManager.Direction>();
                    return typeObj;

                case TypeIdentifier.Splitter:
                    typeObj = new InfraType();
                    typeObj.Identifier = TypeIdentifier.Splitter;
                    typeObj.Script = GD.Load<Reference>("res://actors/splitter/SplitterNode.cs");
                    typeObj.Texture = GD.Load<Texture>("res://actors/splitter/Splitter.png");
                    typeObj.Inputs = new Array<GridManager.Direction>();
                    typeObj.Inputs.Add(GridManager.Direction.Left);
                    typeObj.Outputs = new Array<GridManager.Direction>();
                    typeObj.Outputs.Add(GridManager.Direction.Right);
                    typeObj.Outputs.Add(GridManager.Direction.Up);
                    typeObj.Outputs.Add(GridManager.Direction.Down);
                    return typeObj;

                case TypeIdentifier.Merger:
                    typeObj = new InfraType();
                    typeObj.Identifier = TypeIdentifier.Merger;
                    typeObj.Script = GD.Load<Reference>("res://actors/merger/MergerNode.cs");
                    typeObj.Texture = GD.Load<Texture>("res://actors/merger/Merger.png");
                    typeObj.Inputs = new Array<GridManager.Direction>();
                    typeObj.Inputs.Add(GridManager.Direction.Left);
                    typeObj.Inputs.Add(GridManager.Direction.Up);
                    typeObj.Inputs.Add(GridManager.Direction.Down);
                    typeObj.Outputs = new Array<GridManager.Direction>();
                    typeObj.Outputs.Add(GridManager.Direction.Right);
                    return typeObj;

                default:
                    throw new System.Exception($"unknown type identifier {identifier}");
            }
        }

        public TypeIdentifier Identifier;

        public Reference Script;

        public Texture Texture;

        public bool isProducer;

        public Array<GridManager.Direction> Inputs;

        public Array<GridManager.Direction> Outputs; 
    }
}