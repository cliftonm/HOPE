using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

using Clifton.Assertions;
using Clifton.ExtensionMethods;
using Clifton.SemanticTypeSystem.Interfaces;

namespace Clifton.SemanticTypeSystem
{
	public class NewSemanticTypeEventArgs : EventArgs
	{
		public IRuntimeSemanticType Type { get; protected set; }

		public NewSemanticTypeEventArgs(IRuntimeSemanticType newType)
		{
			Type = newType;
		}
	}

	public class STS : ISemanticTypeSystem
	{
		public event EventHandler<NewSemanticTypeEventArgs> NewSemanticType;
		public event EventHandler<EventArgs> CreationDone;

		public Dictionary<string, ISemanticType> SemanticTypes { get; protected set; }
		public Assembly CompiledAssembly { get; set; }
		public Dictionary<Guid, SemanticTypeInstance> Instances { get; protected set; }
		public List<SemanticTypeInstance> SymbolTable { get { return Instances.Values.ToList(); } }

		public STS()
		{
			SemanticTypes = new Dictionary<string, ISemanticType>();
			Instances = new Dictionary<Guid, SemanticTypeInstance>();
		}

		/// <summary>
		/// Clears all collections and assemblies.
		/// </summary>
		public void Reset()
		{
			SemanticTypes.Clear();
			Instances.Clear();
			SymbolTable.Clear();
			CompiledAssembly = null;
		}

		/// <summary>
		/// Returns the name as the semantic type given the instance.
		/// This is not very efficient as it is walking through the symbol table.
		/// </summary>
		public string GetSemanticTypeName(ISemanticType instance)
		{
			return SymbolTable.Single(s => s.Instance == instance).Name;
		}

		public ISemanticTypeStruct GetSemanticTypeStruct(string typeName)
		{
			Assert.That(SemanticTypes.ContainsKey(typeName), "The semantic type " + typeName + " has not been declared.");

			return SemanticTypes[typeName].Struct;
		}

		/// <summary>
		/// Create an instance of the specified type, adding it to the Instances collection.
		/// </summary>
		public IRuntimeSemanticType Create(string typeName, IRuntimeSemanticType parent = null)
		{
			Assert.That(SemanticTypes.ContainsKey(typeName), "The semantic type "+typeName+" has not been declared.");
			IRuntimeSemanticType t = (IRuntimeSemanticType)CompiledAssembly.CreateInstance("SemanticTypes." + typeName);
			t.Initialize(this);
			Guid guid = Guid.NewGuid();			// We create a unique key for this instance.
			Instances[guid] = (new SemanticTypeInstance() { Name = typeName, Instance = t, Parent = parent, Key = guid, Definition = SemanticTypes[typeName] });

			NewSemanticType.Fire(this, new NewSemanticTypeEventArgs(t));

			return t;
		}

		/// <summary>
		/// Clone the element of the source signal into the destination signal.
		/// </summary>
		/// <param name="destSignal"></param>
		/// <param name="sourceSignal"></param>
		/// <param name="elem"></param>
		public dynamic Clone(dynamic sourceSignal, ISemanticElement elem)
		{
			dynamic subsignal = Create(elem.Name);										// Create the sub-signal
			PropertyInfo pi = sourceSignal.GetType().GetProperty(elem.Name);			// Get the property of the source's sub-type, which will/must be a semantic element
			object val = pi.GetValue(sourceSignal);										// Get the instance of the semantic element we are cloning.

			ISemanticTypeStruct subSemStruct = GetSemanticTypeStruct(elem.Name);

			foreach (INativeType nativeType in subSemStruct.NativeTypes)
			{
				// Copy any native types.
				object ntVal = nativeType.GetValue(this, val);
				nativeType.SetValue(this, subsignal, ntVal);
			}

			// Recurse drilling into semantic types of this type and copying any potential native types.
			foreach (ISemanticElement semanticElem in subSemStruct.SemanticElements)
			{
				dynamic se = Clone((dynamic)val, semanticElem);								// Clone the sub-semantic type
				PropertyInfo piSub = subsignal.GetType().GetProperty(semanticElem.Name);	// Get the PropertyInfo for this type.
				piSub.SetValue(subsignal, se);												// Assign the instance of the created semantic type to the value for this property.
			}

			return subsignal;
		}

		public void FireCreationDone()
		{
			CreationDone.Fire(this, EventArgs.Empty);
		}

		/// <summary>
		/// Given a ST instance, remove it from the instances collection, including all children.
		/// </summary>
		/// <param name="instance"></param>
		public void Destroy(IRuntimeSemanticType instance)
		{
			GetChildInstances(instance).ForEach(child => Destroy(child));
			Guid key = Instances.Single(kvp => kvp.Value.Instance == instance).Key;
			Instances.Remove(key);
		}

		/// <summary>
		/// Return all the child ST instances of the specified ST instance.
		/// </summary>
		protected List<IRuntimeSemanticType> GetChildInstances(IRuntimeSemanticType instance)
		{
			List<IRuntimeSemanticType> children = Instances.Where(kvp => kvp.Value.Parent == instance).Select(i => i.Value.Instance).ToList();

			return children;
		}

		/// <summary>
		/// Parse an XML string, updating the ST symbol table.
		/// Duplicate definitions will result in an exception (yes, we want this behavior.)
		/// </summary>
		public void Parse(List<SemanticTypeDecl> decls, List<SemanticTypeStruct> structs)
		{
//			List<SemanticTypeDecl> decls = Parser.ParseDeclarations(xml);
			//List<SemanticTypeStruct> structs = Parser.ParseStructs(xml);
			Dictionary<string, ISemanticType> stDict = Parser.BuildSemanticTypes(decls, structs);
			SemanticTypes = SemanticTypes.Merge(stDict);
		}

		public string GenerateCode()
		{
			return CodeGenerator.Generate(SemanticTypes);
		}
	}
}

