using System;
using System.Collections.Generic;
using Mono.Cecil;
using System.Collections;

namespace TiviT.NCloak.Mapping
{
	public class TypeMapping
	{
		private readonly string typeName;
		private  string obfuscatedTypeName;

		private readonly Dictionary<MethodReference, MemberMapping> methods;
		private readonly Dictionary<PropertyReference, MemberMapping> properties;
		private readonly Dictionary<string, MemberMapping> fields;

		private readonly Dictionary<string, MethodReference> obfuscatedMethods;
		private readonly Dictionary<string, PropertyReference> obfuscatedProperties;
		private readonly Dictionary<string, FieldReference> obfuscatedFields;
		
		private NameManager nameManager;
		
		
		public TypeMapping(string typeName, string obfuscatedTypeName)
		{
			this.typeName = typeName;
			this.obfuscatedTypeName = obfuscatedTypeName;

			methods = new Dictionary<MethodReference, MemberMapping>();
			properties = new Dictionary<PropertyReference, MemberMapping>();
			fields = new Dictionary<string, MemberMapping>();

			obfuscatedMethods = new Dictionary<string, MethodReference>();
			obfuscatedProperties = new Dictionary<string, PropertyReference>();
			obfuscatedFields = new Dictionary<string, FieldReference>();
			
			nameManager=new NameManager();
		}

		public NameManager NameManager {
			get { return nameManager; }
		}
		
		
		public string ObfuscatedTypeName
		{
			get { return obfuscatedTypeName; }
			set {obfuscatedTypeName=value;}
		}

		
		public string TypeName
		{
			get { return typeName; }
		}

		
		
		public string AddMethodMapping(MethodReference method)
		{
			string obfuscatedMethodName=nameManager.GenerateName(NamingType.Method,method);
			AddMethodMapping(method,obfuscatedMethodName);
			return methods[method].ObfuscatedMemberName;
		}
		
		public void AddMethodMapping(MethodReference method,string obfuscatedMethodName)
		{
			if (method == null) throw new ArgumentNullException("method");
			string methodName = method.Name;
			if (!methods.ContainsKey(method))
			{
				methods.Add(method, new MemberMapping(methodName, obfuscatedMethodName));
				if (obfuscatedMethods.ContainsKey(obfuscatedMethodName)){
					return;
				}
				obfuscatedMethods.Add(obfuscatedMethodName, method);
			}
		}
		
		public void AddMethodMappingSig(MethodReference method,string obfuscName,TypeDefinition typeDef)
		{
			if (method == null) throw new ArgumentNullException("method");
			string methodSig=computeMethodSignature(method);
			foreach (MethodDefinition methodInType in typeDef.Methods){
				if (methodSig==computeMethodSignature(methodInType)){
					AddMethodMapping(methodInType,obfuscName);
					return;
				}
			}
			//throw new ArgumentNullException("method sig");
		}
		
		public void ChangeMethodMapping(MethodReference method,string newObfuscatedName)
		{
			if (!methods.ContainsKey(method)){
				throw new Exception("Method not found!");
			}
			string oldName=methods[method].ObfuscatedMemberName;
			methods.Remove(method);
			obfuscatedMethods.Remove(oldName);
			AddMethodMapping(method,newObfuscatedName);
		}

		public string AddPropertyMapping(PropertyReference property)
		{
			string obfuscatedPropertyName=nameManager.GenerateName(NamingType.Property,property);
			AddPropertyMapping(property,obfuscatedPropertyName);
			return properties[property].ObfuscatedMemberName;
		}
		
		
		
		public void AddPropertyMapping(PropertyReference property,string obfuscatedPropertyName)
		{
			if (property == null) throw new ArgumentNullException("property");
			if (!properties.ContainsKey(property))
			{
				properties.Add(property, new MemberMapping(property.Name, obfuscatedPropertyName));
				obfuscatedProperties.Add(obfuscatedPropertyName, property);
			}
		}

		public void AddPropertyMappingSig(PropertyReference property,string obfuscName,TypeDefinition typeDef)
		{
			string propertySig=property.Name;
			foreach (PropertyDefinition propertyInType in typeDef.Properties){
				if (propertySig==propertyInType.Name){
					AddPropertyMapping(propertyInType,obfuscName);
					return;
				}
			}
			//throw new ArgumentNullException("method sig");
		}
		
		public void AddFieldMapping(FieldReference field)
		{
			string obfuscatedFieldName=nameManager.GenerateName(NamingType.Field,field);
			if (field == null) throw new ArgumentNullException("field");
			string fieldName = field.Name;
			if (!fields.ContainsKey(fieldName))
			{
				fields.Add(fieldName, new MemberMapping(fieldName, obfuscatedFieldName));
				obfuscatedFields.Add(obfuscatedFieldName, field);
			}
		}

		
		public bool HasMethodMapping(MethodReference method)
		{
			if (method == null) throw new ArgumentNullException("method");
			//string methodName = method.Name;
			return methods.ContainsKey(method);
		}
		
		public bool HasMethodMappingSig(MethodReference method)
		{
			string methodSig=computeMethodSignature(method);
			foreach(KeyValuePair<MethodReference, MemberMapping> entry in methods)
			{
				if (computeMethodSignature(entry.Key).ToLower()==methodSig.ToLower()){
					return true;
				}
			}
			return false;
		}

		
		public bool HasPropertyMapping(PropertyReference property)
		{
			if (property == null) throw new ArgumentNullException("property");
			string propertyName = property.Name;
			return properties.ContainsKey(property);
		}
		
		public bool HasPropertyMappingSig(PropertyReference property)
		{
			foreach(KeyValuePair<PropertyReference, MemberMapping> entry in properties)
			{
				if (property.Name==entry.Key.Name){
					return true;
				}
			}
			return false;
		}

		
		public bool HasFieldMapping(FieldReference field)
		{
			if (field == null) throw new ArgumentNullException("field");
			string fieldName = field.Name;
			return fields.ContainsKey(fieldName);
		}

		
		public string GetObfuscatedMethodName(MethodReference method)
		{
			if (method == null) throw new ArgumentNullException("method");
			if (HasMethodMapping(method)){
				return methods[method].ObfuscatedMemberName;
			}
			return null;
		}
		
		public string GetObfuscatedMethodNameSig(MethodReference method)
		{
			string methodSig=computeMethodSignature(method);
			foreach(KeyValuePair<MethodReference, MemberMapping> entry in methods)
			{
				if (computeMethodSignature(entry.Key).ToLower()==methodSig.ToLower()){
					string res=methods[entry.Key].ObfuscatedMemberName;
					return methods[entry.Key].ObfuscatedMemberName;
				}
			}
			throw new ArgumentNullException("method name");
		}

		
		public string GetObfuscatedPropertyName(PropertyReference property)
		{
			if (property == null) throw new ArgumentNullException("property");
			if (HasPropertyMapping(property))
			{
				string propertyName = property.Name;
				return properties[property].ObfuscatedMemberName;
			}
			return null;
		}
		
		public string GetObfuscatedPropertyNameSig(PropertyReference property)
		{
			foreach(KeyValuePair<PropertyReference, MemberMapping> entry in properties)
			{
				if (property.Name==entry.Key.Name){
					return entry.Value.ObfuscatedMemberName;
				}
			}
			return null;
		}

		
		public string GetObfuscatedFieldName(FieldReference field)
		{
			if (field == null) throw new ArgumentNullException("field");
			if (HasFieldMapping(field))
			{
				string fieldName = field.Name;
				return fields[fieldName].ObfuscatedMemberName;
			}
			return null;
		}

		
		public bool HasMethodBeenObfuscated(string obfuscatedMethodName)
		{
			return obfuscatedMethods.ContainsKey(obfuscatedMethodName);
		}

		
		public bool HasPropertyBeenObfuscated(string obfuscatedPropertyName)
		{
			return obfuscatedProperties.ContainsKey(obfuscatedPropertyName);
		}

		
		public bool HasFieldBeenObfuscated(string obfuscatedFieldName)
		{
			return obfuscatedFields.ContainsKey(obfuscatedFieldName);
		}
		
		private string computeMethodSignature(MethodReference method)
		{
			string result=method.Name;
			result+=method.ReturnType.FullName;
			foreach (ParameterDefinition param in method.Parameters){
				result+=param.ParameterType.FullName;
			}
			return result;
		}
	}
}
