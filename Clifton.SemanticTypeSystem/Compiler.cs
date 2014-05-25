using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

using System.CodeDom.Compiler;
using Microsoft.CSharp;

using Clifton.Assertions;
using Clifton.Receptor.Interfaces;
using Clifton.SemanticTypeSystem.Interfaces;

namespace Clifton.SemanticTypeSystem
{
	public static class Compiler
	{
		public static Assembly Compile(string code)
		{
			CodeDomProvider provider = null;
            provider = CodeDomProvider.CreateProvider("CSharp");
            CompilerParameters cp = new CompilerParameters();

            // Generate a class library in memory.
            cp.GenerateExecutable = false;
			cp.GenerateInMemory = true;
            cp.TreatWarningsAsErrors = false;
			cp.ReferencedAssemblies.Add("System.dll");
			cp.ReferencedAssemblies.Add("System.Drawing.dll");
			cp.ReferencedAssemblies.Add("Microsoft.CSharp.dll");
			cp.ReferencedAssemblies.Add(typeof(System.Runtime.CompilerServices.DynamicAttribute).Assembly.Location);	// for "dynamic" keyword.
			cp.ReferencedAssemblies.Add(typeof(STS).Assembly.Location);
			cp.ReferencedAssemblies.Add(typeof(ISemanticType).Assembly.Location);
			cp.ReferencedAssemblies.Add(typeof(IReceptorInstance).Assembly.Location);

            // Invoke compilation of the source file.
            CompilerResults cr = provider.CompileAssemblyFromSource(cp, code);

            if (cr.Errors.Count > 0)
            {
				Assert.ErrorMessage = cr.Errors[0].ToString();
				throw new ApplicationException("Compilation error.");
            }

			return cr.CompiledAssembly;
        }
	}
}	

