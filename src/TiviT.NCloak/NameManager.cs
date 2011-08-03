using System;
using System.Collections.Generic;
using Mono.Cecil;
using Mono.Cecil.Rocks;
namespace TiviT.NCloak
{
	public class NameManager
	{
		private readonly Dictionary<string, CharacterSet> namingTypes;
		
		
		public NameManager()
		{
			namingTypes = new Dictionary<string, CharacterSet>();
		}

		public void SetCharacterSet(string prefix, CharacterSet characterSet)
		{
			if (namingTypes.ContainsKey(prefix)){
				namingTypes[prefix] = characterSet;
			}
			else{
				namingTypes.Add(prefix, characterSet);
			}
		}
		

		public string GenerateName(NamingType type,Object assemblyObj)
		{
			string prefix="";
			if (type==NamingType.Type)
			{
				TypeDefinition typeDef=(TypeDefinition)assemblyObj;
				if (!ObfuscationDetector.isNameObfuscated(typeDef.Name)){
					return typeDef.Name;
				}
				prefix=SmartRenamer.typeRenamer(typeDef);
			}
			else if (type==NamingType.Method)
			{
				MethodDefinition methodDef=(MethodDefinition)assemblyObj;
				if (!ObfuscationDetector.isNameObfuscated(methodDef.Name)){
					return methodDef.Name;
				}
				prefix=SmartRenamer.methodRenamer(methodDef);
			}
			else if (type==NamingType.Property)
			{
				PropertyDefinition propDef=(PropertyDefinition)assemblyObj;
				if (!ObfuscationDetector.isNameObfuscated(propDef.Name)){
					return propDef.Name;
				}
				prefix=SmartRenamer.propertyRenamer(propDef);
			}
			else if (type==NamingType.Field)
			{
				FieldDefinition fieldDef=(FieldDefinition)assemblyObj;
				if (!ObfuscationDetector.isNameObfuscated(fieldDef.Name)){
					return fieldDef.Name;
				}
				prefix=SmartRenamer.fieldRenamer(fieldDef);
			}
			if (prefix==""){
				throw new Exception("Bad prefix");
			}
			
			if (!namingTypes.ContainsKey(prefix)){
				SetCharacterSet(prefix, DefaultCharacterSet);
			}
			return prefix + namingTypes[prefix].Generate();
		}

		
		private static CharacterSet DefaultCharacterSet
		{
			get { return new CharacterSet('0', '9'); }
		}
		
		
	}
	
	public static class SmartRenamer
	{
		
		public static string typeRenamer(TypeDefinition type)
		{
			return "class_";
		}
		
		public static string methodRenamer(MethodDefinition method)
		{
			if (!ObfuscationDetector.isNameObfuscated(method.Name)){
				return method.Name;
			}
			return "method_";
		}
		
		public static string fieldRenamer(FieldDefinition field)
		{
			if (!ObfuscationDetector.isNameObfuscated(field.Name)){
				return field.Name;
			}
			return "field_";
		}
		
		public static string propertyRenamer(PropertyDefinition prop)
		{
			if (!ObfuscationDetector.isNameObfuscated(prop.Name)){
				return prop.Name;
			}
			return "prop_";
		}
	}
	
	public static class ObfuscationDetector
	{

		public static bool isNameObfuscated(string input)
		{
			if (input.Length<=2){//Dotfuscator: names like a,aa,b and so on
				return true;
			}
			if (hasUnPrintableChars(input)){
				return true;
			}
			if (getDigitCount(input)>=4){//CryptoHash name, like c45f34d3l524
				return true;
			}
			return false;
		}
		
		private static int getDigitCount(string input)
		{
			int cnt=0;
			foreach (char c in input){
				if (char.IsDigit(c)){
					cnt++;
				}
			}
			return cnt;
		}
		
		private static bool hasUnPrintableChars(string input)
		{
			foreach (char c in input){
				if (!char.IsSymbol(c) && !char.IsLetterOrDigit(c) && !char.IsPunctuation(c)){
					return true;
				}
			}
			return false;
		}
	}
}
