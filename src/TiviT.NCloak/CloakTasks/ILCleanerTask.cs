using System;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Cecil.Rocks;
using TiviT.NCloak.Mapping;
using System.Reflection;

namespace TiviT.NCloak.CloakTasks
{
	public class ILCleanerTask : ICloakTask
	{
		/// <summary>
		/// Gets the task name.
		/// </summary>
		/// <value>The name.</value>
		public string Name
		{
			get { return "Cleaning IL Code"; }
		}

		/// <summary>
		/// Runs the specified cloaking task.
		/// </summary>
		public void RunTask(ICloakContext context)
		{
			foreach (AssemblyDefinition definition in context.GetAssemblyDefinitions().Values)
			{
				StartCleaning(context, definition);
			}
		}

		/// <summary>
		/// Removes protector generated unneeded code. Like 'dup' and then 'pop'
		/// </summary>
		private static void StartCleaning(ICloakContext context, AssemblyDefinition definition)
		{
			foreach (ModuleDefinition moduleDefinition in definition.Modules){
				foreach (TypeDefinition typeDefinition in moduleDefinition.GetAllTypes()){
					foreach (MethodDefinition method in typeDefinition.Methods){
						if (method.Body==null){
							continue;
						}
						inMethodReplacer(method);
					}
				}
			}
		}
		
		private static void inMethodReplacer(MethodDefinition method)
		{
			for (int i=0;i<method.Body.Instructions.Count;i++)
			{
				Instruction instruction=method.Body.Instructions[i];
				if (instruction.OpCode.Name=="dup" && instruction.Next.OpCode.Name=="pop"){
					//dup places value to stack, pop - removes. This commands are not needed
					method.Body.Instructions.Remove(instruction.Next);
					method.Body.Instructions.Remove(instruction);
					i--;
				}
			}
			
		}
		
		
	}
}