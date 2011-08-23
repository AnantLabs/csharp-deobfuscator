using Mono.Cecil;
using Mono.Cecil.Rocks;
using TiviT.NCloak.Mapping;

namespace TiviT.NCloak.CloakTasks
{
	public class MappingTask : ICloakTask
	{
		/// <summary>
		/// Gets the task name.
		/// </summary>
		/// <value>The name.</value>
		public string Name
		{
			get { return "Creating call map"; }
		}

		/// <summary>
		/// Runs the specified cloaking task.
		/// </summary>
		/// <param name="context">The running context of this cloak job.</param>
		public void RunTask(ICloakContext context)
		{
			//Get out if rename is turned off
			if (context.Settings.NoRename)
				return;
			//Go through the members and build up a mapping graph
			//If this is done then the members in the graph will be obfuscated, otherwise we'll
			//just obfuscate private members

			//Loop through each assembly and process it
			foreach (AssemblyDefinition definition in context.GetAssemblyDefinitions().Values)
			{
				ProcessAssembly(context, definition);
			}
		}

		
		private static void ProcessAssembly(ICloakContext context, AssemblyDefinition definition)
		{
			bool obfuscateAll = context.Settings.ObfuscateAllModifiers;
			
			AssemblyMapping assemblyMapping = context.MappingGraph.AddAssembly(definition);

			foreach (ModuleDefinition moduleDefinition in definition.Modules)
			{
				foreach (TypeDefinition typeDefinition in moduleDefinition.GetAllTypes())
				{
					TypeMapping typeMapping = assemblyMapping.GetTypeMapping(typeDefinition);
					if (typeMapping == null){
						typeMapping = assemblyMapping.AddType(typeDefinition);
					}
					foreach (MethodDefinition methodDefinition in typeDefinition.Methods)
					{
						if (typeMapping.HasMethodBeenObfuscated(methodDefinition.Name)){
							continue;
						}
						//We won't do constructors - causes issues
						if (methodDefinition.IsConstructor){
							continue;
						}
					
						//Take into account whether this is overriden, or an interface implementation
						if (methodDefinition.IsVirtual)
						{
							//We handle this differently - rather than creating a new name each time we need to reuse any already generated names
							//We do this by firstly finding the root interface or object
							TypeDefinition baseType = FindBaseTypeDeclaration(typeDefinition, methodDefinition);
							if (baseType != null )
							{
								//Find it in the mappings
								TypeMapping baseTypeMapping = assemblyMapping.GetTypeMapping(baseType);
								if (baseTypeMapping != null)
								{
									//We found the type mapping - look up the name it uses for this method and use that
									if (baseTypeMapping.HasMethodMappingSig(methodDefinition)){
										string obfuscatedName=baseTypeMapping.GetObfuscatedMethodNameSig(methodDefinition);
										typeMapping.AddMethodMapping(methodDefinition,obfuscatedName);
									}
									else
									{
										//That's strange... we shouldn't get into here - but if we ever do then
										//we'll add the type mapping into both
										string obfuscatedName =typeMapping.AddMethodMapping(methodDefinition);
										baseTypeMapping.AddMethodMappingSig(methodDefinition, obfuscatedName,baseType);
									}
								}
								else
								{
									//Otherwise add it into our list manually
									//at the base level first off
									baseTypeMapping = assemblyMapping.AddType(baseType);
									string obfuscatedName =typeMapping.AddMethodMapping(methodDefinition);
									baseTypeMapping.AddMethodMappingSig(methodDefinition,obfuscatedName, baseType);
								}
							}
							else{
								typeMapping.AddMethodMapping(methodDefinition);
							}
						}
						else //Add normally
							typeMapping.AddMethodMapping(methodDefinition);
					}

					//Properties
					foreach (PropertyDefinition propertyDefinition in typeDefinition.Properties)
					{
						//First of all - check if we've obfuscated it already - if we have then don't bother
						if (typeMapping.HasPropertyBeenObfuscated(propertyDefinition.Name)){
							continue;
						}
						
						if (obfuscateAll)
						{
							if ((propertyDefinition.GetMethod != null && propertyDefinition.GetMethod.IsVirtual) ||
							    (propertyDefinition.SetMethod != null && propertyDefinition.SetMethod.IsVirtual))
							{
								//We handle this differently - rather than creating a new name each time we need to reuse any already generated names
								//We do this by firstly finding the root interface or object
								TypeDefinition baseType = FindBaseTypeDeclaration(typeDefinition, propertyDefinition);
								if (baseType != null)
								{
									//Find it in the mappings
									TypeMapping baseTypeMapping = assemblyMapping.GetTypeMapping(baseType);
									if (baseTypeMapping != null)
									{
										//We found the type mapping - look up the name it uses for this property and use that
										if (baseTypeMapping.HasPropertyMappingSig(propertyDefinition))
											typeMapping.AddPropertyMapping(propertyDefinition, baseTypeMapping.GetObfuscatedPropertyNameSig(propertyDefinition));
										else
										{
											//That's strange... we shouldn't get into here - but if we ever do then
											//we'll add the type mapping into both
											string obfuscatedName = typeMapping.AddPropertyMapping(propertyDefinition);
											baseTypeMapping.AddPropertyMappingSig(propertyDefinition, obfuscatedName,baseType);
										}
									}
									else
									{
										//Otherwise add it into our list manually
										//at the base level first off
										baseTypeMapping = assemblyMapping.AddType(baseType);
										string obfuscatedName = typeMapping.AddPropertyMapping(propertyDefinition);
										baseTypeMapping.AddPropertyMappingSig(propertyDefinition, obfuscatedName,baseType);
									}
								}
								else{
									typeMapping.AddPropertyMapping(propertyDefinition);
								}
							}
							else{
								typeMapping.AddPropertyMapping(propertyDefinition);
							}
						}
						else if (propertyDefinition.GetMethod != null && propertyDefinition.SetMethod != null)
						{
							//Both parts need to be private
							if (propertyDefinition.GetMethod.IsPrivate && propertyDefinition.SetMethod.IsPrivate)
								typeMapping.AddPropertyMapping(propertyDefinition);
						}
						else if (propertyDefinition.GetMethod != null)
						{
							//Only the get is present - make sure it is private
							if (propertyDefinition.GetMethod.IsPrivate)
								typeMapping.AddPropertyMapping(propertyDefinition);
						}
						else if (propertyDefinition.SetMethod != null)
						{
							//Only the set is present - make sure it is private
							if (propertyDefinition.SetMethod.IsPrivate)
								typeMapping.AddPropertyMapping(propertyDefinition);
						}
					}

					//Fields
					foreach (FieldDefinition fieldDefinition in typeDefinition.Fields)
					{
						//First of all - check if we've obfuscated it already - if we have then don't bother
						if (typeMapping.HasFieldBeenObfuscated(fieldDefinition.Name)){
							continue;
						}
						typeMapping.AddFieldMapping(fieldDefinition);
					}
				}
			}
		}

		/// <summary>
		/// Recursively finds the base type declaration for the given method name.
		/// </summary>
		/// <param name="definition">The definition.</param>
		/// <param name="method">The method definition/reference.</param>
		/// <returns></returns>
		private static TypeDefinition FindBaseTypeDeclaration(TypeDefinition definition, MethodReference method)
		{
			//Search the interfaces first
			foreach (TypeReference tr in definition.Interfaces)
			{
				//Convert to a type definition
				if (tr.FullName=="System.IDisposable"){
					continue;
				}
				TypeDefinition td = tr.GetTypeDefinition();
				if (td==null){
					continue;
				}
				MethodDefinition md = td.Methods.FindMethod(method.Name, method.Parameters);
				if (md != null)
					return td;

				//Do a recursive search below
				TypeDefinition baseInterface = FindBaseTypeDeclaration(td, method);
				if (baseInterface != null)
					return baseInterface;
			}

			//Search the base class
			TypeReference baseTr = definition.BaseType;
			if (baseTr != null)
			{
				TypeDefinition baseTd = baseTr.GetTypeDefinition();
				if (baseTd != null)
				{
					MethodDefinition md = baseTd.Methods.FindMethod(method.Name, method.Parameters);
					if (md != null)
						return baseTd;

					//Do a recursive search below
					TypeDefinition baseClass = FindBaseTypeDeclaration(baseTd, method);
					if (baseClass != null)
						return baseClass;
				}
			}

			//We've exhausted all options
			return null;
		}

		/// <summary>
		/// Recursively finds the base type declaration for the given method name.
		/// </summary>
		/// <param name="definition">The definition.</param>
		/// <param name="property">The property definition/reference.</param>
		/// <returns></returns>
		private static TypeDefinition FindBaseTypeDeclaration(TypeDefinition definition, MemberReference property)
		{
			//Search the interfaces first
			foreach (TypeReference tr in definition.Interfaces)
			{
				//Convert to a type definition
				TypeDefinition td = tr.GetTypeDefinition();
				if (td==null)
					continue;
				if (td.Properties.HasProperty(property.Name))
					return td;

				//Do a recursive search below
				TypeDefinition baseInterface = FindBaseTypeDeclaration(td, property);
				if (baseInterface != null)
					return baseInterface;
			}

			//Search the base class
			TypeReference baseTr = definition.BaseType;
			if (baseTr != null)
			{
				TypeDefinition baseTd = baseTr.GetTypeDefinition();
				if (baseTd != null)
				{
					if (baseTd.Properties.HasProperty(property.Name))
						return baseTd;

					//Do a recursive search below
					TypeDefinition baseClass = FindBaseTypeDeclaration(baseTd, property);
					if (baseClass != null)
						return baseClass;
				}
			}

			//We've exhausted all options
			return null;
		}
	}
}
