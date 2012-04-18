using System;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Cecil.Rocks;
//using System.Reflection;

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
			MethodDefinition keyTokenMethod= createTokenMethod(definition);
			foreach (ModuleDefinition moduleDefinition in definition.Modules){
				foreach (TypeDefinition typeDefinition in moduleDefinition.GetAllTypes()){
					foreach (MethodDefinition method in typeDefinition.Methods){
						if (method.Body==null){
							continue;
						}
						inMethodReplacer(method,keyTokenMethod);
					}
				}
				
			}
			
			
		}
		
		private static void inMethodReplacer(MethodDefinition method,MethodDefinition keyTokenMethod)
		{
			for (int i=0;i<method.Body.Instructions.Count;i++)
			{
				Instruction instruction=method.Body.Instructions[i];
				if (instruction.OpCode.Name!="callvirt"){
					continue;
				}
				if (instruction.Operand is MethodReference)
				{
					MethodReference methodReference = (MethodReference)instruction.Operand;
					if (methodReference.Name.IndexOf("GetPublicKeyToken")!=-1){
						//if found such, we should delete this instruction, 2 instructions before
						
						method.Body.Instructions.Remove(instruction.Previous);
						method.Body.Instructions.Remove(instruction.Previous);
						i-=2;
						
						var il = method.Body.GetILProcessor();
						il.InsertBefore(instruction, il.Create(OpCodes.Call, keyTokenMethod));
						
						
						Instruction insertInst=instruction.Next;
						System.Reflection.MethodInfo writeLineMethod = typeof(Console).GetMethod("WriteLine", new Type[]{typeof(string)});
						MethodReference writeLine;
						writeLine = method.Module.Import(writeLineMethod);
						Instruction insertSentence = il.Create(OpCodes.Ldstr, "Fuck");
						Instruction callWriteLine=il.Create(OpCodes.Call, writeLine);
						il.InsertAfter(insertInst, insertSentence);
						il.InsertAfter(insertSentence, callWriteLine);
						
						method.Body.Instructions.Remove(instruction);
					}
				}
			}
		}
		
		private static MethodDefinition createTokenMethod(AssemblyDefinition assembly)
		{
			byte[] token=assembly.Name.PublicKeyToken;
			TypeDefinition storeType = new TypeDefinition("Deobfuscator","PublicKeyStore",
			                                              TypeAttributes.Public | TypeAttributes.Class |
			                                              TypeAttributes.AutoClass | TypeAttributes.AnsiClass |
			                                              TypeAttributes.BeforeFieldInit,
			                                              assembly.Import(typeof(object)));
			
			assembly.MainModule.Types.Add(storeType);
			MethodDefinition method = new MethodDefinition("getPublicKeyToken",
			                                               MethodAttributes.Public | MethodAttributes.Static,
			                                               assembly.Import(typeof(byte[])));
			storeType.Methods.Add(method);
			
			method.Body.InitLocals = true;
			method.Body.MaxStackSize = 3;
			method.AddLocal(assembly, typeof (byte[]));
			method.AddLocal(assembly, typeof (byte[]));
			
			var il = method.Body.GetILProcessor();
			il.Append(OpCodes.Nop);
			sbyte arrayLength=(sbyte)token.Length;
			
			il.Append(il.Create(OpCodes.Ldc_I4_S, arrayLength));//size of array
			il.Append(il.Create(OpCodes.Newarr,  assembly.Import(typeof(byte[]))));
			il.Append(OpCodes.Stloc_0);
			
			for(sbyte i=0;i<token.Length;i++)
			{
				il.Append(OpCodes.Ldloc_0);//load array
				il.Append(il.Create(OpCodes.Ldc_I4_S, i));//store index, where to place
				il.Append(il.Create(OpCodes.Ldc_I4, (int)token[i]));//store actual value
				il.Append(OpCodes.Stelem_I1);//set value to array
			}
			
			il.Append(OpCodes.Ldloc_0);//return manipulations
			il.Append(OpCodes.Stloc_1);
			
			Instruction lastLdLoc = il.Create(OpCodes.Ldloc_1);
			il.Append(il.Create(OpCodes.Br_S, lastLdLoc));
			il.Append(lastLdLoc);
			il.Append(OpCodes.Ret);
			return method;
		}
	}
}