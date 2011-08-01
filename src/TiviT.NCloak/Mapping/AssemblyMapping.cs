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

		/// <summary>
		/// Initializes a new instance of the <see cref="AssemblyMapping"/> class.
		/// </summary>
		/// <param name="assemblyName">Name of the assembly.</param>
		public AssemblyMapping(string assemblyName)
		{
			this.assemblyName = assemblyName;
			typeMappingTable = new Dictionary<TypeReference, TypeMapping>();
			obfuscatedToOriginalMapping = new Dictionary<string, string>();
		}

		/// <summary>
		/// Gets the name of the assembly.
		/// </summary>
		/// <value>The name of the assembly.</value>
		public string AssemblyName
		{
			get { return assemblyName; }
		}

		/// <summary>
		/// Adds the type mapping to the assembly.
		/// </summary>
		/// <param name="type">The type.</param>
		/// <param name="obfuscatedTypeName">Name of the obfuscated type.</param>
		/// <returns></returns>
		public TypeMapping AddType(TypeReference type, string obfuscatedTypeName)
		{
			string typeFullName = type.Name;
			if (type.DeclaringType!=null){
				typeFullName=type.DeclaringType.Name+"."+typeFullName;
			}
			typeFullName = type.Namespace + "." + typeFullName;
			
			TypeMapping typeMapping = new TypeMapping(typeFullName, obfuscatedTypeName);
			typeMappingTable.Add(type, typeMapping);
			//Add a reverse mapping
			if (!String.IsNullOrEmpty(obfuscatedTypeName)){
				string obfuscatedFullName=obfuscatedTypeName;
				if (type.DeclaringType!=null){
					obfuscatedFullName=type.DeclaringType.Name+"."+obfuscatedFullName;
				}
				obfuscatedFullName=type.Namespace + "." + obfuscatedFullName;
				obfuscatedToOriginalMapping.Add(obfuscatedFullName, typeFullName);
			}
			return typeMapping;
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
			//Check the reverse mapping table
			/*if (obfuscatedToOriginalMapping.ContainsKey(typeName))
			{
				string originalTypeName = obfuscatedToOriginalMapping[typeName];
				if (typeMappingTable.ContainsKey(originalTypeName))
					return typeMappingTable[originalTypeName];
			}*/
			/*if (type.DeclaringType!=null)//if declaring type was renamed
			{
				string declaringTypeName=type.DeclaringType.Namespace+"."+type.DeclaringType.Name;
				if (obfuscatedToOriginalMapping.ContainsKey(declaringTypeName))
				{
					string originalDeclaringTypeName = obfuscatedToOriginalMapping[declaringTypeName];
					if (typeMappingTable.ContainsKey(originalDeclaringTypeName))
					{
						string obfuscatedTypeName= typeMappingTable[originalDeclaringTypeName].TypeName;
						//we should cut namespace
						if (obfuscatedTypeName.IndexOf(".")!=-1){
							int place=obfuscatedTypeName.IndexOf(".");
							obfuscatedTypeName=obfuscatedTypeName.Substring(place,obfuscatedTypeName.Length-place);
						}
						
						string newTypeName=obfuscatedTypeName +"."+ type.Name;
						if (typeMappingTable.ContainsKey(newTypeName))
							return typeMappingTable[newTypeName];
					}
				}
			}*/
			
		}
	}
}
