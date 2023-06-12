using System;
using UnityEngine;

namespace Omnix
{
    [ExecuteAlways]
    public class OmnixEditorContext : MonoBehaviour
    {
        public static OmnixEditorContext Context { get; private set; }
        public static bool IsEditing { get; private set; } = false;

        private void OnDestroy()
        {
            IsEditing = false;
        }

        public static OmnixEditorContext GetNew()
        {
            IsEditing = true;
            GameObject editorContext = new GameObject("(Omnix Editor Context)");
            Context = editorContext.AddComponent<OmnixEditorContext>();
            return Context;
        }

        public static void DestroyCurrent()
        {
            if (Context == null) return;
            DestroyImmediate(Context.gameObject);
        }
    }
}