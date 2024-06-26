﻿#region --- License & Copyright Notice ---
/*
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
#endregion

using System;
using System.Globalization;

using IniFile.Items;

namespace IniFile.Config;

	/// <summary>
	///     Global configuration for reading and writing to INI files.
	/// </summary>
	public sealed class IniGlobalConfig
	{
		/// <summary>
		///     Configuration for using hash symbols (#) for comments in INI files.
		/// </summary>
		public HashCommentConfig HashForComments { get; } = new HashCommentConfig();

		/// <summary>
		///     Configuration for padding defaults for sections, properties and comments in INI files.
		/// </summary>
		public PaddingConfig Padding { get; } = new PaddingConfig();

		/// <summary>
		///     Configuration for handling certain data types when reading and writing INI properties.
		/// </summary>
		public TypesConfig Types { get; } = new TypesConfig();

		/// <summary>
		///     Allows the hash symbol (#) to be used to prefix comments.
		/// </summary>
		/// <param name="setAsDefault">Sets the default comment prefix to the hash symbol (#).</param>
		/// <returns>
		///     The same <see cref="IniGlobalConfig"/> instance so that multiple calls can be chained.
		/// </returns>
		public IniGlobalConfig AllowHashForComments(bool setAsDefault = false)
		{
			HashForComments.Allow = true;
			HashForComments.IsDefault = setAsDefault;
			return this;
		}

		public IniGlobalConfig SetSectionPaddingDefaults(PaddingValue? left = null, PaddingValue? insideLeft = null,
			PaddingValue? insideRight = null, PaddingValue? right = null)
		{
			Padding.Section.Left = left ?? 0;
			Padding.Section.InsideLeft = insideLeft ?? 0;
			Padding.Section.InsideRight = insideRight ?? 0;
			Padding.Section.Right = right ?? 0;
			return this;
		}

		public IniGlobalConfig SetPropertyPaddingDefaults(PaddingValue? left = null, PaddingValue? insideLeft = null,
			PaddingValue? insideRight = null, PaddingValue? right = null)
		{
			Padding.Property.Left = left ?? 0;
			Padding.Property.InsideLeft = insideLeft ?? 1;
			Padding.Property.InsideRight = insideRight ?? 1;
			Padding.Property.Right = right ?? 1;
			return this;
		}

		public IniGlobalConfig SetCommentPaddingDefaults(PaddingValue? left = null, PaddingValue? inside = null,
			PaddingValue? right = null)
		{
			Padding.Comment.Left = left ?? 0;
			Padding.Comment.Inside = inside ?? 1;
			Padding.Comment.Right = right ?? 1;
			return this;
		}

		/// <summary>
		///     Sets the default string values to represent <c>true</c> and <c>false</c> when writing
		///     boolean properties.
		/// </summary>
		/// <param name="trueString">The string value to represent <c>true</c>.</param>
		/// <param name="falseString">The string value to represent <c>false</c>.</param>
		/// <returns>
		///     The same <see cref="IniGlobalConfig"/> instance so that multiple calls can be chained.
		/// </returns>
		public IniGlobalConfig SetBooleanStrings(string trueString = "1", string falseString = "0")
		{
			Types.TrueString = string.IsNullOrEmpty(trueString) || trueString.Trim().Length == 0 ? "1" : trueString;
			Types.FalseString = string.IsNullOrEmpty(falseString) || falseString.Trim().Length == 0 ? "0" : falseString;
			return this;
		}

		/// <summary>
		///     Sets the default date and time formats when reading and writing <see cref="DateTime"/>
		///     and <see cref="TimeSpan"/> values.
		/// </summary>
		/// <param name="dateFormat">
		///     The format for dates. If this value is <c>null</c>, then the current culture short
		///     date format is used.
		/// </param>
		/// <returns>
		///     The same <see cref="IniGlobalConfig"/> instance so that multiple calls can be chained.
		/// </returns>
		public IniGlobalConfig SetDateFormats(string? dateFormat = null)
		{
			Types.DateFormat = string.IsNullOrEmpty(dateFormat) || dateFormat.Trim().Length == 0
				? CultureInfo.CurrentCulture.DateTimeFormat.ShortDatePattern
				: dateFormat;
			return this;
		}
	}