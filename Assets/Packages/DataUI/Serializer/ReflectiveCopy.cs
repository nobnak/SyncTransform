using UnityEngine;
using System.Reflection;
using System.Collections.Generic;
using System.Text;

namespace DataUI {
	public static class ReflectiveCopy {
		[System.Flags]
		public enum FieldClassEnum { 
			Primitive = 1, Enum = 1 << 1, String = 1 << 2, Struct = 1  << 3, Class = 1 << 4,
			Any = Primitive | Enum | String | Struct | Class
		}

		public const BindingFlags FIELD_BINDING_ATTR = BindingFlags.Instance | BindingFlags.Public;

		public static IEnumerable<FieldInfo> GetFields<T>(T obj, FieldClassEnum fieldClass) {
			foreach (var field in typeof(T).GetFields(FIELD_BINDING_ATTR)) {
				var fc = FieldClass(field.FieldType);
				if ((fc & fieldClass) != 0)
					yield return field;
			}
		}
		public static IEnumerable<FieldInfo> GetFields<T>(T obj) {
			return GetFields(obj, FieldClassEnum.Any);
		}
		public static string Print<T>(T obj) {
			var buf = new StringBuilder(string.Format("Type {0}:\n", obj.GetType().Name));
			foreach (var field in GetFields(obj, FieldClassEnum.Any)) {
				var ft = field.FieldType;
				var fc = FieldClass(ft);
				buf.AppendFormat("{0} = {1} of {2}({3})\n", field.Name, field.GetValue(obj), ft.Name, fc);
			}
			return buf.ToString();
		}
		public static void CopyFields<SRC, DST>(SRC src, DST dst, IEnumerable<string> fieldNames) {
			var srcType = typeof(SRC);
			var dstType = typeof(DST);
			foreach (var fieldName in fieldNames) {
				var srcField = srcType.GetField(fieldName);
				var dstField = dstType.GetField(fieldName);
				if (srcField == null) {
					Debug.LogFormat("Field Not Found : {0} of {1}", fieldName, srcType.Name);
					continue;
				} else if (dstField == null) {
					Debug.LogFormat("Field Not Found : {0} of {1}", fieldName, dstType.Name);
					continue;
				} else if (srcField.FieldType != dstField.FieldType) {
					Debug.LogFormat("Fields not math of {0} : {1} != {2}", fieldName, srcField.FieldType.Name, dstField.FieldType.Name);
					continue;
				}
				try {
					var val = srcField.GetValue(src);
					dstField.SetValue(dst, val);
					//Debug.LogFormat("Copy Value {0} = {1} from {2} to {3}", fieldName, val, srcType.Name, dstType.Name);
				} catch (System.Exception e) {
					Debug.Log(string.Format("Exception on field {0}:\n {1}", fieldName, e));
				}
			}
		}
		public static void CopyFieldsOfType<SRC, DST, T>(SRC src, DST dst, T fieldNamesFrom) {
			var fieldNames = new List<string>();
			foreach (var field in GetFields(fieldNamesFrom))
				fieldNames.Add(field.Name);
			CopyFields(src, dst, fieldNames);
		}
		public static void CopyFieldsOfSrc<SRC, DST>(SRC src, DST dst) {
			CopyFieldsOfType(src, dst, src);
		}
		public static void CopyFieldsOfDst<SRC, DST>(SRC src, DST dst) {
			CopyFieldsOfType(src, dst, dst);
		}

		public static FieldClassEnum FieldClass(System.Type fieldType) {
			if (fieldType.IsPrimitive)
				return FieldClassEnum.Primitive;
			else if (fieldType.IsEnum)
				return FieldClassEnum.Enum;
			else if (fieldType == typeof(string))
				return FieldClassEnum.String;
			else if (fieldType.IsValueType)
				return FieldClassEnum.Struct;
			else
				return FieldClassEnum.Class;
		}
	}
}