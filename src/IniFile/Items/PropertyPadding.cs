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

namespace IniFile.Items;

	/// <summary>
	///     Represents the padding details for a INI property.
	/// </summary>
	public sealed class PropertyPadding : Padding
	{
		/// <summary>
		///     Initializes a new instance of the <see cref="PropertyPadding"/> class.
		/// </summary>
		public PropertyPadding()
		{
			SetDefaults();
		}

		/// <summary>
		///     The amount of space to the right of the property.
		/// </summary>
		public PaddingValue Right { get; set; }

		/// <summary>
		///     The amount of space between the property name and the equal symbol.
		/// </summary>
		public PaddingValue InsideLeft { get; set; }

		/// <summary>
		///     The amount of space between the equal symbol and the property value.
		/// </summary>
		public PaddingValue InsideRight { get; set; }

		/// <inheritdoc/>
		public override void Reset()
		{
			SetDefaults();
		}

		private void SetDefaults()
		{
			Left = Ini.Config.Padding.Property.Left;
			InsideLeft = Ini.Config.Padding.Property.InsideLeft;
			InsideRight = Ini.Config.Padding.Property.InsideRight;
			Right = Ini.Config.Padding.Property.Right;
		}
	}