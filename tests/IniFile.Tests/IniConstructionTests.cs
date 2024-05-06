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

using System;
using System.IO;

using Moq;

using Shouldly;

using Xunit;

namespace IniFile.Tests;

public sealed class IniConstructionTests
{
	[Fact]
	public void CtorCreatesInstance()
	{
		var ini = new Ini();

		_ = ini.ShouldNotBeNull();
	}

	[Fact]
	public void CtorWithNonNullSettings()
	{
		var ini = new Ini(IniLoadSettings.Default);

		_ = ini.ShouldNotBeNull();
	}

	[Fact]
	public void CtorWithNonReadableStreamWillThrow()
	{
		var mock = new Mock<Stream>();
		_ = mock.Setup(s => s.CanRead).Returns(false);

		_ = Should.Throw<ArgumentException>(() => new Ini(mock.Object));
	}

	[Fact]
	public void CtorWithNullSettings()
	{
		var ini = new Ini(null);

		_ = ini.ShouldNotBeNull();
	}

	[Fact]
	public void CtorWithNullStreamWillThrow()
	{
		_ = Should.Throw<ArgumentNullException>(() => new Ini((Stream)null));
	}

	[Fact]
	public void CtorWithNullTextreaderShouldThrow()
	{
		_ = Should.Throw<ArgumentNullException>(() => new Ini((TextReader)null));
	}

	[Fact]
	public void LoadWithNullContentString()
	{
		_ = Should.Throw<ArgumentNullException>(() => Ini.Load(null));
	}
}