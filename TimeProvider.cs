using UnityEngine;
using System.Collections;

namespace SyncTransformSystem {	
	public class TimeProvider {
        static TimeProvider _instance;

        public static TimeProvider Instance { 
            get { return (_instance == null ? _instance = new TimeProvider () : _instance); }
        }

		public virtual float Time { get { return UnityEngine.Time.time; } }
	}
}
