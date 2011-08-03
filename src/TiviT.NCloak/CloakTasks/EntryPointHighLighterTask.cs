using System;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Cecil.Rocks;
using TiviT.NCloak.Mapping;

namespace TiviT.NCloak.CloakTasks
{
	public class EntryPointHighliterTask : ICloakTask
	{
		/// <summary>
		/// Gets the task name.
		/// </summary>
		/// <value>The name.</value>
		public string Name
		{
			get { return "Highlight entrypoint"; }
		}

		/// <summary>
		/// Runs the specified cloaking task.
		/// </summary>
		public void RunTask(ICloakContext context)
		{
			foreach (AssemblyDefinition definition in context.GetAssemblyDefinitions().Values)
			{
				HighLightEntryPoint(context, definition);
			}
		}

		/// <summary>
		///Renames method and class of entry point. Should be called after Mapping has been done
		/// </summary>
		private static void HighLightEntryPoint(ICloakContext context, AssemblyDefinition definition)
		{
			//Get the assembly mapping information (if any)
			if (!context.MappingGraph.IsAssemblyMappingDefined(definition)){
				return;
			}
			AssemblyMapping assemblyMapping = context.MappingGraph.GetAssemblyMapping(definition);
			
			ModuleDefinition module=definition.MainModule;
			MethodReference mainMethod=module.EntryPoint;
			TypeDefinition mainType=mainMethod.DeclaringType.GetTypeDefinition();
			
			TypeMapping typeMapping = assemblyMapping.GetTypeMapping(mainMethod.DeclaringType);
			assemblyMapping.ChangeTypeMapping(mainType,"MainClass");
			typeMapping.ChangeMethodMapping(mainMethod,"MainMethod");
		}
		
	}
}