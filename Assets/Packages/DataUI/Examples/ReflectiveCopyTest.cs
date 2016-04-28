using UnityEngine;
using System.Collections;

namespace DataUI {
	public class ReflectiveCopyTest : MonoBehaviour {
		public Data data;

		void Start () {
			Debug.Log(ReflectiveCopy.Print(data));
		}

    	[System.Serializable]
    	public class Data {
    		public enum TeamEnum { Alpha = 0, Bravo, Charlie }

    		public int intData;
    		public float floatData;
    		public string stringData;
    		public TeamEnum team;
    		public InnerClass innerClass;
    		public InnerStruct innerStruct;

    		[System.Serializable]
    		public class InnerClass {
    			public int innerIntData;
    		}

    		[System.Serializable]
    		public struct InnerStruct {
    			public int innerIntData;
    		}
    	}
    }
}