using UnityEngine;

namespace Omnix.Demos
{
    public class ButtonsDemo : MonoBehaviour
    {
        private void PrivateMethod() => Debug.Log("PrivateMethod Called");
        public void PublicMethod() => Debug.Log("PublicMethod Called");
        
        private static void PrivateStaticMethod() => Debug.Log("PrivateStaticMethod Called");
        public static void PublicStaticMethod() => Debug.Log("PublicStaticMethod Called");
    }
}