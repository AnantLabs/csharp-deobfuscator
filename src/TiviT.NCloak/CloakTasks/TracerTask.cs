using System;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Cecil.Rocks;
using TiviT.NCloak.Mapping;
using System.Reflection;

namespace TiviT.NCloak.CloakTasks
{
	public class TracerTask : ICloakTask
	{
		/// <summary>
		/// Gets the task name.
		/// </summary>
		/// <value>The name.</value>
		public string Name
		{
			get { return "Adding tracing capabilities"; }
		}

		/// <summary>
		/// Runs the specified cloaking task.
		/// </summary>
		public void RunTask(ICloakContext context)
		{
			foreach (AssemblyDefinition definition in context.GetAssemblyDefinitions().Values)
			{
				AddTracing(context, definition);
			}
		}

		/// <summary>
		/// Just adds Console.WriteLine(ClassName.MethodName) to each method. Run 'test.exe > test.out' to get result
		/// </summary>
		private static void AddTracing(ICloakContext context, AssemblyDefinition definition)
		{
			//Get the assembly mapping information (if any)
			if (!context.MappingGraph.IsAssemblyMappingDefined(definition)){
				return;
			}
			MethodInfo writeLineMethod = typeof(Console).GetMethod("WriteLine", new Type[]{typeof(string)});
			foreach (ModuleDefinition moduleDefinition in definition.Modules){
				foreach (TypeDefinition typeDefinition in moduleDefinition.GetAllTypes()){
					foreach (MethodDefinition method in typeDefinition.Methods){
						if (method.Body==null){
							continue;
						}
						ILProcessor worker = method.Body.GetILProcessor();
						string sentence=typeDefinition.Name+"."+method.Name;
						
						MethodReference writeLine=definition.MainModule.Import(writeLineMethod);
						
						Instruction insertSentence=worker.Create(OpCodes.Ldstr, sentence);
						Instruction callWriteLine=worker.Create(OpCodes.Call, writeLine);
						
						Instruction ins = method.Body.Instructions[0];
						worker.InsertBefore(ins, insertSentence);
						worker.InsertAfter(insertSentence, callWriteLine);
					}
				}
				
			}
			
			
		}
	}
}