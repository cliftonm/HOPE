/*
    Copyright 2104 Higher Order Programming

    This program is free software; you can redistribute it and/or
    modify it under the terms of the GNU General Public License
    as published by the Free Software Foundation; either version 2
    of the License, or (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with this program; if not, write to the Free Software
    Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02110-1301, USA.

*/

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

