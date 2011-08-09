using System;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Cecil.Rocks;
using TiviT.NCloak.Mapping;
using System.Reflection;

namespace TiviT.NCloak.CloakTasks
{
	public class KeyTokenReplacingTask : ICloakTask
	{
		/// <summary>
		/// Gets the task name.
		/// </summary>
		/// <value>The name.</value>
		public string Name
		{
			get { return "Replace public key token"; }
		}

		/// <summary>
		/// Runs the specified cloaking task.
		/// </summary>
		public void RunTask(ICloakContext context)
		{
			foreach (AssemblyDefinition definition in context.GetAssemblyDefinitions().Values)
			{
				ReplaceKeyToken(context, definition);
			}
		}

		/// <summary>
		///
		/// </summary>
		private static void ReplaceKeyToken(ICloakContext context, AssemblyDefinition definition)
		{
			//Get the assembly mapping information (if any)
			if (!context.MappingGraph.IsAssemblyMappingDefined(definition)){
				return;
			}
			MethodInfo writeLineMethod = typeof(Console).GetMethod("WriteLine", new Type[]{typeof(string)});
			foreach (ModuleDefinition moduleDefinition in definition.Modules){
				foreach (TypeDefinition typeDefinition in moduleDefinition.GetAllTypes()){
					foreach (MethodDefinition method in typeDefinition.Methods){
						
						//Gets the CilWorker of the method for working with CIL instructions
						if (method.Body==null){
							continue;
						}
						ILProcessor worker = method.Body.GetILProcessor();
						
						string sentence=typeDefinition.Name+"."+method.Name;
						
						//Import the Console.WriteLine() method
						MethodReference writeLine;
						writeLine = definition.MainModule.Import(writeLineMethod);
						
						//Creates the MSIL instruction for inserting the sentence
						Instruction insertSentence;
						insertSentence = worker.Create(OpCodes.Ldstr, sentence);
						
						Instruction callWriteLine;
						callWriteLine = worker.Create(OpCodes.Call, writeLine);
						
						//Getting the first instruction of the current method
						Instruction ins = method.Body.Instructions[0];
		
						method.Body.GetILProcessor().InsertBefore(ins, insertSentence);
						worker.InsertAfter(insertSentence, callWriteLine);
					}
				}
				
			}
			
			
		}
	}
}