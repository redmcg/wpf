using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;

class Xaml2Cs
{
	public Xaml2Cs ()
	{
		types = new Dictionary<string,XamlType>();

		types["FocusManager"] = new XamlType("System.Windows.Input", "FocusManager");
		types["FrameworkElement"] = new XamlType("System.Windows", "FrameworkElement");
		types["KeyboardNavigation"] = new XamlType("System.Windows.Input", "KeyboardNavigation");
		types["ResourceDictionary"] = new XamlType("System.Windows", "ResourceDictionary");
		types["SolidColorBrush"] = new XamlType("System.Windows.Media", "SolidColorBrush");
		types["ToolBar"] = new XamlType("System.Windows.Controls", "ToolBar");
		types["ToolBarTray"] = new XamlType("System.Windows.Controls", "ToolBarTray");
		types["bool"] = new XamlType(null, "bool");
		types["KeyboardNavigationMode"] = new XamlType("System.Windows.Input", "KeyboardNavigationMode");
		types["KeyboardNavigationMode"].is_enum = true;

		types["ToolBar"].base_type = types["FrameworkElement"];

		types["FocusManager"].AddProperty(types["bool"], "IsFocusScope", true);
		types["FrameworkElement"].AddProperty(types["ResourceDictionary"], "Resources", false);
		types["FrameworkElement"].props["Resources"].auto = true;
		types["KeyboardNavigation"].AddProperty(types["KeyboardNavigationMode"], "DirectionalNavigation", true);
		types["KeyboardNavigation"].AddProperty(types["KeyboardNavigationMode"], "TabNavigation", true);
		types["ToolBarTray"].AddProperty(types["bool"], "IsLocked", true);

		elements_by_local = new Dictionary<string,XamlElement>();
	}

	class XamlType
	{
		public string ns;
		public string name;
		public Dictionary<string, XamlProperty> props = new Dictionary<string, XamlProperty>();
		public bool is_enum;
		public XamlType base_type;

		public XamlType(string ns, string name)
		{
			this.ns = ns;
			this.name = name;
		}

		public void AddProperty(XamlType value_type, string name, bool attached)
		{
			var new_prop = new XamlProperty();
			new_prop.name = name;
			new_prop.container_type = this;
			new_prop.value_type = value_type;
			new_prop.attached = attached;
			this.props[name] = new_prop;
		}
	}

	class XamlProperty
	{
		public string name;
		public XamlType container_type;
		public XamlType value_type;
		public bool attached;
		public bool auto;
	}

	Dictionary<string, XamlType> types;

	HashSet<string> namespaces = new HashSet<string>();

	class XamlElement
	{
		public XamlType type;
		public XamlElement parent;
		public List<XamlElement> children = new List<XamlElement>();
		public string local_name;
		public string class_name;
		public string class_modifier = "";
		public string name;
		public List<string> early_init = new List<string>();
		public List<string> late_init = new List<string>();
		public XamlProperty prop;
		public string key;
	}

	XamlElement root_element;
	
	Dictionary<string, XamlElement> elements_by_local;

	string attribute_string_to_expression(XamlElement element, XamlProperty prop, string str)
	{
		string value_expression;
		if (prop.value_type.name == "bool" &&
			str == "True")
		{
			value_expression = "true";
		}
		else if (prop.value_type.name == "bool" &&
			str == "False")
		{
			value_expression = "false";
		}
		else if (prop.value_type.is_enum)
		{
			if (prop.value_type.ns != null)
				namespaces.Add(prop.value_type.ns);
			value_expression = String.Format("{0}.{1}", prop.value_type.name, str);
		}
		else
		{
			throw new Exception("failed converting property value");
		}

		return str;
	}

	public void ReadXaml(string filename, string[] references)
	{
		XamlElement current = null;
		using (var reader = XmlReader.Create(filename))
		{
			while (reader.Read())
			{
				if (reader.NodeType == XmlNodeType.Element)
				{
					var parent = current;
					current = new XamlElement();
					current.parent = parent;
					string local_name;
					if (root_element == null)
					{
						root_element = current;
					}
					if (reader.Name.Contains("."))
					{
						int index = reader.Name.LastIndexOf(".");
						string container_typename = reader.Name.Substring(0, index);
						string container_propname = reader.Name.Substring(index+1);
						XamlProperty prop = null;
						XamlType container_type = null;
						if (!types.TryGetValue(container_typename, out container_type))
							throw new NotImplementedException(String.Format("type {0}", container_typename));
						while (container_type != null)
						{
							if (container_type.props.TryGetValue(container_propname, out prop))
								break;
							container_type = container_type.base_type;
						}

						if (prop == null)
							throw new NotImplementedException(String.Format("element {0}", reader.Name));

						current.type = prop.value_type;
						current.prop = prop;

						if (reader.GetAttribute("x:Name") != null &&
							!elements_by_local.ContainsKey(reader.GetAttribute("x:Name")))
						{
							local_name = reader.GetAttribute("x:Name");
						}
						else if (reader.GetAttribute("x:Uid") != null &&
							!elements_by_local.ContainsKey(reader.GetAttribute("x:Uid")))
						{
							local_name = reader.GetAttribute("x:Uid");
						}
						else
						{
							int i = 0;
							do
							{
								local_name = String.Format("{0}{1}", current.type.name.ToLower(), i);
							} while (elements_by_local.ContainsKey(local_name));
						}

						if (prop.auto)
						{
							current.early_init.Add(String.Format("{0} {1} = {2}.{3};", current.type.name, local_name, parent.local_name, prop.name));
						}
						else
							throw new NotImplementedException("property creation");
					}
					else if (!types.TryGetValue(reader.Name, out current.type))
					{
						throw new NotImplementedException(String.Format("type {0}", reader.Name));
					}
					else
					{
						if (current == root_element) {
							local_name = "this";
						}
						else if (reader.GetAttribute("x:Name") != null &&
							!elements_by_local.ContainsKey(reader.GetAttribute("x:Name")))
						{
							local_name = reader.GetAttribute("x:Name");
						}
						else if (reader.GetAttribute("x:Uid") != null &&
							!elements_by_local.ContainsKey(reader.GetAttribute("x:Uid")))
						{
							local_name = reader.GetAttribute("x:Uid");
						}
						else
						{
							int i = 0;
							do
							{
								local_name = String.Format("{0}{1}", current.type.name.ToLower(), i);
							} while (elements_by_local.ContainsKey(local_name));
						}
						if (current != root_element) {
							current.early_init.Add(String.Format("{0} {1} = new {0}();", current.type.name, local_name));
						}
					}
					if (current.type.ns != null)
					{
						namespaces.Add(current.type.ns);
					}
					current.local_name = local_name;
					elements_by_local[local_name] = current;
					if (current.parent != null)
					{
						current.parent.children.Add(current);
					}
					if (reader.MoveToFirstAttribute())
					{
						do {
							bool handled_attribute = false;
							switch (reader.Name)
							{
							case "xmlns":
							case "xmlns:x":
							case "xmlns:ui":
							case "xml:lang":
							case "x:Uid":
							case "xmlns:MappingPIGen1":
								// Ignore
								handled_attribute = true;
								break;
							case "x:Class":
								current.class_name = reader.Value;
								handled_attribute = true;
								break;
							case "x:ClassModifier":
								current.class_modifier = reader.Value;
								handled_attribute = true;
								break;
							case "x:Name":
								current.name = reader.Value;
								current.early_init.Add(String.Format("{0}.Name = \"{1}\";", current.local_name, current.name));
								handled_attribute = true;
								break;
							case "x:Key":
								current.key = reader.Value;
								current.late_init.Add(String.Format("{0}.Add(\"{1}\",{2});", parent.local_name, current.key, current.local_name));
								handled_attribute = true;
								break;
							}
							if (!handled_attribute && reader.Name.Contains("."))
							{
								var index = reader.Name.LastIndexOf(".");
								var typename = reader.Name.Substring(0, index);
								XamlType container_type;
								if (types.TryGetValue(typename, out container_type))
								{
									var attrname = reader.Name.Substring(index+1);
									XamlProperty prop;
									if (container_type.props.TryGetValue(attrname, out prop))
									{
										string value_expression = attribute_string_to_expression(current, prop, reader.Value);
										if (prop.attached)
										{
											current.early_init.Add(String.Format("{0}.SetValue({1}.{2}Property, {3});", current.local_name, prop.container_type.name, prop.name, value_expression));
										}
										else
										{
											current.early_init.Add(String.Format("{0}.{1} = {2};", current.local_name, prop.name, value_expression));
										}
										handled_attribute = true;
									}
								}
							}
							if (!handled_attribute)
							{
								throw new NotImplementedException(String.Format("attribute {0}", reader.Name));
							}
						} while (reader.MoveToNextAttribute());
					}
				}
			}
		}
	}

	private void WriteInitializeComponent(TextWriter f, XamlElement element)
	{
		foreach (var line in element.early_init)
		{
			f.WriteLine(line);
		}
		foreach (var child in element.children)
		{
			WriteInitializeComponent(f, child);
		}
		foreach (var line in element.late_init)
		{
			f.WriteLine(line);
		}
	}

	public void WriteCs(TextWriter f)
	{
		f.WriteLine("// This file was generated by xaml2cs");
		f.WriteLine();

		string ns = null;
		string class_name = null;

		int i = root_element.class_name.LastIndexOf(".");
		ns = root_element.class_name.Substring(0, i);
		class_name = root_element.class_name.Substring(i+1);

		foreach (var used_ns in namespaces)
		{
			f.WriteLine("using {0};", used_ns);
		}

		f.WriteLine("namespace {0} {{", ns);
		f.WriteLine("{1} partial class {0} : {2} {{", class_name, root_element.class_modifier, root_element.type.name);
		f.WriteLine("public void InitializeComponent() {");
		WriteInitializeComponent(f, root_element);
		f.WriteLine("}"); // end InitializeComponent
		f.WriteLine("}"); // end class
		f.WriteLine("}"); // end namespace
	}

	public static void Main(string[] arguments)
	{
		var instance = new Xaml2Cs();

		var filename = arguments[0];
		var references = new string[0]; // FIXME

		try
		{
			instance.ReadXaml(filename, references);
		}
		finally
		{
			// Temporary, for testing as we go.
			instance.WriteCs(System.Console.Out);
		}
	}
}
