﻿#region --- License & Copyright Notice ---
/*
IniFile Library for .NET
Copyright (c) 2018 Jeevan James
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

using System.Linq;
using Shouldly;
using Xunit;
using Xunit.DataAttributes;

namespace IniFile.Tests;

public sealed class IniLoadSettingTests
{
	private readonly IniLoadSettings _settings;

	public IniLoadSettingTests()
	{
		_settings = new IniLoadSettings();
	}

	[Theory]
	[EmbeddedResourceContent("IniFile.Tests.Data.Valid.ini")]
	public void LoadShouldIgnoreBlankLines(string iniContent)
	{
		_settings.IgnoreBlankLines = true;

		Ini ini = Ini.Load(iniContent, _settings);

		foreach (Section section in ini)
		{
			section.Items.OfType<BlankLine>().Count().ShouldBe(0);
			section.Items.OfType<Comment>().Count().ShouldBeGreaterThan(0);
			foreach (Property property in section)
			{
				property.Items.OfType<BlankLine>().Count().ShouldBe(0);
				property.Items.OfType<Comment>().Count().ShouldBeGreaterThan(0);
			}
		}
	}

	[Theory]
	[EmbeddedResourceContent("IniFile.Tests.Data.Valid.ini")]
	public void LoadShouldIgnoreComments(string iniContent)
	{
		_settings.IgnoreComments = true;

		Ini ini = Ini.Load(iniContent, _settings);

		foreach (Section section in ini)
		{
			section.Items.OfType<Comment>().Count().ShouldBe(0);
			section.Items.OfType<BlankLine>().Count().ShouldBeGreaterThan(0);
			foreach (Property property in section)
			{
				property.Items.OfType<Comment>().Count().ShouldBe(0);
				property.Items.OfType<BlankLine>().Count().ShouldBeGreaterThan(0);
			}
		}
	}

	[Theory]
	[EmbeddedResourceContent("IniFile.Tests.Data.Valid.ini")]
	public void LoadShouldIgnoreBlankLinesAndComments(string iniContent)
	{
		_settings.IgnoreBlankLines = true;
		_settings.IgnoreComments = true;

		Ini ini = Ini.Load(iniContent, _settings);

		foreach (Section section in ini)
		{
			section.Items.Count.ShouldBe(0);
			foreach (Property property in section)
			{
				property.Items.Count.ShouldBe(0);
			}
		}
	}
}