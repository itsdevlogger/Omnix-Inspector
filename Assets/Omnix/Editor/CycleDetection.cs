using System;
using System.Collections.Generic;
using System.Reflection;
using Omnix.Core;


public enum DetectionType
{
    NoLoop,
    LoopButSerializationDepthReached,
    LoopButEntryPointNotFound,
    LoopAndFoundEntry
}

public abstract class CycleDetector<TInfo>
where TInfo : class
{
    public delegate CycleDetector<TInfo> InstanceCreator(object root, HashSet<Type> ignoreTypes);

    private HashSet<Type> _ignoreTypes;
    private Stack<object> _stack;
    private TInfo[] _currentEnumerator;
    private int _currentEnumeratorIndex;
    private object _currentParent;
    public TInfo Current;

    protected abstract Type CurrentType { get; }
    protected abstract TInfo[] GetChildrenOf(object obj);
    protected abstract object GetValueOf(TInfo info, object target);

    public CycleDetector(object root, TInfo[] baseEnumerator, HashSet<Type> ignoreTypes)
    {
        _ignoreTypes = ignoreTypes;
        _stack = new Stack<object>();
        _currentParent = root;
        _currentEnumerator = baseEnumerator;
        _currentEnumeratorIndex = 0;
    }

    public bool MoveNext()
    {
        if (_currentEnumerator.Length >= _currentEnumeratorIndex) return false;
        Current = _currentEnumerator[_currentEnumeratorIndex];
        _currentEnumeratorIndex++;

        if (!_ignoreTypes.Contains(CurrentType))
        {
            object value = GetValueOf(Current, _currentParent);
            _stack.Push(value);
        }

        if (_currentEnumeratorIndex < _currentEnumerator.Length) return true;
        if (_stack.Count == 0) return false;

        _currentParent = _stack.Pop();
        _currentEnumerator = GetChildrenOf(_currentParent);
        _currentEnumeratorIndex = 0;
        return true;
    }

    /// <returns>true is there's a loop false otherwise</returns>
    protected static DetectionType DetectCycle(object root, HashSet<Type> ignoreTypes, InstanceCreator instanceCreator, out TInfo entryPoint)
    {
        CycleDetector<TInfo> slow = instanceCreator(root, ignoreTypes);
        CycleDetector<TInfo> fast = instanceCreator(root, ignoreTypes);

        slow.MoveNext();
        if (!fast.MoveNext() || !fast.MoveNext()) // fast moves twice the speed of slow
        {
            entryPoint = null;
            return DetectionType.NoLoop;
        }

        while (true)
        {
            slow.MoveNext();
            if (!fast.MoveNext() || !fast.MoveNext())
            {
                entryPoint = null;
                return DetectionType.NoLoop;
            }
            if ((fast.CurrentType == slow.CurrentType) && !ignoreTypes.Contains(fast.CurrentType)) break;
        }


        int maxCount = 0;
        slow = instanceCreator(root, ignoreTypes);
        while (true)
        {
            if (maxCount > 100)
            {
                entryPoint = null;
                return DetectionType.LoopButSerializationDepthReached;
            }
            maxCount++;

            if (!slow.MoveNext() || !fast.MoveNext())
            {
                entryPoint = null;
                return DetectionType.LoopButEntryPointNotFound;
            }
            if ((slow.CurrentType == fast.CurrentType) && !ignoreTypes.Contains(fast.CurrentType)) break;
        }
        entryPoint = slow.Current;
        return DetectionType.LoopAndFoundEntry;
    }
}


public class FieldCycleDetector : CycleDetector<FieldInfo>
{
    public FieldCycleDetector(object root, HashSet<Type> ignoreTypes) : base(root, root.GetType().GetFields(Helpers.Flags), ignoreTypes) { }
    protected override Type CurrentType => Current.FieldType;
    protected override FieldInfo[] GetChildrenOf(object obj) => obj.GetType().GetFields(Helpers.Flags);
    protected override object GetValueOf(FieldInfo info, object target) => Current.GetValue(target) ?? Activator.CreateInstance(Current.FieldType);
    public static DetectionType Detect(object root, HashSet<Type> ignoreTypes, out FieldInfo entryPoint) =>
        DetectCycle(
            root: root,
            ignoreTypes: ignoreTypes,
            instanceCreator: (object obj, HashSet<Type> ignore) => new FieldCycleDetector(obj, ignore),
            out entryPoint
        );
}


public class PropertyCycleDetector : CycleDetector<PropertyInfo>
{
    public PropertyCycleDetector(object root, HashSet<Type> ignoreTypes) : base(root, root.GetType().GetProperties(Helpers.Flags), ignoreTypes) { }
    protected override Type CurrentType => Current.PropertyType;
    protected override PropertyInfo[] GetChildrenOf(object obj) => obj.GetType().GetProperties(Helpers.Flags);
    protected override object GetValueOf(PropertyInfo info, object target) => Current.GetValue(target) ?? Activator.CreateInstance(Current.PropertyType);
    public static DetectionType Detect(object root, HashSet<Type> ignoreTypes, out PropertyInfo entryPoint) =>
        DetectCycle(
            root: root,
            ignoreTypes: ignoreTypes,
            instanceCreator: (object obj, HashSet<Type> ignore) => new PropertyCycleDetector(obj, ignore),
            out entryPoint
        );

}