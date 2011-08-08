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
			if (type.DeclaringType!=null){
				return "ChildClass_";
			}
			if (type.IsInterface){
				return "Interface_";
			}
			if (type.IsEnum){
				return "Enum_";
			}
			if (type.BaseType!=null){
				if (type.BaseType.Name=="Form"){
					return "Form_";
				}
			}
			if (type.Methods.Count<4){
				return "TinyClass_";
			}
			return "Class_";
		}
		
		public static string methodRenamer(MethodDefinition method)
		{
			if (method.Parameters.Count==2){
				if (method.Parameters[0].ParameterType.Name=="Object"){
					if (method.Parameters[1].ParameterType.Name=="EventArgs"){
						return "eventHandler_";
					}
				}
			}
			if (method.Parameters.Count==1){
				if (hasWinFormComponent(method.Parameters[0].ParameterType.Name)){
				    	return "winformSetter_";
				    }
			}
			if (hasWinFormComponent(method.ReturnType.Name)){
			    	return "winformGetter_";
			    }
			
			if (method.ReturnType.Name=="Boolean"){
				return "boolMethod_";
			}
			if (method.ReturnType.Name=="String" && method.Parameters.Count==1){
				if (method.Parameters[0].ParameterType.Name=="Int32"){
					return "possibleDecryptor_";
				}
			}
			
			return "method_";
		}
		
		public static string fieldRenamer(FieldDefinition field)
		{
			if (field.FieldType.Name=="Button"){
				return "button_";
			}
			if (field.FieldType.Name=="TextBox"){
				return "textbox_";
			}
			if (field.FieldType.Name=="ComboBox"){
				return "combobox_";
			}
			if (field.FieldType.Name=="CheckBox"){
				return "checkbox_";
			}
			if (field.FieldType.Name=="Label"){
				return "label_";
			}
			
			if (field.FieldType.Name=="Boolean"){
				return "bool_";
			}
			if (field.FieldType.Name=="String"){
				return "string_";
			}
		
			return "field_";
		}
		
		public static string propertyRenamer(PropertyDefinition prop)
		{
			
			return "prop_";
		}
		
		private static bool hasWinFormComponent(string name)
		{
			if (name=="TextBox"){
				return true;
			}else if (name=="Button"){
				return true;
			}else if (name=="ComboBox"){
				return true;
			}else if (name=="CheckBox"){
				return true;
			}else if (name=="Label"){
				return true;
			}else if (name=="PictureBox"){
				return true;
			}else if (name=="ListBox"){
				return true;
			}
			return false;
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
