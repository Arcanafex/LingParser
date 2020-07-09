using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Lx
{
    public class FiniteStateAutomoton<T>
    {
        public Dictionary<T, Node<T>> States { get; private set; }
        public Dictionary<NodeChain<T>, Dictionary<Node<T>, Transition<T>>> Transitions { get; private set; }        

        public FiniteStateAutomoton()
        {
            States = new Dictionary<T, Node<T>>();
            Transitions = new Dictionary<NodeChain<T>, Dictionary<Node<T>, Transition<T>>>();
        }

        //public void AddTransition(T[] from, T[] to)
        //{
        //    Node<T> startNode;
        //    Node<T> endNode;

        //    if (from == null)
        //    {
        //        startNode = Start;
        //    }
        //    else
        //    {
        //        startNode = States.FirstOrDefault(n => n.Value.SequenceEqual(from));

        //        if (startNode == null)
        //        {
        //            startNode = new Node<T>(from);
        //        }

        //        States.Add(startNode);
        //    }

        //    if (to == null)
        //    {
        //        endNode = End;
        //    }
        //    else
        //    {
        //        endNode = States.FirstOrDefault(n => n.Value.SequenceEqual(to));

        //        if (endNode == null)
        //            endNode = new Node<T>(to);

        //        States.Add(endNode);
        //    }

        //    startNode.AddTransition(endNode);
        //}

        public Node<T> AddNode(T segment)
        {
            var node = GetNode(segment);

            if (node == null)
            {
                node = new Node<T>(this, segment);
                States.Add(segment, node);
            }


            return node;
        }

        public void AddTransition(Transition<T> transition)
        {
            AddTransition(transition.Chain, transition.EndState, transition.Weight);
        }

        public void AddTransition(Node<T> from, Node<T> to, int weight = 1)
        {
            AddTransition(from.ToChain(), to, weight);
        }

        public void AddTransition(NodeChain<T> from, Node<T> to, int weight = 1)
        {
            // TODO: Verify all nodes are present in model's sets

            var toNode = to.Value != null ? AddNode(to.Value) : to;
            var fromChain = new NodeChain<T>();

            foreach (var node in from)
            {
                if (node.Value == null)
                    fromChain.Add(node);
                else
                    fromChain.Add(AddNode(node.Value));
            }

            if (Transitions.ContainsKey(from))
            {
                if (Transitions[from].ContainsKey(to))
                {
                    Transitions[from][to].Weight += weight;
                }
                else
                {
                    var transition = new Transition<T>()
                    {
                        Chain = from,
                        EndState = to,
                        Weight = weight
                    };

                    Transitions[from].Add(to, transition);
                }
            }
            else
            {
                var transitions = new Dictionary<Node<T>, Transition<T>>
                {
                    {
                        to,
                        new Transition<T>()
                        {
                            Chain = from,
                            EndState = to,
                            Weight = weight
                        }
                    }
                };

                Transitions.Add(from, transitions);
            }
        }

        //public NodeChain<T> StartChain(Node<T> node)
        //{
        //    var chain = new NodeChain<T>(this);
        //    chain.AddLast(node);

        //    return chain;
        //}

        //public NodeChain<T> StartChain(IEnumerable<Node<T>> nodes)
        //{
        //    var chain = new NodeChain<T>(this);

        //    foreach (var node in nodes)
        //    {
        //        chain.AddLast(node);
        //    }

        //    return chain;
        //}

        public Node<T> GetNode(T segment)
        {
            if (segment == null)
                return null;

            return States.ContainsKey(segment) ? States[segment] : null;
        }

        public Dictionary<Node<T>, Transition<T>> GetTransitions(Node<T> node)
        {
            return GetTransitions(new List<Node<T>>() { node });
        }

        public Dictionary<Node<T>, Transition<T>> GetTransitions(IEnumerable<Node<T>> chain = null)
        {
            var transitions = new Dictionary<Node<T>, Transition<T>>();
            var sequence = chain.ToChain();

            if (Transitions.Count > 0)
            {
                if (sequence.Count == 0)
                {
                    transitions = Transitions[Node<T>.Start.ToChain()];
                }
                if (Transitions.ContainsKey(sequence))
                {
                    transitions = Transitions[sequence];
                }
                else
                {
                    int skip = 0;

                    while (transitions.Count == 0 && skip < sequence.Count)
                    {
                        var key = sequence.Skip(++skip).ToChain();
                        if (Transitions.ContainsKey(key))
                            transitions = Transitions[key];
                    }
                }
            }

            return transitions;
        }

        public void Parse(T[] input, int length)
        {
            var chain = new Queue<Node<T>>();
            chain.Enqueue(Node<T>.Start);

            if (input != null && input.Length > 0)
            {
                foreach (var item in input)
                {
                    var node = AddNode(item);
                    AddTransition(chain.ToChain(), node);
                    chain.Enqueue(node);

                    if (chain.Peek() == Node<T>.Start || chain.Count > length)
                    {
                        chain.Dequeue();
                    }
                }

                AddTransition(chain.ToChain(), Node<T>.End);
            }
        }

        public List<T> GenerateRandomChain()
        {
            var random = new Random();

            return GenerateRandomChain(random);
        }

        public List<T> GenerateRandomChain(Random random)
        {
            var randomWalk = new NodeChain<T>();

            Node<T> currentState = GetNext(randomWalk, random);

            while (currentState.Type != NodeType.End)
            {
                randomWalk.Add(currentState);
                currentState = GetNext(randomWalk, random);
            }

            return randomWalk.Select(s => s.Value).ToList();
        }

        public Node<T> GetNext(IEnumerable<Node<T>> chain, Random random)
        {
            var transitions = GetTransitions(chain);
            int total = transitions.Sum(t => t.Value.Weight) + 1;
            int pick = random.Next(total);

            foreach (var transition in transitions.Values)
            {
                pick -= transition.Weight;

                if (pick <= 0)
                {
                    return transition.EndState;
                }
            }

            return Node<T>.End;
        }
    }

    //Discource set as a way of specifying features for a sequence of Expressions.


    public enum NodeType { Start, End, Value }

    public class Node<T>
    {
        internal readonly Guid id;
        public NodeType Type { get; }
        public FiniteStateAutomoton<T> Parent { get; }
        public T Value { get; private set; }

        internal Node(FiniteStateAutomoton<T> parent, T value)
        {
            id = Guid.NewGuid();
            Parent = parent;
            Type = NodeType.Value;
            Value = value;
        }

        internal Node(NodeType type)
        {
            this.id = new Guid((int)type, 0, 0, new byte[8]);
            this.Type = type;
        }

        public static Node<T> Start
        {
            get
            {
                return new Node<T>(NodeType.Start);
            }
        }

        public static Node<T> End
        {
            get
            {
                return new Node<T>(NodeType.End);
            }
        }

        public void UpdateValue(T value)
        {
            if (Value.Equals(value))
                return;

            if (Parent != null)
            {
                Parent.States.Remove(Value);
                Parent.States.Add(value, this);
            }

            Value = value;
        }

        //public void AddTransition(Node<T> to, int weight = 1)
        //{
        //    if (Transitions.ContainsKey(to))
        //    {
        //        Transitions[to] += weight;
        //    }
        //    else
        //    {
        //        Transitions.Add(to, weight);
        //    }
        //}

        //public Node<T> GetNext(Random random)
        //{
        //    int total = Transitions.Values.Sum();
        //    int pick = random.Next(total);

        //    foreach(var node in Transitions.Keys)
        //    {
        //        pick -= Transitions[node];

        //        if (pick <= 0)
        //        {
        //            return node;
        //        }
        //    }

        //    return End;
        //}

        public static bool operator ==(Node<T> left, Node<T> right)
        {
            if (left is null)
                return right is null;
            else
                return left.Equals(right);
        }

        public static bool operator !=(Node<T> left, Node<T> right)
        {
            return !(left.Equals(right));
        }

        public override bool Equals(object obj)
        {
            if (obj is Node<T>)
                return Equals((Node<T>)obj);
            else
                return false;
        }

        public bool Equals(Node<T> other)
        {
            if (other is null || Type != other.Type)
                return false;

            if (id == other.id)
                return true;
            else
                return false;
        }

        public override int GetHashCode()
        {
            return BitConverter.ToInt32(id.ToByteArray(), 0);
        }

        public override string ToString()
        {
            if (Type == NodeType.Value)
                return Value.ToString();
            else
                return $"[{Type.ToString()}]";
        }
    }

    public class NodeChain<T> : List<Node<T>>
    {
        public static bool operator ==(NodeChain<T> left, NodeChain<T> right)
        {
            if (left is null)
                return right is null;

            return left.Equals(right);
        }

        public static bool operator !=(NodeChain<T> left, NodeChain<T> right)
        {
            return !(left.Equals(right));
        }

        public bool Equals(NodeChain<T> other)
        {
            if (other is null)
                return false;

            if (this.Count == other.Count)
            {
                if (Count == 1)
                {

                }

                for (int i = 0; i < Count; i++)
                {
                    if (this[i] != other[i])
                        return false;
                }
            }

            return true;
        }

        public override bool Equals(object obj)
        {
            return obj is NodeChain<T> ? Equals((NodeChain<T>)obj) : false;
        }

        public override int GetHashCode()
        {
            int thing = 0;

            foreach (Node<T> item in this)
            {
                thing += BitConverter.ToInt32(item.id.ToByteArray(), 0);
            }

            return thing;
        }

        public override string ToString()
        {
            return ToString(string.Empty);
        }

        public string ToString(string separator)
        {
            var builder = new StringBuilder();
            int i = 0;

            foreach (var node in this)
            {
                builder.Append(node.ToString());

                if (++i < this.Count)
                    builder.Append(separator);
            }

            return builder.ToString();
        }

    }

    public static class NodeChainExtensions
    {
        public static NodeChain<T> ToChain<T>(this IEnumerable<Node<T>> list)
        {
            var chain = new NodeChain<T>();

            foreach (var item in list)
            {
                chain.Add(item);
            }

            return chain;
        }

        public static NodeChain<T> ToChain<T>(this Node<T> node)
        {
            var chain = new NodeChain<T>();
            chain.Add(node);
            return chain;
        }

    }

    public class TransitionTable<T> : Dictionary<List<Node<T>>, Dictionary<Node<T>, Transition<T>>>
    {

    }

    public class Transition<T>
    {
        public NodeChain<T> Chain;
        public Node<T> EndState;
        public int Weight;

        public FiniteStateAutomoton<T> ParentModel
        {
            get
            {
                return EndState.Parent;
            }
        }

        public void MergeTransition(Transition<T> mergee)
        {
            if (mergee == null)
                return;

            if (mergee.ParentModel != null && mergee.ParentModel.Transitions.ContainsKey(mergee.Chain))
            {
                if (mergee.ParentModel.Transitions[mergee.Chain].Remove(mergee.EndState))
                {
                    if (mergee.ParentModel.Transitions[mergee.Chain].Count == 0)
                        mergee.ParentModel.Transitions.Remove(mergee.Chain);
                }
            }

            Weight += mergee.Weight;
        }

        public static bool operator ==(Transition<T> left, Transition<T> right)
        {
            if (left is null)
                return right is null;

            return left.Equals(right);
        }

        public static bool operator !=(Transition<T> left, Transition<T> right)
        {
            return !(left.Equals(right));
        }

        public bool Equals(Transition<T> other)
        {
            if (other == null)
                return false;

            return EndState == other.EndState && Chain.SequenceEqual(other.Chain);
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as Transition<T>);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }

}

