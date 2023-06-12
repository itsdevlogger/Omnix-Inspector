using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using JetBrains.Annotations;
using Omnix.Core;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;


/// <summary>
/// Member of a class. Can be a SerializedProperty, Field, Method or Property.
/// </summary>
public class ClassMember
{
    /// <summary>
    /// Properties that should not be counted as class members. (Unity-Internal properties)
    /// </summary>
    private static readonly HashSet<string> IgnoreProperties = new HashSet<string>()
    {
        "m_ObjectHideFlags",
        "m_CorrespondingSourceObject",
        "m_PrefabInstance",
        "m_PrefabAsset",
        "m_GameObject",
        "m_Enabled",
        "m_EditorHideFlags",
        "m_Script",
        "m_Name",
        "m_EditorClassIdentifier",
    };

    #region Fields
    public readonly string Name;
    public readonly string DisplayName;
    public readonly object TargetObject;
    public readonly SerializedProperty Serialized;
    public readonly Type ValueType;
    public readonly MemberInfo MemberInfo;
    public readonly bool CanWrite;
    public readonly Func<object> GetValue;
    #endregion

    #region Getters
    public bool boolValue
    {
        get => (bool)GetValue();
        set
        {
            if (CanWrite) Serialized.boolValue = value;
        }
    }

    public float floatValue
    {
        get => (float)GetValue();
        set
        {
            if (CanWrite) Serialized.floatValue = value;
        }
    }

    public string stringValue
    {
        get => (string)GetValue();
        set
        {
            if (CanWrite) Serialized.stringValue = value;
        }
    }

    public Color colorValue
    {
        get => (Color)GetValue();
        set
        {
            if (CanWrite) Serialized.colorValue = value;
        }
    }

    public AnimationCurve animationCurveValue
    {
        get => (AnimationCurve)GetValue();
        set
        {
            if (CanWrite) Serialized.animationCurveValue = value;
        }
    }

    public Object objectReferenceValue
    {
        get => (Object)GetValue();
        set
        {
            if (CanWrite) Serialized.objectReferenceValue = value;
        }
    }

    public Vector2 vector2Value
    {
        get => (Vector2)GetValue();
        set
        {
            if (CanWrite) Serialized.vector2Value = value;
        }
    }

    public Vector3 vector3Value
    {
        get => (Vector3)GetValue();
        set
        {
            if (CanWrite) Serialized.vector3Value = value;
        }
    }

    public Vector4 vector4Value
    {
        get => (Vector4)GetValue();
        set
        {
            if (CanWrite) Serialized.vector4Value = value;
        }
    }

    public Vector2Int vector2IntValue
    {
        get => (Vector2Int)GetValue();
        set
        {
            if (CanWrite) Serialized.vector2IntValue = value;
        }
    }

    public Vector3Int vector3IntValue
    {
        get => (Vector3Int)GetValue();
        set
        {
            if (CanWrite) Serialized.vector3IntValue = value;
        }
    }

    public Rect rectValue
    {
        get => (Rect)GetValue();
        set
        {
            if (CanWrite) Serialized.rectValue = value;
        }
    }

    public RectInt rectIntValue
    {
        get => (RectInt)GetValue();
        set
        {
            if (CanWrite) Serialized.rectIntValue = value;
        }
    }

    public Bounds boundsValue
    {
        get => (Bounds)GetValue();
        set
        {
            if (CanWrite) Serialized.boundsValue = value;
        }
    }

    public BoundsInt boundsIntValue
    {
        get => (BoundsInt)GetValue();
        set
        {
            if (CanWrite) Serialized.boundsIntValue = value;
        }
    }

    public int intValue
    {
        get => (int)GetValue();
        set
        {
            if (CanWrite) Serialized.intValue = value;
        }
    }
    #endregion

    #region Constructor
    public ClassMember(MemberInfo memberInfo, object target)
    {
        Name = memberInfo.Name;
        DisplayName = ObjectNames.NicifyVariableName(Name);
        TargetObject = target;
        Serialized = null;
        MemberInfo = memberInfo;
        CanWrite = false;
        switch (MemberInfo)
        {
            case FieldInfo fieldInfo:
                ValueType = fieldInfo.FieldType;
                GetValue = (() => fieldInfo.GetValue(target));
                break;
            case PropertyInfo propertyInfo:
                ValueType = propertyInfo.PropertyType;
                GetValue = (() => propertyInfo.GetValue(target));
                break;
            default:
                throw new Exception("Not implemented for this");
        }
    }

    public ClassMember(SerializedProperty serialized, MemberInfo memberInfo)
    {
        Name = serialized.name;
        DisplayName = serialized.displayName;
        TargetObject = serialized.serializedObject.targetObject;
        Serialized = serialized;
        MemberInfo = memberInfo;
        CanWrite = true;
        switch (MemberInfo)
        {
            case FieldInfo fieldInfo:
                ValueType = fieldInfo.FieldType;
                GetValue = (() => fieldInfo.GetValue(TargetObject));
                break;
            case PropertyInfo propertyInfo:
                ValueType = propertyInfo.PropertyType;
                GetValue = (() => propertyInfo.GetValue(TargetObject));
                break;
            default:
                throw new Exception("Not implemented for this");
        }
    }
    #endregion

    /// <returns> Is this member supported in the Omnix-Inspector framework </returns>
    private static bool ShouldIgnore(MemberInfo memberInfo, Type enumarableType)
    {
        if (memberInfo.GetCustomAttribute<ObsoleteAttribute>() != null) return true;
        if (IgnoreProperties.Contains(memberInfo.Name)) return true;
        if (memberInfo.DeclaringType != null && memberInfo.DeclaringType.IsAssignableFrom(enumarableType)) return true;
        return false;
    }

    /// <summary>
    /// Loop through all members of a object. All these memebrs will be non-serialized and not-editable in the Inspector.
    /// </summary>
    public static IEnumerable<ClassMember> GetMembers(object target)
    {
        Type type = target.GetType();
        Type enumType = typeof(IEnumerable);
        foreach (FieldInfo fieldInfo in type.GetFields(Helpers.Flags))
        {
            if (ShouldIgnore(fieldInfo, enumType)) continue;
            yield return new ClassMember(fieldInfo, target);
        }
        
        foreach (PropertyInfo propertyInfo in type.GetProperties(Helpers.Flags))
        {
            if (ShouldIgnore(propertyInfo, enumType)) continue;
            yield return new ClassMember(propertyInfo, target);
        }
    }

    /// <summary>
    /// Loop through all members of a object. All these memebrs will be serialized and editable in the Omnix Inspector.
    /// </summary>
    /// <returns></returns>
    public static IEnumerable<ClassMember> GetMembers(object target, [NotNull] SerializedObject serializedObject)
    {
        Type type = target.GetType();
        Type enumType = typeof(IEnumerable);
        foreach (PropertyInfo propertyInfo in type.GetProperties(Helpers.Flags))
        {
            if (ShouldIgnore(propertyInfo, enumType)) continue;
            yield return new ClassMember(propertyInfo, target);
        }


        Dictionary<string, MemberInfo> serializedMembers = new Dictionary<string, MemberInfo>();
        foreach (FieldInfo fieldInfo in type.GetFields(Helpers.Flags))
        {
            if (ShouldIgnore(fieldInfo, enumType)) continue;

            if (IsSerialized(fieldInfo))
            {
                serializedMembers.Add(fieldInfo.Name, fieldInfo);
            }
            else yield return new ClassMember(fieldInfo, target);
        }

        SerializedProperty iterator = serializedObject.GetIterator();
        iterator.Next(true);
        if (IsIncluded(iterator))
        {
            SerializedProperty iterCopy = iterator.Copy();
            yield return new ClassMember(iterCopy, serializedMembers[iterCopy.name]);
        }
        while (iterator.Next(false))
        {
            if (IsIncluded(iterator))
            {
                SerializedProperty iterCopy = iterator.Copy();
                yield return new ClassMember(iterCopy, serializedMembers[iterCopy.name]);
            }
        }


        bool IsIncluded(SerializedProperty serProp) => !IgnoreProperties.Contains(serProp.name) && serializedMembers.ContainsKey(serProp.name);

        bool IsSerialized(FieldInfo fieldInfo) => !fieldInfo.IsStatic
                                                  && ((fieldInfo.IsPublic && fieldInfo.GetCustomAttribute<NonSerializedAttribute>() == null)
                                                      || (fieldInfo.GetCustomAttribute<SerializeField>() != null));
    }
    
    /// <summary>
    /// Get dictionary of all the members of a object. With key being Name of the member and value being Serialized Object
    /// </summary>
    public static Dictionary<string, ClassMember> GetMembersDictionary(SerializedObject serializedObject)
    {
        Dictionary<string,ClassMember> properties = new Dictionary<string, ClassMember>();
        foreach (ClassMember member in GetMembers(serializedObject.targetObject, serializedObject))
        {
            if (member.CanWrite || Helpers.SupportedTypes.Contains(member.ValueType) || member.ValueType.IsSubclassOf(typeof(Object)))
            {
                properties.Add(member.Name, member);
            }
        }
        return properties;
    }
}
