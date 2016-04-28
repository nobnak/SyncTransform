using System.IO;
using System.Xml.Serialization;
using UnityEngine;

namespace DataUI {
	public static class Serializer {
		public static void Save<T>(T data, string savePath) {
			var fullPath = DataFullPath(savePath);
			using (var writer = new StreamWriter(fullPath, false, System.Text.Encoding.UTF8))
				Serialize(writer, data);
			Debug.LogFormat ("Serialized of type {0} at {1}", typeof(T), fullPath);
		}
		public static T Load<T>(string savePath, T defaultData) {
			try {
				if (!string.IsNullOrEmpty(savePath)) {
					var fullPath = DataFullPath(savePath);
					if (File.Exists(fullPath)) {
						using (var reader = new StreamReader(fullPath, System.Text.Encoding.UTF8))
							return Deserialize<T>(reader);
					} else {
						Debug.LogFormat("Serialized Data Not found of type {0} at {1}", typeof(T), fullPath);
					}
				}
			} catch (System.Exception e) {
				Debug.LogError(e);
			}
			return defaultData;
		}
		
		public static string DataFullPath(string savePath) { 
			return Path.Combine(Application.streamingAssetsPath, savePath);
		}
		public static T Deserialize<T>(StreamReader reader) {
			return (T)(new XmlSerializer(typeof(T)).Deserialize (reader));
		}
		public static void Serialize<T>(StreamWriter writer, T data) {
			new XmlSerializer(typeof(T)).Serialize(writer, data);
		}
	}
}
