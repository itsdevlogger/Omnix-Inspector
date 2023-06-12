using System;
using Omnix.Core;
using Omnix.Preview;
using UnityEditor;

namespace Omnix.Windows
{
    /// <summary>
    /// Base class for Omnix Window
    /// </summary>
    public abstract class OmnixWindowBase : EditorWindow
    {
        [MenuItem("Omnix/Elements", validate = true)]
        [MenuItem("Omnix/Hierarchy", validate = true)]
        [MenuItem("Omnix/Inspector", validate = true)]
        public static bool CanOpenWindow() => Actives.Preview != null;

        [MenuItem("Omnix/Init", validate = true)]
        public static bool CanOpenPreviewWindow() => Actives.Preview == null;


        [MenuItem("Omnix/Elements", priority = 1)]
        private static void OpenElements()
        {
            if (Actives.Elements == null)
                Actives.Elements = OpenWindow<OmnixElementsWindow>("Elements");
        }
        
        [MenuItem("Omnix/Hierarchy", priority = 2)]
        private static void OpenHierarchy()
        {
            if (Actives.Hierarchy == null)
                Actives.Hierarchy = OpenWindow<OmnixHierarchyWindow>("Hierarchy");
        }

        [MenuItem("Omnix/Inspector", priority = 3)]
        private static void OpenInspector()
        {
            if (Actives.Inspector == null)
                Actives.Inspector = OpenWindow<OmnixInspectorWindow>("Inspector");
        }

        [MenuItem("Omnix/Init", priority = 0)]
        private static void OpenInitWindow()
        {
            OpenWindow<OmnixInitWindow>("Init");
        }

        public static class Actives
        {
            public static OmnixPreviewWindow Preview;
            public static OmnixHierarchyWindow Hierarchy;
            public static OmnixElementsWindow Elements;
            public static OmnixInspectorWindow Inspector;

            public static LayoutPreview FirstLayoutPreview => (LayoutPreview)FirstLayoutWrapper.Current.FirstLayout;
            
            public static void OpenAll()
            {
                Helpers.GetInternalClasses(out Type hierarchy, out Type inspector, out Type gameView, out Type console);
                if (Preview == null)   Preview = OpenWindow<OmnixPreviewWindow>("Preview", gameView);
                if (Elements == null)  Elements = OpenWindow<OmnixElementsWindow>("Elements", console);
                if (Hierarchy == null) Hierarchy = OpenWindow<OmnixHierarchyWindow>("Hierarchy", hierarchy);
                if (Inspector == null) Inspector = OpenWindow<OmnixInspectorWindow>("Inspector", inspector);
            }

            public static void CloseAll(OmnixWindowBase exclude)
            {
                if (Preview != null && Preview != exclude) Preview.Close();
                if (Hierarchy != null && Hierarchy != exclude) Hierarchy.Close();
                if (Elements != null && Elements != exclude) Elements.Close();
                if (Inspector != null && Inspector != exclude) Inspector.Close();
            }

        }

        private static TWindow OpenWindow<TWindow>(string title, Type nextTo = null)
        where TWindow : OmnixWindowBase
        {
            TWindow window;
            if (nextTo == null) window = GetWindow<TWindow>(title);
            else window = GetWindow<TWindow>(title, nextTo);
            Helpers.OnRepaint += window.Repaint;
            window.Show();
            return window;
        }

        protected virtual void OnDestroy()
        {
            Helpers.OnRepaint -= this.Repaint;
        }
    }
}