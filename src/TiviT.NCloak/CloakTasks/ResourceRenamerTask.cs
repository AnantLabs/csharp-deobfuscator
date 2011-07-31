using System;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Cecil.Rocks;
using TiviT.NCloak.Mapping;

namespace TiviT.NCloak.CloakTasks
{
	public class ResourceRenamerTask : ICloakTask
	{
		/// <summary>
		/// Gets the task name.
		/// </summary>
		/// <value>The name.</value>
		public string Name
		{
			get { return "Rename from resources"; }
		}

		/// <summary>
		/// Runs the specified cloaking task.
		/// </summary>
		public void RunTask(ICloakContext context)
		{
			foreach (AssemblyDefinition definition in context.GetAssemblyDefinitions().Values)
			{
				RenameResources(context, definition);
			}
		}

		/// <summary>
		/// If assembly has forms, then it has resources, which have same names as class of the form.
		/// If form class name is changed, resource name should be changed too
		/// </summary>
		private static void RenameResources(ICloakContext context, AssemblyDefinition definition)
		{
			//Get the assembly mapping information (if any)
			if (!context.MappingGraph.IsAssemblyMappingDefined(definition)){
				return;
			}
			AssemblyMapping assemblyMapping = context.MappingGraph.GetAssemblyMapping(definition);
			
			foreach (ModuleDefinition moduleDefinition in definition.Modules){
				foreach (TypeDefinition typeDefinition in moduleDefinition.GetAllTypes()){
					TypeMapping typeMapping = assemblyMapping.GetTypeMapping(typeDefinition);
					if (typeMapping == null){
						continue;
					}
					if (String.IsNullOrEmpty(typeMapping.ObfuscatedTypeName)){
						continue;
					}
					if (typeDefinition.BaseType==null){
						continue;
					}
					if (typeDefinition.BaseType.Name=="Form")//our case{
						renameResource(moduleDefinition,typeDefinition.Name,typeMapping.ObfuscatedTypeName);
				}
			}
		}
		
		
		private static void renameResource(ModuleDefinition moduleDefinition,string oldName,string newName)
		{
			string resOldName=oldName+".resources";
			for(int i=0;i<moduleDefinition.Resources.Count;i++)
			{
				if (moduleDefinition.Resources[i].Name==resOldName)
				{
					moduleDefinition.Resources[i].Name=newName+".resources";
					return;
				}
			}
		}
		
		
		
	}
}