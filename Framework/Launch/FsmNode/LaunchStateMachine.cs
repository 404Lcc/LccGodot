using System;
using System.Collections.Generic;

namespace LccModel
{
    public interface ILaunchStateNode
    {
        void OnCreate(LaunchStateMachine machine);
        void OnEnter();
        void OnUpdate(double delta);
        void OnExit();
    }

    public sealed class LaunchStateMachine
    {
        private readonly Dictionary<Type, ILaunchStateNode> _nodes = new();
        private readonly Dictionary<string, object?> _blackboard = new();
        private ILaunchStateNode? _currentNode;

        public LauncherOperation Owner { get; }
        public string PreviousNode { get; private set; } = string.Empty;
        public string CurrentNode { get; private set; } = string.Empty;

        public LaunchStateMachine(LauncherOperation owner)
        {
            Owner = owner;
        }

        public void AddNode<T>() where T : ILaunchStateNode, new()
        {
            var node = new T();
            node.OnCreate(this);
            _nodes[typeof(T)] = node;
        }

        public void Run<T>() where T : ILaunchStateNode
        {
            ChangeState<T>();
        }

        public void ChangeState<T>() where T : ILaunchStateNode
        {
            if (!_nodes.TryGetValue(typeof(T), out var node))
            {
                Owner.SetError($"Missing launch state node: {typeof(T).Name}");
                return;
            }

            _currentNode?.OnExit();
            PreviousNode = CurrentNode;
            CurrentNode = typeof(T).Name;
            _currentNode = node;
            _currentNode.OnEnter();
        }

        public void Update(double delta)
        {
            _currentNode?.OnUpdate(delta);
        }

        public void SetBlackboardValue(string key, object? value)
        {
            _blackboard[key] = value;
        }

        public T? GetBlackboardValue<T>(string key)
        {
            return _blackboard.TryGetValue(key, out var value) ? (T?)value : default;
        }
    }
}
