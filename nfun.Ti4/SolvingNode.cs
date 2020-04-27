﻿using System;
using System.Collections.Generic;
using System.Linq;
using NFun.Tic.SolvingStates;
using Array = NFun.Tic.SolvingStates.Array;

namespace NFun.Tic
{
    public enum SolvingNodeType
    {
        Named,
        SyntaxNode,
        TypeVariable
    }

    public class SolvingNode
    {
        private IState _state;
        public int GraphId { get; set; } = -1;
        public static SolvingNode CreateTypeNode(IType type) 
            => new SolvingNode(type.ToString(), type, SolvingNodeType.TypeVariable);

        public SolvingNode(string name, IState state, SolvingNodeType type)
        {
            Name = name;
            State = state;
            Type = type;
        }

        public SolvingNodeType Type { get; }
        public List<SolvingNode> Ancestors { get; } = new List<SolvingNode>();
        public List<SolvingNode> MemberOf { get; } = new List<SolvingNode>();
        public bool IsSolved => State is Primitive || (State as Array)?.IsSolved == true;

        public IState State
        {
            get => _state;
            set
            {
                if(value == null)
                    throw new InvalidOperationException();
                if(IsSolved && !value.Equals(_state))
                    throw new InvalidOperationException("Node is already solved");
                
                if (value is Array array) 
                    array.ElementNode.MemberOf.Add(this);
                
                _state = value;
            }
        }

        public string Name { get; }
        public override string ToString()
        {
            if (Name == State.ToString())
                return Name;
            else 
                return $"{Name}:{State}";
        }

        public void PrintToConsole()
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write($"{Name}:");
            Console.ResetColor();

            if (State is Primitive concrete)
                Console.Write($"{concrete.Name} ");
            else if (State is RefTo reference)
                Console.Write($"{reference.Node.Name}");
            else if (State is Constrains constrains)
                Console.Write(constrains);
            else if (State is Array array)
                Console.Write("arr("+ array.ElementNode.Name+")");
            else if (State is Fun fun)
                Console.Write($"({string.Join(",", fun.ArgNodes.Select(a => a.Name))})->{fun.RetNode.Name}");
            if (Ancestors.Any()) 
                Console.Write( "  <=" + string.Join(",", Ancestors.Select(a=>a.Name)));

            Console.WriteLine();

        }


        public bool BecomeConcrete(Primitive primitive)
        {
            if (this.State is Primitive oldConcrete)
                return oldConcrete.Equals(primitive);
            if (this.State is Constrains constrains)
            {
                if (!constrains.Fits(primitive))
                    return false;
                this.State = primitive;
                return true;
            }

            return false;
        }
        public bool TrySetAncestor(Primitive anc)
        {
            var node = this;
            if (node.State is RefTo)
                node = node.GetNonReference();

            if (node.State is Primitive oldConcrete)
            {
                return oldConcrete.CanBeImplicitlyConvertedTo(anc);
            }
            else if (node.State is Constrains constrains)
            {
                return constrains.TryAddAncestor(anc);
            };
            return false;
        }
        public void SetAncestor(Primitive anc)
        {
            if(!TrySetAncestor(anc))
                throw new InvalidOperationException();
        }
    }
}
