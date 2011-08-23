using System.Collections.Generic;
using System;
using Mono.Cecil;

namespace TiviT.NCloak.Mapping
{
	public class AssemblyMapping
	{
		private readonly string assemblyName;
		private readonly Dictionary<TypeReference, TypeMapping> typeMappingTable;
		private readonly Dictionary<string, string> obfuscatedToOriginalMapping;
		
		private NameManager nameManager;
		
		

		public AssemblyMapping(string assemblyName)
		{
			this.assemblyName = assemblyName;
			typeMappingTable = new Dictionary<TypeReference, TypeMapping>();
			obfuscatedToOriginalMapping = new Dictionary<string, string>();
			nameManager=new NameManager();
		}

		public NameManager NameManager {
			get { return nameManager; }
		}
		
		public string AssemblyName
		{
			get { return assemblyName; }
		}

		
		private string createTypeFullName(TypeReference type,string typeName)
		{
			string typeFullName = typeName;
			if (type.DeclaringType!=null){
				typeFullName=type.DeclaringType.Name+"."+typeFullName;
			}
			typeFullName = type.Namespace + "." + typeFullName;
			return typeFullName;
		}
		
		public TypeMapping AddType(TypeReference type)
		{
			string obfuscatedTypeName=nameManager.GenerateName(NamingType.Type,type);
			string typeFullName = createTypeFullName(type,type.Name);
			
			TypeMapping typeMapping = new TypeMapping(typeFullName, obfuscatedTypeName);
			typeMappingTable.Add(type, typeMapping);
			//Add a reverse mapping
			if (!String.IsNullOrEmpty(obfuscatedTypeName)){
				string obfuscatedFullName=createTypeFullName(type,obfuscatedTypeName);
				obfuscatedToOriginalMapping.Add(obfuscatedFullName, typeFullName);
			}
			return typeMapping;
		}

		public void ChangeTypeMapping(TypeReference type,string newObfuscatedName)
		{
			string oldFullName=typeMappingTable[type].ObfuscatedTypeName;
			obfuscatedToOriginalMapping.Remove(oldFullName);
			string fullNewName=createTypeFullName(type,newObfuscatedName);
			
			obfuscatedToOriginalMapping.Add(fullNewName,typeMappingTable[type].TypeName);
			typeMappingTable[type].ObfuscatedTypeName=newObfuscatedName;
		}
		
		
		public TypeMapping GetTypeMapping(TypeReference type)
		{
			if (type == null){
				return null;
			}
			if (typeMappingTable.ContainsKey(type)){
				return typeMappingTable[type];
			}
			return null;
		}
	}
}
