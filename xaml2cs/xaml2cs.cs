using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;

class Xaml2Cs
{
	public Xaml2Cs ()
	{
		types = new Dictionary<string,XamlType>();

		types["Binding"] = new XamlType("System.Windows.Data", "Binding");
		types["Border"] = new XamlType("System.Windows.Controls", "Border");
		types["Brush"] = new XamlType("System.Windows.Media", "Brush");
		types["Button"] = new XamlType("System.Windows.Controls", "Button");
		types["Canvas"] = new XamlType("System.Windows.Controls", "Canvas");
		types["Color"] = new XamlType("System.Windows.Media", "Color");
		types["ComponentResourceKey"] = new XamlType("System.Windows", "ComponentResourceKey");
		types["Condition"] = new XamlType("System.Windows", "Condition");
		types["ConditionCollection"] = new XamlType("System.Windows", "ConditionCollection");
		types["ContentControl"] = new XamlType("System.Windows.Controls", "ContentControl");
		types["ContentPresenter"] = new XamlType("System.Windows.Controls", "ContentPresenter");
		types["Control"] = new XamlType("System.Windows.Controls", "Control");
		types["ControlTemplate"] = new XamlType("System.Windows.Controls", "ControlTemplate");
		types["ControlTemplate"].is_template = true;
		types["DependencyProperty"] = new XamlType("System.Windows", "DependencyProperty");
		types["double"] = new XamlType(null, "double");
		types["FocusManager"] = new XamlType("System.Windows.Input", "FocusManager");
		types["FrameworkElement"] = new XamlType("System.Windows", "FrameworkElement");
		types["FrameworkTemplate"] = new XamlType("System.Windows", "FrameworkTemplate");
		types["Geometry"] = new XamlType("System.Windows.Media", "Geometry");
		types["GradientBrush"] = new XamlType("System.Windows.Media", "GradientBrush");
		types["GradientStop"] = new XamlType("System.Windows.Media", "GradientStop");
		types["GradientStopCollection"] = new XamlType("System.Windows.Media", "GradientStopCollection");
		types["Grid"] = new XamlType("System.Windows.Controls", "Grid");
		types["HeaderedItemsControl"] = new XamlType("System.Windows.Controls", "HeaderedItemsControl");
		types["ItemsControl"] = new XamlType("System.Windows.Controls", "ItemsControl");
		types["ItemsPresenter"] = new XamlType("System.Windows.Controls", "ItemsPresenter");
		types["KeyboardNavigation"] = new XamlType("System.Windows.Input", "KeyboardNavigation");
		types["Label"] = new XamlType("System.Windows.Controls", "Label");
		types["LinearGradientBrush"] = new XamlType("System.Windows.Media", "LinearGradientBrush");
		types["MenuItem"] = new XamlType("System.Windows.Controls", "MenuItem");
		types["MultiTrigger"] = new XamlType("System.Windows.Markup", "MultiTrigger");
		types["object"] = new XamlType(null, "object");
		types["Panel"] = new XamlType("System.Windows.Controls", "Panel");
		types["Path"] = new XamlType("System.Windows.Shapes", "Path");
		types["PlacementMode"] = new XamlType("System.Windows.Controls.Primitives", "PlacementMode");
		types["PlacementMode"].is_enum = true;
		types["Point"] = new XamlType("System.Windows", "Point");
		types["Popup"] = new XamlType("System.Windows.Controls.Primitives", "Popup");
		types["PopupAnimation"] = new XamlType("System.Windows.Controls.Primitives", "PopupAnimation");
		types["PopupAnimation"].is_enum = true;
		types["PropertyPath"] = new XamlType("System.Windows", "PropertyPath");
		types["RelativeSource"] = new XamlType("System.Windows.Data", "RelativeSource");
		types["ResourceDictionary"] = new XamlType("System.Windows", "ResourceDictionary");
		types["ScrollViewer"] = new XamlType("System.Windows.Controls", "ScrollViewer");
		types["Setter"] = new XamlType("System.Windows", "Setter");
		types["Shape"] = new XamlType("System.Windows.Shapes", "Shape");
		types["SolidColorBrush"] = new XamlType("System.Windows.Media", "SolidColorBrush");
		types["string"] = new XamlType(null, "string");
		types["Style"] = new XamlType("System.Windows", "Style");
		types["TextBoxBase"] = new XamlType("System.Windows.Controls.Primitives", "TextBoxBase");
		types["TextBox"] = new XamlType("System.Windows.Controls.Primitives", "TextBox");
		types["Thickness"] = new XamlType("System.Windows", "Thickness");
		types["ToolBar"] = new XamlType("System.Windows.Controls", "ToolBar");
		types["ToolBarTray"] = new XamlType("System.Windows.Controls", "ToolBarTray");
		types["Trigger"] = new XamlType("System.Windows", "Trigger");
		types["TriggerCollection"] = new XamlType("System.Windows", "TriggerCollection");
		types["Type"] = new XamlType("System", "Type");
		types["UIElement"] = new XamlType("System.Windows", "UIElement");
		types["bool"] = new XamlType(null, "bool");
		types["HorizontalAlignment"] = new XamlType("System.Windows", "HorizontalAlignment");
		types["HorizontalAlignment"].is_enum = true;
		types["KeyboardNavigationMode"] = new XamlType("System.Windows.Input", "KeyboardNavigationMode");
		types["KeyboardNavigationMode"].is_enum = true;
		types["VerticalAlignment"] = new XamlType("System.Windows", "VerticalAlignment");
		types["VerticalAlignment"].is_enum = true;

		types["Border"].base_type = types["FrameworkElement"];
		types["Button"].base_type = types["Control"];
		types["Canvas"].base_type = types["Panel"];
		types["ContentControl"].base_type = types["Control"];
		types["ContentPresenter"].base_type = types["FrameworkElement"];
		types["Control"].base_type = types["FrameworkElement"];
		types["ControlTemplate"].base_type = types["FrameworkTemplate"];
		types["FrameworkElement"].base_type = types["UIElement"];
		types["Grid"].base_type = types["FrameworkElement"];
		types["HeaderedItemsControl"].base_type = types["ItemsControl"];
		types["ItemsControl"].base_type = types["Control"];
		types["ItemsPresenter"].base_type = types["FrameworkElement"];
		types["Label"].base_type = types["ContentControl"];
		types["LinearGradientBrush"].base_type = types["GradientBrush"];
		types["MenuItem"].base_type = types["HeaderedItemsControl"];
		types["Panel"].base_type = types["FrameworkElement"];
		types["Path"].base_type = types["Shape"];
		types["Popup"].base_type = types["FrameworkElement"];
		types["ScrollViewer"].base_type = types["ContentControl"];
		types["Shape"].base_type = types["FrameworkElement"];
		types["TextBoxBase"].base_type = types["Control"];
		types["TextBox"].base_type = types["TextBoxBase"];
		types["ToolBar"].base_type = types["HeaderedItemsControl"];

		types["ConditionCollection"].add_statement = "{0}.Add({1});";
		types["FrameworkTemplate"].add_statement = "{0}.VisualTree.AppendChild({1});";
		types["GradientStopCollection"].add_statement = "{0}.Add({1});";
		types["ItemsControl"].add_statement = "{0}.Items.Add({1});";
		types["MultiTrigger"].add_statement = "{0}.Setters.Add({1});";
		types["Panel"].add_statement = "{0}.Children.Add({1});";
		types["ResourceDictionary"].add_with_key_statement = "{0}.Add({1}, {2});";
		types["Style"].add_statement = "{0}.Setters.Add({1});";
		types["Trigger"].add_statement = "{0}.Setters.Add({1});";
		types["TriggerCollection"].add_statement = "{0}.Add({1});";

		types["Binding"].AddProperty(types["PropertyPath"], "Path", true);
		types["Binding"].AddProperty(types["RelativeSource"], "RelativeSource", true);
		types["Border"].AddProperty(types["Brush"], "Background", true);
		types["Border"].AddProperty(types["Brush"], "BorderBrush", true);
		types["Border"].AddProperty(types["Thickness"], "BorderThickness", true);
		types["Border"].AddProperty(types["Thickness"], "Padding", true);
		types["ComponentResourceKey"].AddProperty(types["object"], "ResourceId", false);
		types["ComponentResourceKey"].AddProperty(types["Type"], "TypeInTargetAssembly", false);
		types["Condition"].AddProperty(types["DependencyProperty"], "Property", false);
		types["Condition"].AddProperty(types["object"], "Value", false);
		types["Condition"].props["Value"].indirect_property = true;
		types["Control"].AddProperty(types["Brush"], "Background", true);
		types["Control"].AddProperty(types["Brush"], "BorderBrush", true);
		types["Control"].AddProperty(types["Thickness"], "BorderThickness", true);
		types["Control"].AddProperty(types["Thickness"], "Padding", true);
		types["ControlTemplate"].AddProperty(types["Type"], "TargetType", false);
		types["ControlTemplate"].AddProperty(types["TriggerCollection"], "Triggers", false);
		types["ControlTemplate"].props["Triggers"].auto = true;
		types["FocusManager"].AddProperty(types["bool"], "IsFocusScope", true);
		types["FrameworkElement"].AddProperty(types["double"], "Height", true);
		types["FrameworkElement"].AddProperty(types["HorizontalAlignment"], "HorizontalAlignment", false);
		types["FrameworkElement"].AddProperty(types["Thickness"], "Margin", true);
		types["FrameworkElement"].AddProperty(types["string"], "Name", true);
		types["FrameworkElement"].AddProperty(types["ResourceDictionary"], "Resources", false);
		types["FrameworkElement"].props["Resources"].auto = true;
		types["FrameworkElement"].AddProperty(types["Style"], "Style", true);
		types["FrameworkElement"].AddProperty(types["VerticalAlignment"], "VerticalAlignment", false);
		types["FrameworkElement"].AddProperty(types["double"], "Width", true);
		types["GradientBrush"].AddProperty(types["GradientStopCollection"], "GradientStops", false);
		types["GradientBrush"].props["GradientStops"].auto = true;
		types["GradientStop"].AddProperty(types["Color"], "Color", false);
		types["GradientStop"].AddProperty(types["double"], "Offset", false);
		types["KeyboardNavigation"].AddProperty(types["KeyboardNavigationMode"], "DirectionalNavigation", true);
		types["KeyboardNavigation"].AddProperty(types["KeyboardNavigationMode"], "TabNavigation", true);
		types["LinearGradientBrush"].AddProperty(types["Point"], "EndPoint", false);
		types["LinearGradientBrush"].AddProperty(types["Point"], "StartPoint", false);
		types["MultiTrigger"].AddProperty(types["ConditionCollection"], "Conditions", false);
		types["MultiTrigger"].props["Conditions"].auto = true;
		types["object"].AddProperty(types["object"], "_key", false);
		types["object"].AddProperty(types["object"], "_dynamicresource", false);
		types["Path"].AddProperty(types["Geometry"], "Data", true);
		types["Popup"].AddProperty(types["bool"], "AllowsTransparency", true);
		types["Popup"].AddProperty(types["bool"], "IsOpen", true);
		types["Popup"].AddProperty(types["PlacementMode"], "Placement", true);
		types["Popup"].AddProperty(types["PopupAnimation"], "PopupAnimation", true);
		types["Popup"].AddProperty(types["double"], "VerticalOffset", true);
		types["ScrollViewer"].AddProperty(types["bool"], "CanContentScroll", true);
		types["Setter"].AddProperty(types["DependencyProperty"], "Property", false);
		types["Setter"].AddProperty(types["string"], "TargetName", false);
		types["Setter"].AddProperty(types["object"], "Value", false);
		types["Setter"].props["Value"].indirect_property = true;
		types["Shape"].AddProperty(types["Brush"], "Fill", true);
		types["SolidColorBrush"].AddProperty(types["Color"], "Color", false);
		types["Style"].AddProperty(types["Style"], "BasedOn", false);
		types["Style"].AddProperty(types["Type"], "TargetType", false);
		types["Style"].AddProperty(types["object"], "Value", false);
		types["Style"].props["Value"].indirect_property = true;
		types["ToolBarTray"].AddProperty(types["bool"], "IsLocked", true);
		types["Trigger"].AddProperty(types["DependencyProperty"], "Property", false);
		types["Trigger"].AddProperty(types["object"], "Value", false);
		types["UIElement"].AddProperty(types["bool"], "AllowDrop", true);
		types["UIElement"].AddProperty(types["bool"], "Focusable", true);
		types["UIElement"].AddProperty(types["bool"], "IsEnabled", true);
		types["UIElement"].AddProperty(types["bool"], "IsKeyboardFocused", true);
		types["UIElement"].AddProperty(types["bool"], "IsMouseOver", true);
		types["UIElement"].AddProperty(types["double"], "Opacity", true);
		types["UIElement"].AddProperty(types["bool"], "SnapsToDevicePixels", true);

		elements_by_local = new Dictionary<string,XamlElement>();
		elements_by_name = new Dictionary<string,XamlElement>();
		static_resources = new Dictionary<string,XamlElement>();
	}

	class XamlType
	{
		public string ns;
		public string name;
		public Dictionary<string, XamlProperty> props = new Dictionary<string, XamlProperty>();
		public bool is_enum;
		public XamlType base_type;
		public string add_with_key_statement;
		public string add_statement;
		public bool is_template;

		public XamlType(string ns, string name)
		{
			this.ns = ns;
			this.name = name;
		}

		public void AddProperty(XamlType value_type, string name, bool dependency)
		{
			var new_prop = new XamlProperty();
			new_prop.name = name;
			new_prop.container_type = this;
			new_prop.value_type = value_type;
			new_prop.dependency = dependency;
			this.props[name] = new_prop;
		}

		public bool SubclassOf(XamlType other)
		{
			if (other.name == "object")
				return true;
			XamlType mine = this;
			while (mine != null)
			{
				if (mine == other)
					return true;
				mine = mine.base_type;
			}
			return false;
		}

		public bool LookupProp(string name, out XamlProperty prop)
		{
			XamlType container_type = this;
			while (container_type != null)
			{
				if (container_type.props.TryGetValue(name, out prop))
					return true;
				container_type = container_type.base_type;
			}
			prop = null;
			return false;
		}

		public string AddWithKeyStatement(string parent_expr, string key_expr, string value_expr)
		{
			XamlType parent = this;
			while (parent.add_with_key_statement == null)
			{
				if (parent.base_type == null)
					throw new NotImplementedException(String.Format("add with key for {0}", name));
				parent = parent.base_type;
			}
			return String.Format(parent.add_with_key_statement, parent_expr, key_expr, value_expr);
		}

		public string AddStatement(string parent_expr, string value_expr)
		{
			XamlType parent = this;
			while (parent.add_statement == null)
			{
				if (parent.base_type == null)
					throw new NotImplementedException(String.Format("add child for {0}", name));
				parent = parent.base_type;
			}
			return String.Format(parent.add_statement, parent_expr, value_expr);
		}
	}

	class XamlProperty
	{
		public string name;
		public XamlType container_type;
		public XamlType value_type;
		public bool dependency;
		public bool auto;
		public bool indirect_property;
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
		public bool has_attributes;
		public string key;
		public string key_expr;
		public XamlType target_type;
		public string setter_property;
		public bool is_template;
	}

	XamlElement root_element;
	
	Dictionary<string, XamlElement> elements_by_local;

	Dictionary<string, XamlElement> elements_by_name;
	
	Dictionary<string, XamlElement> static_resources;

	string attribute_string_to_expression(XamlElement element, XamlProperty prop, string str)
	{
		if (prop.name == "Property")
		{
			element.setter_property = str;
		}
		if (prop.container_type.name == "Setter"
			&& prop.name == "TargetName")
		{
			element.target_type = elements_by_name[str].type;
		}

		string value_expression;
		if (str.StartsWith("{DynamicResource ") && str.EndsWith("}"))
		{
			value_expression = String.Format("({0})this.FindResource({1})",
				prop.value_type.name,
				attribute_string_to_expression(element, types["object"].props["_dynamicresource"],
					str.Substring(17, str.Length - 18)));
		}
		else if (str.StartsWith("{StaticResource ") && str.EndsWith("}"))
		{
			string key = str.Substring(16, str.Length - 17);
			value_expression = static_resources[key].local_name;
		}
		else if (str.StartsWith("{x:Static ") && str.EndsWith("}"))
		{
			value_expression = str.Substring(10, str.Length - 11);
		}
		else if (str.StartsWith("{x:Type ") && str.EndsWith("}"))
		{
			string type_name = str.Substring(8, str.Length - 9);
			XamlType type;
			if (!types.TryGetValue(type_name, out type))
				throw new NotImplementedException(String.Format("type {0}", type_name));
			if (type.ns != null)
				namespaces.Add(type.ns);
			value_expression = String.Format("typeof({0})", type.name);
			if (prop.name == "TargetType")
				element.target_type = type;
		}
		else if (str == "{x:Null}")
		{
			value_expression = "null";
		}
		else if (str.StartsWith("{Binding ") && str.EndsWith("}"))
		{
			string attributes_str = str.Substring(9, str.Length - 10);
			var initializers = new List<string>();
			XamlType binding_type = types["Binding"];
			foreach (string prop_str in attributes_str.Split(','))
			{
				string trimmed_str = prop_str.Trim();
				string[] parts = trimmed_str.Split(new char[] {'='}, 2);
				XamlProperty binding_prop;
				if (!binding_type.LookupProp(parts[0], out binding_prop))
					throw new NotImplementedException(string.Format("Binding property {0}", parts[0]));
				string prop_val_expr = attribute_string_to_expression(null, binding_prop, parts[1]);
				initializers.Add(String.Format("{0} = {1}, ", parts[0], prop_val_expr));
			}
			return String.Format("new Binding{{ {0} }}", String.Join("", initializers));
		}
		else if (str.StartsWith("{TemplateBinding ") && str.EndsWith("}"))
		{
			string binding_target = str.Substring(17, str.Length - 18);
			XamlType binding_type = types["Binding"];
			return String.Format("new Binding{{ RelativeSource = new RelativeSource(RelativeSourceMode.TemplatedParent), Path = new PropertyPath(\"{0}\"), }}", binding_target);
		}
		else if (str.StartsWith("{RelativeSource ") && str.EndsWith("}"))
		{
			if (prop.value_type.ns != null)
				namespaces.Add(prop.value_type.ns);
			string contents = str.Substring(16, str.Length - 17);
			if (contents == "TemplatedParent")
			{
				value_expression = String.Format("new RelativeSource(RelativeSourceMode.{0})", contents);
			}
			else
			{
				throw new NotImplementedException(String.Format("RelativeSource {0}", contents));
			}
		}
		else if (str.StartsWith("{ComponentResourceKey ") && str.EndsWith("}"))
		{
			string attributes_str = str.Substring(22, str.Length - 23);
			var initializers = new List<string>();
			XamlType result_type = types["ComponentResourceKey"];
			foreach (string prop_str in attributes_str.Split(','))
			{
				string trimmed_str = prop_str.Trim();
				string[] parts = trimmed_str.Split(new char[] {'='}, 2);
				XamlProperty result_prop;
				if (!result_type.LookupProp(parts[0], out result_prop))
					throw new NotImplementedException(string.Format("ComponentResourceKey property {0}", parts[0]));
				string prop_val_expr = attribute_string_to_expression(null, result_prop, parts[1]);
				initializers.Add(String.Format("{0} = {1}, ", parts[0], prop_val_expr));
			}
			return String.Format("new ComponentResourceKey{{ {0} }}", String.Join("", initializers));
		}
		else if (prop.indirect_property)
		{
			XamlType actual_type;
			XamlProperty actual_prop;
			actual_type = element.target_type;
			if (!actual_type.LookupProp(element.setter_property, out actual_prop))
				throw new NotImplementedException(String.Format("property {0}", element.setter_property));
			return attribute_string_to_expression(element, actual_prop, str);
		}
		else if (str.StartsWith("{"))
		{
			throw new Exception(String.Format("failed converting property value {0}", str));
		}
		else if (prop.value_type.name == "bool" &&
			String.Compare(str, "True", true) == 0)
		{
			value_expression = "true";
		}
		else if (prop.value_type.name == "bool" &&
			String.Compare(str, "False", true) == 0)
		{
			value_expression = "false";
		}
		else if (prop.value_type.is_enum)
		{
			if (prop.value_type.ns != null)
				namespaces.Add(prop.value_type.ns);
			value_expression = String.Format("{0}.{1}", prop.value_type.name, str);
		}
		else if (prop.value_type.name == "DependencyProperty")
		{
			XamlType target_type = element.target_type;
			value_expression = String.Format("{0}.{1}Property", target_type.name, str);
		}
		else if (prop.value_type.name == "Color" && str.StartsWith("#") && str.Length == 9)
		{
			if (prop.value_type.ns != null)
				namespaces.Add(prop.value_type.ns);
			byte a, r, g, b;
			a = Convert.ToByte(str.Substring(1, 2), 16);
			r = Convert.ToByte(str.Substring(3, 2), 16);
			g = Convert.ToByte(str.Substring(5, 2), 16);
			b = Convert.ToByte(str.Substring(7, 2), 16);
			value_expression = String.Format("Color.FromArgb({0}, {1}, {2}, {3})", a, r, g, b);
		}
		else if (prop.value_type.name == "Brush" && str == "Transparent")
		{
			if (prop.value_type.ns != null)
				namespaces.Add(prop.value_type.ns);
			value_expression = String.Format("Brushes.{0}", str);
		}
		else if (prop.value_type.name == "Geometry")
		{
			if (prop.value_type.ns != null)
				namespaces.Add(prop.value_type.ns);
			value_expression = String.Format("Geometry.Parse(\"{0}\")", str);
		}
		else if (prop.value_type.name == "Point" && str.Contains(","))
		{
			if (prop.value_type.ns != null)
				namespaces.Add(prop.value_type.ns);
			value_expression = String.Format("new Point({0})", str);
		}
		else if (prop.value_type.name == "Thickness")
		{
			if (prop.value_type.ns != null)
				namespaces.Add(prop.value_type.ns);
			value_expression = String.Format("new Thickness({0})", str);
		}
		else if (prop.value_type.name == "PropertyPath")
		{
			if (prop.value_type.ns != null)
				namespaces.Add(prop.value_type.ns);
			value_expression = String.Format("new PropertyPath(\"{0}\")", str);
		}
		else if (prop.value_type.name == "double")
		{
			Double.Parse(str);
			value_expression = str;
		}
		else if (prop.value_type.name == "object" || prop.value_type.name == "string")
		{
			value_expression = String.Format("\"{0}\"", str);
		}
		else
		{
			throw new Exception(String.Format("failed converting property value {0}", str));
		}

		return value_expression;
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
					bool empty = reader.IsEmptyElement;
					if (root_element == null)
					{
						root_element = current;
					}
					if (parent != null)
						current.target_type = parent.target_type;
					if (reader.Name.Contains("."))
					{
						int index = reader.Name.LastIndexOf(".");
						string container_typename = reader.Name.Substring(0, index);
						string container_propname = reader.Name.Substring(index+1);
						XamlProperty prop = null;
						XamlType container_type = null;
						if (!types.TryGetValue(container_typename, out container_type))
							throw new NotImplementedException(String.Format("type {0}", container_typename));
						if (!container_type.LookupProp(container_propname, out prop))
							throw new NotImplementedException(String.Format("property {0}", container_propname));
						container_type = prop.container_type;

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
								i++;
							} while (elements_by_local.ContainsKey(local_name));
						}

						if (prop.auto)
						{
							current.early_init.Add(String.Format("{0} {1} = {2}.{3};", current.type.name, local_name, parent.local_name, prop.name));
						}
						else
						{
							current.early_init.Add(String.Format("{0} {1} = new {2}();", current.type.name, local_name, prop.value_type.name));
							current.late_init.Add(String.Format("{0}.{1} = {2};", parent.local_name, prop.name, local_name));
						}
					}
					else if (!types.TryGetValue(reader.Name, out current.type))
					{
						throw new NotImplementedException(String.Format("type {0}", reader.Name));
					}
					else
					{
						current.is_template = (parent != null &&
							(parent.is_template || parent.type.is_template));

						bool needs_declaration = true;
						if (current == root_element) {
							local_name = "this";
							needs_declaration = false;
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
								i++;
							} while (elements_by_local.ContainsKey(local_name));
						}
						if (needs_declaration) {
							if (current.is_template)
							{
								if (reader.GetAttribute("x:Name") != null)
									current.early_init.Add(String.Format("FrameworkElementFactory {1} = new FrameworkElementFactory(typeof({0}), \"{1}\");", current.type.name, local_name, reader["x:Name"]));
								else
									current.early_init.Add(String.Format("FrameworkElementFactory {1} = new FrameworkElementFactory(typeof({0}));", current.type.name, local_name));
							}
							else
								current.early_init.Add(String.Format("{0} {1} = new {0}();", current.type.name, local_name));
							if (current.parent != null &&
								!current.parent.has_attributes &&
								current.parent.prop != null &&
								current.type.SubclassOf(current.parent.prop.value_type))
							{
								current.prop = current.parent.prop;
								current.parent.early_init.Clear();
								current.parent.late_init.Clear();
								if (current.parent.prop.dependency)
								{
									current.parent.late_init.Add(String.Format("{0}.SetValue({1}.{2}Property, {3});", current.parent.parent.local_name, current.parent.prop.container_type.name, current.parent.prop.name, local_name));
								}
								else
								{
									current.early_init.Add(String.Format("{0}.{1} = {2};", current.parent.parent.local_name, current.parent.prop.name, local_name));
								}
							}
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
						current.has_attributes = true;
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
								elements_by_name[reader.Value] = current;
								handled_attribute = true;
								break;
							case "x:Key":
								current.key = reader.Value;
								XamlProperty key_prop;
								types["object"].LookupProp("_key", out key_prop);
								current.key_expr = attribute_string_to_expression(current, key_prop, reader.Value);
								static_resources.Add(current.key, current);
								handled_attribute = true;
								break;
							}
							if (!handled_attribute)
							{
								XamlProperty prop = null;
								if (reader.Name.Contains("."))
								{
									var index = reader.Name.LastIndexOf(".");
									var typename = reader.Name.Substring(0, index);
									XamlType container_type;
									if (types.TryGetValue(typename, out container_type))
									{
										var attrname = reader.Name.Substring(index+1);
										container_type.LookupProp(attrname, out prop);
									}
								}
								else
								{
									current.type.LookupProp(reader.Name, out prop);
								}
								if (prop != null)
								{
									string value_expression = attribute_string_to_expression(current, prop, reader.Value);
									if (prop.dependency)
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
							if (!handled_attribute)
							{
								throw new NotImplementedException(String.Format("attribute {0}", reader.Name));
							}
						} while (reader.MoveToNextAttribute());
					}
					if (current.parent != null &&
						current.prop == null)
					{
						if (current.parent.is_template)
						{
							current.late_init.Add(String.Format("{0}.AppendChild({1});",
								current.parent.local_name, current.local_name));
						}
						else if (current.key != null)
						{
							current.late_init.Add(
								current.parent.type.AddWithKeyStatement(
									current.parent.local_name,
									current.key_expr,
									current.local_name));
						}
						else
						{
							current.late_init.Add(
								current.parent.type.AddStatement(
									current.parent.local_name,
									current.local_name));
						}
					}
					if (empty)
					{
						current = current.parent;
					}
				}
				else if (reader.NodeType == XmlNodeType.EndElement)
				{
					current = current.parent;
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
