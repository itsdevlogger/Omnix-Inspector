using System;
using Omnix.Core;
using UnityEngine;
using Random = System.Random;

namespace Omnix.Data
{
    /// <summary>
    /// Base class for all Data Classes.
    /// </summary>
    [Serializable]
    public abstract class DataClassBase
    {
        /// <summary>
        /// Default callback target 
        /// </summary>
        public const string CallbackTargetForNone = "(None)";

        /// <summary>
        /// Characters which are used to create guid for an element
        /// </summary>
        public static readonly string RandomGuidChars = "1234567890qwertyuiopasdfghjklzxcvbnmQWERTYUIOPASDFGHJKLZXCVBNM_";

        /// <summary> Unique ID for this element </summary>
        public string GUID => _myGUID;

        /// <summary> Unique ID for this element </summary>
        [SerializeField, HideInInspector] private string _myGUID;

        public ShowIfTargetType showIfTargetType;
        public ElementType elementType;
        public OmnixStyleSheet styleSheet;
        public RectOffset padding;

        public string nameInKsHierarchy;
        public string callbackTarget;
        public string showIfTarget;
        public bool hideDontDisable;

        // Used only while unity is deserialize
        protected DataClassBase()
        {
            if (FirstLayoutWrapper.Current != null) FirstLayoutWrapper.Current.DrawerData.Register(this);
            styleSheet = new OmnixStyleSheet();
            padding = new RectOffset();
        }

        // So we dont need to set ElementGuid while deserializing
        protected DataClassBase(bool _ignored)
        {
            _myGUID = RandomGuid();
            if (FirstLayoutWrapper.Current != null) FirstLayoutWrapper.Current.DrawerData.Register(this);
            styleSheet = new OmnixStyleSheet();
            padding = new RectOffset();
        }

        /// <summary>
        /// Copy all data of this instace to a new instance.
        /// </summary>
        /// <returns></returns>
        public abstract DataClassBase Copy();

        /// <summary>
        /// Get a new random GUID.
        /// </summary>
        private static string RandomGuid()
        {
            var random = new Random();
            char[] guid = new char[30];
            int count = RandomGuidChars.Length;
            for (int i = 0; i < 30; i++)
            {
                guid[i] = RandomGuidChars[random.Next(0, count)];
            }
            return new string(guid);
        }

    }
}