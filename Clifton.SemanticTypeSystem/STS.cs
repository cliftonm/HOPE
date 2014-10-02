using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

using Clifton.Assertions;
using Clifton.ExtensionMethods;
using Clifton.SemanticTypeSystem.Interfaces;
using Clifton.Tools.Strings.Extensions;

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

	public class FullyQualifiedNativeType : IFullyQualifiedNativeType
	{
		public string Name { get { return FullyQualifiedName.RightOfRightmostOf('.'); } }
		public string FullyQualifiedName { get; set; }
		public string FullyQualifiedNameSansRoot { get { return FullyQualifiedName.RightOf('.'); } }
		public string Alias { get; set; }
		public INativeType NativeType { get; set; }
		public object Value { get; set; }				// Only used when getting FQNT's for an associated signal.
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

		public bool VerifyProtocolExists(string protocol)
		{
			return SemanticTypes.ContainsKey(protocol);
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
		/// Clone the element of the source signal into a new destination signal.  
		/// This does NOT clone the signal--it is designed to clone the specific child semantic element of the supplied signal.
		/// </summary>
		public dynamic Clone(dynamic sourceSignal, ISemanticElement childElem)
		{
			dynamic subsignal = Create(childElem.Name);										// Create the sub-signal
			PropertyInfo pi = sourceSignal.GetType().GetProperty(childElem.Name);			// Get the property of the source's sub-type, which will/must be a semantic element
			object val = pi.GetValue(sourceSignal);										// Get the instance of the semantic element we are cloning.

			ISemanticTypeStruct subSemStruct = GetSemanticTypeStruct(childElem.Name);

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

		// TODO: Ordinality (which still needs to be implemented as a property of NT's and SE's) needs to be preserved.
		/// <summary>
		/// Returns a list of fully qualified (full path) type names for the final implementing native types.
		/// </summary>
		public List<IFullyQualifiedNativeType> GetFullyQualifiedNativeTypes(string protocolName)
		{
			List<IFullyQualifiedNativeType> ret = new List<IFullyQualifiedNativeType>();
			string stack = protocolName;
			ISemanticTypeStruct st = GetSemanticTypeStruct(protocolName);
			string alias = st.Alias;
			RecurseGetFullyQualifiedNativeTypes(st, stack, alias, ret);

			return ret;
		}

		// TODO: Ordinality (which still needs to be implemented as a property of NT's and SE's) needs to be preserved.
		/// <summary>
		/// Returns a list of fully qualified (full path) type names for the final implementing native types and their values, given the provided source signal.
		/// </summary>
		public List<IFullyQualifiedNativeType> GetFullyQualifiedNativeTypeValues(dynamic signal, string protocolName)
		{
			List<IFullyQualifiedNativeType> ret = new List<IFullyQualifiedNativeType>();
			string stack = protocolName;
			ISemanticTypeStruct st = GetSemanticTypeStruct(protocolName);
			string alias = st.Alias;
			RecurseGetFullyQualifiedNativeTypeValues(signal, st, stack, alias, ret);

			return ret;
		}

		/// <summary>
		/// Recursively drills into a signal's structure and sets the target native type instance to the specified value.
		/// IMPORTANT! The fqn must NOT include the root ST, as the signal itself is the root ST.
		/// </summary>
		public void SetFullyQualifiedNativeTypeValue(dynamic signal, string fqn, object val)
		{
			// ST's will still have something left to process.
			if (fqn.Contains('.'))
			{
				string protocolName = fqn.LeftOf('.');
				ISemanticTypeStruct st = GetSemanticTypeStruct(protocolName);
				string remainder = fqn.RightOf('.');
				PropertyInfo pi = signal.GetType().GetProperty(protocolName);
				object childSignal = pi.GetValue(signal);
				SetFullyQualifiedNativeTypeValue(childSignal, remainder, val);
			}
			else
			{
				PropertyInfo pi = signal.GetType().GetProperty(fqn);
				pi.SetValue(signal, val);
			}
		}

		protected void RecurseGetFullyQualifiedNativeTypes(ISemanticTypeStruct st, string stack, string alias, List<IFullyQualifiedNativeType> fqntList)
		{
			foreach (INativeType nativeType in st.NativeTypes)
			{
				alias = (!String.IsNullOrEmpty(alias) ? alias : nativeType.Alias);
				fqntList.Add(new FullyQualifiedNativeType() { FullyQualifiedName = stack + "." + nativeType.Name, NativeType = nativeType, Alias = alias });
			}

			foreach (ISemanticElement childElem in st.SemanticElements)
			{
				stack = stack + "." + childElem.Name;			// push
				ISemanticTypeStruct stChild = GetSemanticTypeStruct(childElem.Name);
				string newAlias = (!String.IsNullOrEmpty(alias) ? alias : stChild.Alias);
				RecurseGetFullyQualifiedNativeTypes(stChild, stack, newAlias, fqntList);
				stack = stack.LeftOfRightmostOf('.');			// pop
			}
		}

		protected void RecurseGetFullyQualifiedNativeTypeValues(object signal, ISemanticTypeStruct st, string stack, string alias, List<IFullyQualifiedNativeType> fqntList)
		{
			foreach (INativeType nativeType in st.NativeTypes)
			{
				// Acquire value through reflection.
				PropertyInfo pi = signal.GetType().GetProperty(nativeType.Name);
				object val = pi.GetValue(signal);
				alias = (!String.IsNullOrEmpty(alias) ? alias : nativeType.Alias);

				fqntList.Add(new FullyQualifiedNativeType() 
				{ 
					FullyQualifiedName = stack + "." + nativeType.Name, 
					Alias = alias,
					NativeType = nativeType,
					Value = val
				});
			}

			foreach (ISemanticElement childElem in st.SemanticElements)
			{
				// Acquire child SE through reflection:
				PropertyInfo piSub = signal.GetType().GetProperty(childElem.Name);
				object childSignal = piSub.GetValue(signal);

				stack = stack + "." + childElem.Name;			// push
				ISemanticTypeStruct stChild = GetSemanticTypeStruct(childElem.Name);
				string newAlias = (!String.IsNullOrEmpty(alias) ? alias : stChild.Alias);
				RecurseGetFullyQualifiedNativeTypeValues(childSignal, stChild, stack, newAlias, fqntList);
				stack = stack.LeftOfRightmostOf('.');			// pop
			}
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
		/// Merge new semantic types with existing ones.
		/// </summary>
		public void Parse(List<SemanticTypeDecl> decls, List<SemanticTypeStruct> structs)
		{
			Dictionary<string, ISemanticType> stDict = Parser.BuildSemanticTypes(decls, structs);
			SemanticTypes = SemanticTypes.Merge(stDict);
		}

		public string GenerateCode()
		{
			return CodeGenerator.Generate(SemanticTypes);
		}
	}
}

