﻿/*
IniFile Library for .NET
Copyright (c) 2018-2021 Jeevan James
All rights reserved.

Licensed under the Apache License, Version 2.0 (the "License");
you may not use this file except in compliance with the License.
You may obtain a copy of the License at

	http://www.apache.org/licenses/LICENSE-2.0

Unless required by applicable law or agreed to in writing, software
distributed under the License is distributed on an "AS IS" BASIS,
WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
See the License for the specific language governing permissions and
limitations under the License.
*/

using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

using IniFile.Items;

namespace IniFile;

/// <summary>
/// <para> Represents a section object in an INI. </para>
/// <para> A section is also a collection of <see cref="Property"/> instances. </para>
/// </summary>
public sealed partial class Section : MajorIniItem, IPaddedItem<SectionPadding>
{
	/// <summary>
	/// Initializes a new instance of the <see cref="Section"/> class.
	/// </summary>
	/// <param name="name">  The name of the section. </param>
	/// <param name="items"> Collection of strings that represent the comments and blank lines of the section. If the string is <c> null </c>, an empty string or a whitespace string, then a <see cref="BlankLine"/> object is created, otherwise a <see cref="Comment"/> is created. </param>
	/// <inheritdoc/>
	public Section(string name, params string[] items)
		: base(name, items)
	{
	}

	/// <summary>
	/// Padding details of this <see cref="Section"/>.
	/// </summary>
	public SectionPadding Padding { get; } = new();

	/// <inheritdoc/>
	public override string ToString()
	{
		return $"{Padding.Left}[{Padding.InsideLeft}{Name}{Padding.InsideRight}]{Padding.Right}";
	}
}

public sealed partial class Section : IList<Property>
{
	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private readonly List<Property> properties = new();

	/// <summary>
	/// The number of properties in this section.
	/// </summary>
	public int Count => properties.Count;

	public bool IsReadOnly => false;

	public Property this[int index]
	{
		get => properties[index];
		set => properties[index] = value;
	}

	/// <summary>
	/// Gets or sets the value of the property given its name.
	/// </summary>
	/// <param name="name"> The name of the property to get or set. </param>
	/// <returns> The value of the property. </returns>
	public PropertyValue this[string name]
	{
		get
		{
			Property property = properties.Find(p => p.Name == name);
			return property?.Value ?? PropertyValue.Empty;
		}
		set
		{
			Property property = properties.Find(p => p.Name == name);
			if (property == null)
			{
				property = new Property(name, value);
				properties.Add(property);
			}
			else
				property.Value = value;
		}
	}

	public void Add(Property item)
	{
		properties.Add(item);
	}

	public void Clear()
	{
		properties.Clear();
	}

	public bool Contains(Property item)
	{
		return properties.Contains(item);
	}

	public void CopyTo(Property[] array, int arrayIndex)
	{
		properties.CopyTo(array, arrayIndex);
	}

	public IEnumerator<Property> GetEnumerator()
	{
		return properties.GetEnumerator();
	}

	public int IndexOf(Property item)
	{
		return properties.IndexOf(item);
	}

	public void Insert(int index, Property item)
	{
		properties.Insert(index, item);
	}

	public bool Remove(Property item)
	{
		return properties.Remove(item);
	}

	public void RemoveAt(int index)
	{
		properties.RemoveAt(index);
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return properties.GetEnumerator();
	}
}