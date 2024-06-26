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

using System.Diagnostics;

using IniFile.Items;

namespace IniFile;

public enum CommentChar
{
	Semicolon,
	Hash
}

/// <summary>
/// Represents a comment object in an INI.
/// </summary>
public sealed class Comment : MinorIniItem, IPaddedItem<CommentPadding>
{
	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private string text;

	/// <summary>
	/// Initializes a new instance of the <see cref="Comment"/> class with an empty comment.
	/// </summary>
	public Comment()
		: this(string.Empty)
	{
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="Comment"/> class with the specified text.
	/// </summary>
	/// <param name="text"> The comment text. </param>
	public Comment(string text)
	{
		Text = text;
		CommentChar = Ini.Config.HashForComments.Allow && Ini.Config.HashForComments.IsDefault
			? CommentChar.Hash
			: CommentChar.Semicolon;
	}

	public CommentChar CommentChar { get; set; }

	/// <summary>
	/// Padding details of this <see cref="Comment"/>.
	/// </summary>
	public CommentPadding Padding { get; } = new();

	/// <summary>
	/// The comment text.
	/// </summary>
	public string Text
	{
		get => text;
		set => text = value ?? string.Empty;
	}

	/// <inheritdoc/>
	public override string ToString()
	{
		char commentChar = CommentChar == CommentChar.Semicolon ? ';' : '#';
		return $"{Padding.Left}{commentChar}{Padding.Inside}{Text}{Padding.Right}";
	}
}