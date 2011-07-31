using System;
using System.Collections.Generic;
using Mono.Cecil;
using Mono.Cecil.Rocks;
namespace TiviT.NCloak
{
	public class NameManager
	{
		private readonly Dictionary<NamingType, CharacterSet> namingTypes;
		
		
		public NameManager()
		{
			namingTypes = new Dictionary<NamingType, CharacterSet>();
		}

		public void SetCharacterSet(NamingType type, CharacterSet characterSet)
		{
			if (namingTypes.ContainsKey(type))
				namingTypes[type] = characterSet;
			else
				namingTypes.Add(type, characterSet);
		}
		
		public void ResetNonTypeCounters()
		{
			ResetCounter(NamingType.Field);
			ResetCounter(NamingType.Method);
			ResetCounter(NamingType.Property);
		}
		
		public void ResetCounter(NamingType type)
		{
			if (!namingTypes.ContainsKey(type)){
				return;
			}else{
				namingTypes[type].resetCounter();
			}
		}

		public string GenerateName(NamingType type,Object assemblyObj)
		{
			if (type==NamingType.Type)
			{
				TypeDefinition typeDef=(TypeDefinition)assemblyObj;
				return resolveName(type,typeDef.Name);
			}
			if (type==NamingType.Method)
			{
				MethodDefinition methodDef=(MethodDefinition)assemblyObj;
				return resolveName(type,methodDef.Name);
			}
			if (type==NamingType.Property)
			{
				PropertyDefinition propDef=(PropertyDefinition)assemblyObj;
				return resolveName(type,propDef.Name);
			}
			if (type==NamingType.Field)
			{
				FieldDefinition fieldDef=(FieldDefinition)assemblyObj;
				return resolveName(type,fieldDef.Name);
			}
			return GenerateName(type);
		}
		
		private string resolveName(NamingType type,string name)
		{
			if (!ObfuscationDetector.isNameObfuscated(name)){
				return name;
			}
			string prefix="unknown";
			if (type==NamingType.Field){
				prefix="field_";
			}
			else if (type==NamingType.Property){
				prefix="property_";
			}
			else if (type==NamingType.Method){
				prefix="method_";
			}
			else if (type==NamingType.Type){
				prefix="class_";
			}
			return prefix+GenerateName(type);
		}

		
		public string GenerateName(NamingType type)
		{
			//Check if such type exists
			if (!namingTypes.ContainsKey(type))
				SetCharacterSet(type, DefaultCharacterSet);

			//Generate a new name
			if (type == NamingType.Field) //For fields append an _ to make sure it differs from properties etc
				return "_" + namingTypes[type].Generate();
			return namingTypes[type].Generate();
		}
		
		private static CharacterSet DefaultCharacterSet
		{
			get { return new CharacterSet('0', '9'); }
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
