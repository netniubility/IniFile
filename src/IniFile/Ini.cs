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

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

#if NETSTANDARD
using System.Threading.Tasks;
#endif

using IniFile.Config;
using IniFile.Items;

namespace IniFile;

/// <summary>
/// <para> In-memory object representation of an INI file. </para>
/// <para> This class is a read-only collection of <see cref="Section"/> objects. </para>
/// </summary>
[DebuggerDisplay("INI file - {Count} sections")]
public sealed partial class Ini
{
	/// <summary>
	/// Global configuration for the framework.
	/// </summary>
	public static readonly IniGlobalConfig Config = new();

	private static readonly Regex MultilineStartPattern = new(@"^<<(\w+)$");

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private readonly IniLoadSettings settings;

	/// <summary>
	/// Initializes a new empty instance of the <see cref="Ini"/> class with the default settings.
	/// </summary>
	public Ini()
		: this(null)
	{
	}

	/// <summary>
	/// Initializes a new empty instance of the <see cref="Ini"/> class with the specified settings.
	/// </summary>
	/// <param name="settings"> The Ini file settings. </param>
	public Ini(IniLoadSettings settings)
		: base(GetEqualityComparer(settings))
	{
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="Ini"/> class and loads the data from the specified file.
	/// </summary>
	/// <param name="iniFile">  The .ini file to load from. </param>
	/// <param name="settings"> Optional Ini file settings. </param>
	/// <exception cref="ArgumentNullException"> Thrown if the <c> iniFile </c> is <c> null </c>. </exception>
	/// <exception cref="FileNotFoundException"> Thrown if the specified file does not exist. </exception>
	public Ini(FileInfo iniFile, IniLoadSettings? settings = null)
		: base(GetEqualityComparer(settings))
	{
		ArgumentNullException.ThrowIfNull(iniFile);
		if (!iniFile.Exists)
		{
			throw new FileNotFoundException(string.Format(CultureInfo.CurrentCulture, ErrorMessages.IniFileDoesNotExist, iniFile.FullName), iniFile.FullName);
		}

		this.settings = settings ?? IniLoadSettings.Default;

		using var stream = new FileStream(iniFile.FullName, FileMode.Open, FileAccess.Read, FileShare.Read);
		using var reader = new StreamReader(stream, this.settings.Encoding ?? Encoding.UTF8, this.settings.DetectEncoding);
		ParseIniFile(reader);
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="Ini"/> class and loads the data from the specified file.
	/// </summary>
	/// <param name="iniFilePath"> The path to the .ini file. </param>
	/// <param name="settings">    Optional Ini file settings. </param>
	/// <exception cref="ArgumentNullException"> Thrown if the <c> iniFilePath </c> is <c> null </c>. </exception>
	/// <exception cref="FileNotFoundException"> Thrown if the specified file does not exist. </exception>
	public Ini(string iniFilePath, IniLoadSettings? settings = null)
		: base(GetEqualityComparer(settings))
	{
		ArgumentNullException.ThrowIfNull(iniFilePath);
		if (!File.Exists(iniFilePath))
		{
			throw new FileNotFoundException(string.Format(CultureInfo.CurrentCulture, ErrorMessages.IniFileDoesNotExist, iniFilePath), iniFilePath);
		}

		this.settings = settings ?? IniLoadSettings.Default;

		using var stream = new FileStream(iniFilePath, FileMode.Open, FileAccess.Read, FileShare.Read);
		using var reader = new StreamReader(stream, this.settings.Encoding ?? Encoding.UTF8, this.settings.DetectEncoding);
		ParseIniFile(reader);
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="Ini"/> class and loads the data from the specified stream.
	/// </summary>
	/// <param name="stream">   The stream to load from. </param>
	/// <param name="settings"> Optional Ini file settings. </param>
	/// <exception cref="ArgumentNullException"> Thrown if the specified stream is null. </exception>
	/// <exception cref="ArgumentException"> Thrown if the stream cannot be read. </exception>
	public Ini(Stream stream, IniLoadSettings? settings = null)
		: base(GetEqualityComparer(settings))
	{
		ArgumentNullException.ThrowIfNull(stream);
		if (!stream.CanRead)
		{
			throw new ArgumentException(ErrorMessages.StreamNotReadable, nameof(stream));
		}

		this.settings = settings ?? IniLoadSettings.Default;

		using var reader = new StreamReader(stream, this.settings.Encoding ?? Encoding.UTF8, this.settings.DetectEncoding);
		ParseIniFile(reader);
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="Ini"/> class and loads the data from the specified <see cref="TextReader"/>.
	/// </summary>
	/// <param name="reader">   The <see cref="TextReader"/> to load from. </param>
	/// <param name="settings"> Optional Ini file settings. </param>
	/// <exception cref="ArgumentNullException"> Thrown if the specified text reader is null. </exception>
	public Ini(TextReader reader, IniLoadSettings? settings = null)
		: base(GetEqualityComparer(settings))
	{
		ArgumentNullException.ThrowIfNull(reader);

		this.settings = settings ?? IniLoadSettings.Default;

		ParseIniFile(reader);
	}

	/// <summary>
	/// Any trailing comments and blank lines at the end of the INI.
	/// </summary>
	public IList<MinorIniItem> TrailingItems { get; } = new List<MinorIniItem>();

	/// <summary>
	/// Initializes a new instance of the <see cref="Ini"/> class and loads the data from the specified string.
	/// </summary>
	/// <param name="content">  The string representing the Ini content. </param>
	/// <param name="settings"> Optional Ini file settings. </param>
	/// <returns> An instance of the <see cref="Ini"/> class. </returns>
	public static Ini Load(string content, IniLoadSettings? settings = null)
	{
		using var reader = new StringReader(content);
		return new Ini(reader, settings);
	}

	/// <summary>
	/// Formats the INI content by resetting all padding and applying any formatting rules as per the <c> options </c> parameter.
	/// </summary>
	/// <param name="options"> Optional rules for formatting the INI content. Uses default rules ( <see cref="IniFormatOptions.Default"/>) if not specified. </param>
	[SuppressMessage("Major Code Smell", "S1066:Collapsible \"if\" statements should be merged", Justification = "<Pending>")]
	public void Format(IniFormatOptions? options = null)
	{
		options ??= IniFormatOptions.Default;

		for (int s = 0; s < Count; s++)
		{
			Section section = this[s];

			// Reset padding for each minor item in the section
			FormatMinorItems(section.Items);

			// Insert blank line between sections, if specified by options
			if (options.EnsureBlankLineBetweenSections)
			{
				if (s > 0 && (section.Items.Count == 0 || !(section.Items[0] is BlankLine)))
				{
					section.Items.Insert(0, new BlankLine());
				}
			}

			// Reset padding for the section itself
			section.Padding.Reset();

			for (int p = 0; p < section.Count; p++)
			{
				Property property = section[p];

				// Reset padding for each minor item in the property
				FormatMinorItems(property.Items);

				// Insert blank line between properties, if specified
				if (options.EnsureBlankLineBetweenProperties)
				{
					if (p > 0 && (property.Items.Count == 0 || !(property.Items[0] is BlankLine)))
					{
						property.Items.Insert(0, new BlankLine());
					}
				}

				// Reset padding for the property itself
				property.Padding.Reset();
			}
		}

		// Remove any trailing blank lines
		for (int i = TrailingItems.Count - 1; i >= 0; i--)
		{
			// Non blank lines are fine, so when we encounter the first non blank line, exit this loop.
			if (!(TrailingItems[i] is BlankLine))
			{
				break;
			}

			if (TrailingItems[i] is BlankLine)
			{
				TrailingItems.RemoveAt(i);
			}
		}

		// Format any remaining trailing items
		FormatMinorItems(TrailingItems);
	}

	/// <summary>
	/// Saves the <see cref="Ini"/> instance to a file at the specified file path.
	/// </summary>
	/// <param name="filePath"> The path of the file to save to. </param>
	public void SaveTo(string filePath)
	{
		using StreamWriter writer = File.CreateText(filePath);
		SaveTo(writer);
	}

	/// <summary>
	/// Saves the <see cref="Ini"/> instance to the specified file.
	/// </summary>
	/// <param name="file"> The <see cref="FileInfo"/> object that represents the file to save to. </param>
	/// <exception cref="ArgumentNullException"> Thrown if the specified file is null. </exception>
	public void SaveTo(FileInfo file)
	{
		ArgumentNullException.ThrowIfNull(file);
		using StreamWriter writer = File.CreateText(file.FullName);
		SaveTo(writer);
	}

	/// <summary>
	/// Saves the <see cref="Ini"/> instance to the specified stream.
	/// </summary>
	/// <param name="stream"> The stream to save to. </param>
	/// <exception cref="ArgumentNullException"> Thrown if the specified stream is null. </exception>
	/// <exception cref="ArgumentException"> Thrown if the stream cannot be written to. </exception>
	public void SaveTo(Stream stream)
	{
		ArgumentNullException.ThrowIfNull(stream);
		if (!stream.CanWrite)
		{
			throw new ArgumentException(ErrorMessages.StreamNotWritable, nameof(stream));
		}

		using var writer = new StreamWriter(stream);
		SaveTo(writer);
	}

	/// <summary>
	/// Saves the <see cref="Ini"/> instance to the specified text writer.
	/// </summary>
	/// <param name="writer"> The text writer to save to. </param>
	/// <exception cref="ArgumentNullException"> Thrown if the specified text writer is null. </exception>
	public void SaveTo(TextWriter writer)
	{
		ArgumentNullException.ThrowIfNull(writer);
		InternalSave(writer);
		writer.Flush();
	}

	/// <summary>
	/// Constructs a string representing the <see cref="Ini"/> instance data.
	/// </summary>
	/// <returns> A string representing the <see cref="Ini"/> instance data. </returns>
	public override string ToString()
	{
		var sb = new StringBuilder();
		using var writer = new StringWriter(sb);
		InternalSave(writer);
		return sb.ToString();
	}

	private static void AddRangeAndClear(IList<MinorIniItem> source, IList<MinorIniItem> minorItems)
	{
		foreach (MinorIniItem item in minorItems)
		{
			source.Add(item);
		}

		minorItems.Clear();
	}

	/// <summary>
	/// Formats a collection of minor INI items (i.e. <see cref="Comment"/> and <see cref="BlankLine"/>). By default, the paddings for these items are reset, but the method can also optionally remove consecutive blank lines.
	/// </summary>
	/// <param name="minorItems"> The collection of items to format. </param>
	private static void FormatMinorItems(IList<MinorIniItem> minorItems)
	{
		foreach (MinorIniItem minorItem in minorItems)
		{
			if (minorItem is BlankLine blankLine)
			{
				blankLine.Padding.Reset();
			}
			else if (minorItem is Comment comment)
			{
				comment.Padding.Reset();
			}
		}

		for (int i = minorItems.Count - 1; i > 0; i--)
		{
			if (minorItems[i] is BlankLine && minorItems[i - 1] is BlankLine)
			{
				minorItems.RemoveAt(i);
			}
		}
	}

	/// <summary>
	/// Read all lines from the given text reader, parse and generate INI objects such as section, properties, comments and blank lines.
	/// </summary>
	/// <param name="reader"> The <see cref="TextReader"/> instance to read the INI lines from. </param>
	/// <returns> </returns>
	private static IEnumerable<IniItem> ParseLines(TextReader reader)
	{
		// These variables track multiline values
		Property? mlProperty = null;
		var mlValue = new StringBuilder();
		string? mlEot = null;

		for (string line = reader.ReadLine(); line is not null; line = reader.ReadLine())
		{
			// Are we in the middle of a multi-line property value? If so, continue adding the line strings to the mlValue variable until the EOT string is found.
			if (mlProperty is not null)
			{
				// Have we reached the end of the multi-line value (end-of-text)?
				if (line.Trim() == mlEot) // EOT is case-sensitive
				{
					mlProperty.Value = mlValue.ToString();
					mlProperty = null;
				}
				else
				{
					if (mlValue.Length > 0)
					{
						mlValue.AppendLine();
					}

					mlValue.Append(line);
				}
			}
			else
			{
				IniItem iniItem = IniItemFactory.CreateItem(line);

				if (iniItem is Property property)
				{
					// If the property value matches the start of a multiline value, enable multiline mode (mlProperty != null) and add all subsequent lines until the end-of-text marker is found.
					Match match = MultilineStartPattern.Match(property.Value);
					if (match.Success)
					{
						// Start tracking the multiline value until we encounter an end-of-text (EOT) line
						mlProperty = property;
#if NET35
							mlValue = new StringBuilder();
#else
						mlValue.Clear();
#endif
						mlEot = match.Groups[1].Value;
					}
				}

				yield return iniItem;
			}
		}
	}

	/// <summary>
	/// Go through each line object and construct a hierarchical object model, with properties under sections and comments/blank lines under respective sections and properties.
	/// </summary>
	/// <param name="settings">  The INI load settings that control the load behavior. </param>
	/// <param name="lineItems"> </param>
	private void CreateObjectModel(IniLoadSettings settings, IList<IniItem> lineItems)
	{
		Section? currentSection = null;
		var minorItems = new List<MinorIniItem>();
		foreach (IniItem item in lineItems)
		{
			switch (item)
			{
				case BlankLine blankLine when !settings.IgnoreBlankLines:
					minorItems.Add(blankLine);
					break;

				case Comment comment when !settings.IgnoreComments:
					minorItems.Add(comment);
					break;

				case Section section:
					currentSection = section;
					AddRangeAndClear(currentSection.Items, minorItems);
					Add(currentSection);
					break;

				case Property property when currentSection is null:
					throw new FormatException(string.Format(CultureInfo.CurrentCulture, ErrorMessages.PropertyWithoutSection, property.Name));
				case Property property:
					AddRangeAndClear(property.Items, minorItems);
					if (currentSection is null)
					{
						throw new InvalidOperationException(ErrorMessages.CreateObjectModelInvalidCurrentSection);
					}

					currentSection.Add(property);
					break;
			}
		}

		// If there are comments or blank lines at the end of the INI, which cannot be assigned to any section or property, assign them to the TrailingItems collection.
		if (minorItems.Count > 0)
		{
			AddRangeAndClear(TrailingItems, minorItems);
		}
	}

	/// <summary>
	/// Common method called to save the <see cref="Ini"/> instance data to various destinations such as streams, strings, files and text writers.
	/// </summary>
	/// <param name="writer"> The text writer to write the data to. </param>
	private void InternalSave(TextWriter writer)
	{
		foreach (Section section in this)
		{
			foreach (MinorIniItem minorItem in section.Items)
			{
				writer.WriteLine(minorItem.ToString());
			}

			writer.WriteLine(section.ToString());
			foreach (Property property in section)
			{
				foreach (MinorIniItem minorItem in property.Items)
				{
					writer.WriteLine(minorItem.ToString());
				}

				writer.WriteLine(property.ToString());
			}
		}

		foreach (MinorIniItem trailingItem in TrailingItems)
		{
			writer.WriteLine(trailingItem.ToString());
		}
	}

	/// <summary>
	/// Transforms the INI content into an in-memory object model.
	/// </summary>
	/// <param name="reader"> The INI content. </param>
	private void ParseIniFile(TextReader reader)
	{
		// Go through all the lines and build a flat collection of INI objects.
		IList<IniItem> lineItems = ParseLines(reader).ToList();

		// Go through the line objects and construct an object model.
		CreateObjectModel(settings, lineItems);
	}

#if NETSTANDARD
		/// <summary>
		/// Saves the <see cref="Ini"/> instance to the specified stream asynchronously.
		/// </summary>
		/// <param name="stream"> The stream to save to. </param>
		/// <returns> A task representing the asynchronous save operation. </returns>
		/// <exception cref="ArgumentNullException"> Thrown if the specified stream is null. </exception>
		/// <exception cref="ArgumentException"> Thrown if the stream cannot be written to. </exception>
		public Task SaveToAsync(Stream stream)
		{
			if (stream is null)
				throw new ArgumentNullException(nameof(stream));
			if (!stream.CanWrite)
				throw new ArgumentException(ErrorMessages.StreamNotWritable, nameof(stream));
			return SaveToAsyncInternal(stream);
		}

		private async Task SaveToAsyncInternal(Stream stream)
		{
			using var writer = new StreamWriter(stream);
			await SaveToAsync(writer).ConfigureAwait(false);
		}

		/// <summary>
		/// Saves the <see cref="Ini"/> instance to the specified text writer asynchronously.
		/// </summary>
		/// <param name="writer"> The text writer to save to. </param>
		/// <returns> A task representing the asynchronous save operation. </returns>
		/// <exception cref="ArgumentNullException"> Thrown if the specified text writer is null. </exception>
		public Task SaveToAsync(TextWriter writer)
		{
			if (writer is null)
				throw new ArgumentNullException(nameof(writer));
			return SaveToAsyncInternal(writer);
		}

		private async Task SaveToAsyncInternal(TextWriter writer)
		{
			await InternalSaveAsync(writer).ConfigureAwait(false);
			await writer.FlushAsync().ConfigureAwait(false);
		}

		/// <summary>
		/// Common method called to asynchronously save the <see cref="Ini"/> instance data to various destinations such as streams, strings, files and text writers.
		/// </summary>
		/// <param name="writer"> The text writer to write the data to. </param>
		/// <returns> A task representing the asynchronous save operation. </returns>
		private async Task InternalSaveAsync(TextWriter writer)
		{
			foreach (Section section in this)
			{
				foreach (MinorIniItem minorItem in section.Items)
					await writer.WriteLineAsync(minorItem.ToString()).ConfigureAwait(false);
				await writer.WriteLineAsync(section.ToString()).ConfigureAwait(false);
				foreach (Property property in section)
				{
					foreach (MinorIniItem minorItem in property.Items)
						await writer.WriteLineAsync(minorItem.ToString()).ConfigureAwait(false);
					await writer.WriteLineAsync(property.ToString()).ConfigureAwait(false);
				}
			}

			foreach (MinorIniItem trailingItem in TrailingItems)
				await writer.WriteLineAsync(trailingItem.ToString()).ConfigureAwait(false);
		}
#endif
}

public sealed partial class Ini : KeyedCollection<string, Section>
{
	public new Section this[string key]
	{
		get
		{
			ArgumentNullException.ThrowIfNull(key);

			IEqualityComparer<string> comparer = GetEqualityComparer(settings);
			foreach (Section section in this)
			{
				if (comparer.Equals(key, section.Name))
				{
					return section;
				}
			}
			return null;
		}
	}

	protected override string GetKeyForItem(Section item)
	{
		ArgumentNullException.ThrowIfNull(item);
		return item.Name;
	}

	private static IEqualityComparer<string> GetEqualityComparer(IniLoadSettings settings)
	{
		settings ??= IniLoadSettings.Default;
		return settings.CaseSensitive ? StringComparer.Ordinal : StringComparer.OrdinalIgnoreCase;
	}
}