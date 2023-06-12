using System;
using System.Diagnostics.CodeAnalysis;
using Omnix.Core;
using Omnix.Data;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Omnix.InputFields
{
    /// <summary>
    /// Drawer for Unity builtin classes
    /// </summary>
    public class IfDefault : InputFieldBase
    {
        public IfDefault(ClassMember targetMember, PropertyData myData, LayoutBase parent) : base(targetMember, myData, parent)
        {
            DrawAction = GetDrawer();
        }

        private Action GetDrawer()
        {
            // @formatter:off
            if (TargetMember.ValueType == typeof(Color))                return () => TargetMember.colorValue = EditorGUI.ColorField(TotalRect, MyData.content, TargetMember.colorValue);
            if (TargetMember.ValueType == typeof(Vector2))              return () => TargetMember.vector2Value = EditorGUI.Vector2Field(TotalRect, MyData.content, TargetMember.vector2Value);
            if (TargetMember.ValueType == typeof(Vector3))              return () => TargetMember.vector3Value = EditorGUI.Vector3Field(TotalRect, MyData.content, TargetMember.vector3Value);
            if (TargetMember.ValueType == typeof(Vector4))              return () => TargetMember.vector4Value = EditorGUI.Vector4Field(TotalRect, MyData.content, TargetMember.vector4Value);
            if (TargetMember.ValueType == typeof(Rect))                 return () => TargetMember.rectValue = EditorGUI.RectField(TotalRect, MyData.content, TargetMember.rectValue);
            if (TargetMember.ValueType == typeof(AnimationCurve))       return () => TargetMember.animationCurveValue = EditorGUI.CurveField(TotalRect, MyData.content, TargetMember.animationCurveValue);
            if (TargetMember.ValueType == typeof(Bounds))               return () => TargetMember.boundsValue = EditorGUI.BoundsField(TotalRect, MyData.content, TargetMember.boundsValue);
            if (TargetMember.ValueType == typeof(Vector2Int))           return () => TargetMember.vector2IntValue = EditorGUI.Vector2IntField(TotalRect, MyData.content, TargetMember.vector2IntValue);
            if (TargetMember.ValueType == typeof(Vector3Int))           return () => TargetMember.vector3IntValue = EditorGUI.Vector3IntField(TotalRect, MyData.content, TargetMember.vector3IntValue);
            if (TargetMember.ValueType == typeof(RectInt))              return () => TargetMember.rectIntValue = EditorGUI.RectIntField(TotalRect, MyData.content, TargetMember.rectIntValue);
            if (TargetMember.ValueType == typeof(BoundsInt))            return () => TargetMember.boundsIntValue = EditorGUI.BoundsIntField(TotalRect, MyData.content, TargetMember.boundsIntValue);
            if (TargetMember.ValueType == typeof(Sprite))               return () => TargetMember.objectReferenceValue = EditorGUI.ObjectField(TotalRect, MyData.content, TargetMember.objectReferenceValue, typeof(Sprite), allowSceneObjects: false);
            if (TargetMember.CanWrite)                                  return () => EditorGUI.PropertyField(TotalRect, TargetMember.Serialized, MyData.content, true);
            if (TargetMember.ValueType.IsSubclassOf(c: typeof(Object))) return () => EditorGUI.ObjectField(TotalRect, MyData.content, TargetMember.objectReferenceValue, TargetMember.ValueType, allowSceneObjects: false);
                                                                        return () => GUI.Label(TotalRect, $"Not supported for type {TargetMember.ValueType}");
            // @formatter:on
        }
    }
}