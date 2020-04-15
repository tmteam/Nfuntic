﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace nfun.Ti4
{

    public enum SolvingNodeType
    {
        Named,
        SyntaxNode,
        TypeVariable
    }

    public class SolvingNode
    {
        private object _state;
        public int GraphId { get; set; }

        public SolvingNode(string name, object state, SolvingNodeType type)
        {
            Name = name;
            State = state;
            Type = type;
        }

        public SolvingNodeType Type { get; }
        public List<SolvingNode> Ancestors { get; } = new List<SolvingNode>();
        public List<SolvingNode> MemberOf { get; } = new List<SolvingNode>();
        public bool IsSolved => State is PrimitiveType || (State as ArrayOf)?.IsSolved == true;

        public object State
        {
            get => _state;
            set
            {
                if(value == null)
                    throw new InvalidOperationException();
                if(IsSolved && value!=_state)
                    throw new InvalidOperationException("Node is already solved");
                
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

            if (State is PrimitiveType concrete)
                Console.Write($"{concrete.Name} ");
            else if (State is RefTo reference)
                Console.Write($"{reference.Node.Name}");
            else if (State is SolvingConstrains constrains)
                Console.Write(constrains);
            else if (State is ArrayOf array)
                Console.Write("arr("+ array.ElementNode.Name+")");

            if (Ancestors.Any()) 
                Console.Write( "  <=" + string.Join(",", Ancestors.Select(a=>a.Name)));

            Console.WriteLine();

        }


        public bool BecomeConcrete(PrimitiveType primitive)
        {
            if (this.State is PrimitiveType oldConcrete)
            {
                return oldConcrete == primitive;
            }
            else if (this.State is SolvingConstrains constrains)
            {
                if (!constrains.Fits(primitive))
                    return false;
                this.State = primitive;
                return true;
            }

            return false;
        }
        public bool TrySetAncestor(PrimitiveType anc)
        {
            var node = this;
            if (node.State is RefTo)
                node = node.GetNonReference();

            if (node.State is PrimitiveType oldConcrete)
            {
                return oldConcrete.CanBeImplicitlyConvertedTo(anc);
            }
            else if (node.State is SolvingConstrains constrains)
            {
                return constrains.TryAddAncestor(anc);
            };
            return false;
        }
        public void SetAncestor(PrimitiveType anc)
        {
            if(!TrySetAncestor(anc))
                throw new InvalidOperationException();
        }
    }

    public class RefTo
    {
        public RefTo(SolvingNode node)
        {
            if (node.Type != SolvingNodeType.TypeVariable)
            {

            }
            Node = node;
        }
        public SolvingNode Node { get; }
        public override string ToString() => $"ref({Node})";
    }
}
