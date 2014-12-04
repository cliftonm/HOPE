using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

using Clifton.Assertions;
using Clifton.ExtensionMethods;
using Clifton.SemanticTypeSystem.Interfaces;
using Clifton.Tools.Data;
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
		public bool UniqueField { get; set; }
		public int Ordinality { get; set; }
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
			dynamic subsignal = null;

			if (sourceSignal != null)
			{
				PropertyInfo pi = sourceSignal.GetType().GetProperty(childElem.Name);			// Get the property of the source's sub-type, which will/must be a semantic element
				object val = pi.GetValue(sourceSignal);										// Get the instance of the semantic element we are cloning.

				// A sub-ST can be null, especially as produced by the persistence engine with outer joins.
				if (val != null)
				{
					subsignal = Create(childElem.Name);										// Create the sub-signal
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
				}
			}

			return subsignal;
		}

		/// <summary>
		/// Recurse into the named structure, returning itself and all sub-structures.
		/// The return is a list of tuples, where Item1 is the ST and Item2 is the parent ST of Item1
		/// </summary>
		public List<Tuple<ISemanticTypeStruct, ISemanticTypeStruct>> GetAllSemanticTypes(string protocolName)
		{
			List<Tuple<ISemanticTypeStruct, ISemanticTypeStruct>> ret = new List<Tuple<ISemanticTypeStruct, ISemanticTypeStruct>>();
			ISemanticTypeStruct sts = GetSemanticTypeStruct(protocolName);
			GetAllSemanticTypes(sts, null, ret);

			return ret;
		}

		/// <summary>
		/// Returns true if the signal has the specified semantic type and its value is not null.
		/// </summary>
		public bool TryGetSignalValue(dynamic signal, string semanticTypeName, out object val)
		{
			val = null;
			bool ret = false;

			PropertyInfo pi = signal.GetType().GetProperty(semanticTypeName);

			if (pi != null)
			{
				val = pi.GetValue(signal);

				if (val != null)
				{
					ret = true;
				}
			}

			return ret;
		}

		// TODO: Fix references to GetSemanticTypeStruct(child.Name);
		// because these two are identical:
		// ISemanticTypeStruct s1 = child.Element.Struct;
		// ISemanticTypeStruct s2 = GetSemanticTypeStruct(child.Name);

		/// <summary>
		/// Recursively add ST children to the structure list.
		/// </summary>
		protected void GetAllSemanticTypes(ISemanticTypeStruct sts, ISemanticTypeStruct parent, List<Tuple<ISemanticTypeStruct, ISemanticTypeStruct>> structList)
		{
			structList.Add(new Tuple<ISemanticTypeStruct, ISemanticTypeStruct>(sts, parent));
			sts.SemanticElements.ForEach(child => GetAllSemanticTypes(child.Element.Struct, sts, structList));
		}

		/// <summary>
		/// Returns a list of fully qualified (full path) type names for the final implementing native types.
		/// This function recurses into all child ST to derive all NT's in the ST tree.
		/// Set the recurse flag to false to prevent this behavior, thus the method will return only NT's for the current protocol.
		/// </summary>
		public List<IFullyQualifiedNativeType> GetFullyQualifiedNativeTypes(string protocolName, bool recurse = true)
		{
			List<IFullyQualifiedNativeType> ret = new List<IFullyQualifiedNativeType>();
			string stack = protocolName;
			ISemanticTypeStruct st = GetSemanticTypeStruct(protocolName);
			string alias = st.Alias;
			RecurseGetFullyQualifiedNativeTypes(st, stack, alias, st.Unique, 0, recurse, ret);

			return ret;
		}

		/// <summary>
		/// Returns a list of fully qualified (full path) type names for the final implementing native types and their values, given the provided source signal.
		/// This function recurses into all child ST to derive all NT's in the ST tree.
		/// Set the recurse flag to false to prevent this behavior, thus the method will return only NT's for the current protocol.
		/// </summary>
		public List<IFullyQualifiedNativeType> GetFullyQualifiedNativeTypeValues(dynamic signal, string protocolName, bool recurse = true)
		{
			List<IFullyQualifiedNativeType> ret = new List<IFullyQualifiedNativeType>();
			string stack = protocolName;
			ISemanticTypeStruct st = GetSemanticTypeStruct(protocolName);
			string alias = st.Alias;
			RecurseGetFullyQualifiedNativeTypeValues(signal, st, stack, alias, st.Unique, 0, recurse, ret);

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
				// Convert to the target type.  For example, in the signal creator, we need to convert a LoggerMessage's date/time from a string to DateTime.
				object convertedVal = Converter.Convert(val, pi.PropertyType);
				pi.SetValue(signal, convertedVal);
			}
		}

		/// <summary>
		/// Creates a custom type of the given name and known child protocols.
		/// </summary>
		public void CreateCustomType(string name, List<string> childProtocols)
		{
			List<SemanticTypeDecl> decls = new List<SemanticTypeDecl>();
			List<SemanticTypeStruct> structs = new List<SemanticTypeStruct>();

			SemanticTypeDecl decl;
			SemanticTypeStruct sts;

			// Noun Decl.
			decl = new SemanticTypeDecl() { OfTypeName = "Noun" };
			decl.AttributeValues.Add(new AttributeValue() { Name = "Name", Value = "Noun" });
			decls.Add(decl);

			// Noun Struct.
			sts = new SemanticTypeStruct() { DeclTypeName = "Noun" };
			sts.NativeTypes.Add(new Clifton.SemanticTypeSystem.NativeType() { Name = "Name", ImplementingType = "string" });
			structs.Add(sts);

			// Custom type decl and struct.
			decl = new SemanticTypeDecl() { OfTypeName = "Noun" };
			decl.AttributeValues.Add(new AttributeValue() { Name = "Name", Value = name });
			sts = new SemanticTypeStruct() { DeclTypeName = name };
			decls.Add(decl);
			structs.Add(sts);

			// Elements of the custom type.
			foreach (string childProtocol in childProtocols)
			{
				sts.SemanticElements.Add(new Clifton.SemanticTypeSystem.SemanticElement() { Name = childProtocol});
				// Recursively copy decls and structs of the existing types into our decls and structs lists.
				SemanticTypeStruct existingSts = (SemanticTypeStruct)GetSemanticTypeStruct(childProtocol);
				CopyDeclStruct(decls, structs, existingSts);
			}

			// Compile it.
			Parse(decls, structs);
			string code = GenerateCode();
			System.Reflection.Assembly assy = Compiler.Compile(code);
			CompiledAssembly = assy;
		}

		protected void CopyDeclStruct(List<SemanticTypeDecl> decls, List<SemanticTypeStruct> structs, SemanticTypeStruct sts)
		{
			// Ignore duplicates.
			if (!decls.Contains(sts.DeclType))
			{
				decls.Add(sts.DeclType);
				structs.Add(sts);

				sts.SemanticElements.ForEach(child => CopyDeclStruct(decls, structs, (SemanticTypeStruct)child.Element.Struct));
			}
		}

		protected void RecurseGetFullyQualifiedNativeTypes(ISemanticTypeStruct st, string stack, string alias, bool isUnique, int ordinality, bool recurse, List<IFullyQualifiedNativeType> fqntList)
		{
			foreach (INativeType nativeType in st.NativeTypes)
			{
				string ntalias = (!String.IsNullOrEmpty(alias) ? alias : nativeType.Alias);
				fqntList.Add(new FullyQualifiedNativeType() 
				{ 
					FullyQualifiedName = stack + "." + nativeType.Name, 
					NativeType = nativeType, 
					Alias = ntalias,
					UniqueField = nativeType.UniqueField || isUnique,
					Ordinality = nativeType.Ordinality + ordinality
				});
			}

			if (recurse)
			{
				foreach (ISemanticElement childElem in st.SemanticElements)
				{
					stack = stack + "." + childElem.Name;			// push
					ISemanticTypeStruct stChild = GetSemanticTypeStruct(childElem.Name);

					// Check first if the child element has an alias.  If so (and no prior alias is overriding) then use the alias defined in the element.
					string newAlias = (!String.IsNullOrEmpty(alias) ? alias : childElem.Alias);

					// Check next if the semantic type itself has an alias.  If so (and no prior alias is overriding) then use the alias defined for the semantic type of the child element.
					// Confusing, isn't it?
					newAlias = (!String.IsNullOrEmpty(newAlias) ? newAlias : stChild.Alias);

					int newOrdinality = ((ordinality != 0) ? ordinality : childElem.Ordinality);
					RecurseGetFullyQualifiedNativeTypes(stChild, stack, newAlias, isUnique || stChild.Unique, newOrdinality, recurse, fqntList);
					stack = stack.LeftOfRightmostOf('.');			// pop
				}
			}
		}

		protected void RecurseGetFullyQualifiedNativeTypeValues(object signal, ISemanticTypeStruct st, string stack, string alias, bool isUnique, int ordinality, bool recurse, List<IFullyQualifiedNativeType> fqntList)
		{
			foreach (INativeType nativeType in st.NativeTypes)
			{
				// Acquire value through reflection.
				PropertyInfo pi = signal.GetType().GetProperty(nativeType.Name);
				object val = pi.GetValue(signal);
				string ntalias = (!String.IsNullOrEmpty(alias) ? alias : nativeType.Alias);

				fqntList.Add(new FullyQualifiedNativeType() 
				{ 
					FullyQualifiedName = stack + "." + nativeType.Name, 
					Alias = ntalias,
					NativeType = nativeType,
					Value = val,
					UniqueField = nativeType.UniqueField || isUnique,
					Ordinality = nativeType.Ordinality + ordinality
				});
			}

			if (recurse)
			{
				foreach (ISemanticElement childElem in st.SemanticElements)
				{
					// Acquire child SE through reflection:
					PropertyInfo piSub = signal.GetType().GetProperty(childElem.Name);
					object childSignal = piSub.GetValue(signal);

					stack = stack + "." + childElem.Name;			// push
					ISemanticTypeStruct stChild = GetSemanticTypeStruct(childElem.Name);
					string newAlias = (!String.IsNullOrEmpty(alias) ? alias : stChild.Alias);
					int newOrdinality = ((ordinality != 0) ? ordinality : childElem.Ordinality);
					RecurseGetFullyQualifiedNativeTypeValues(childSignal, stChild, stack, newAlias, isUnique || stChild.Unique, newOrdinality, recurse, fqntList);
					stack = stack.LeftOfRightmostOf('.');			// pop
				}
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
			SemanticTypes = SemanticTypes.MergeNonDuplicates(stDict);
		}

		public string GenerateCode()
		{
			return CodeGenerator.Generate(SemanticTypes);
		}
	}
}

