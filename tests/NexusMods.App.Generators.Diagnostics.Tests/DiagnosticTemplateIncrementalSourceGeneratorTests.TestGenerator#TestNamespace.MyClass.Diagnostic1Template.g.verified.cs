﻿//HintName: TestNamespace.MyClass.Diagnostic1Template.g.cs
// <auto-generated/>
#nullable enable

namespace TestNamespace;

partial class MyClass
{
	internal static global::NexusMods.Abstractions.Diagnostics.Diagnostic<Diagnostic1MessageData> CreateDiagnostic1(global::NexusMods.Abstractions.Diagnostics.References.ModReference ModA, global::NexusMods.Abstractions.Diagnostics.References.ModReference ModB, global::System.String Something, global::System.Int32 Count)
	{
		var messageData = new Diagnostic1MessageData(ModA, ModB, Something, Count);

		return new global::NexusMods.Abstractions.Diagnostics.Diagnostic<Diagnostic1MessageData>
		{
			Id = new global::NexusMods.Abstractions.Diagnostics.DiagnosticId(source: Source, number: 1),
			Title = "Diagnostic 1",
			Severity = global::NexusMods.Abstractions.Diagnostics.DiagnosticSeverity.Warning,
			Summary = global::NexusMods.Abstractions.Diagnostics.DiagnosticMessage.From("Mod '{ModA}' conflicts with '{ModB}' because it's missing '{Something}' and {Count} other stuff!"),
			Details = global::NexusMods.Abstractions.Diagnostics.DiagnosticMessage.DefaultValue,
			MessageData = messageData,
			DataReferences = new global::System.Collections.Generic.Dictionary<global::NexusMods.Abstractions.Diagnostics.References.DataReferenceDescription, global::NexusMods.Abstractions.Diagnostics.References.IDataReference>
			{
				{ global::NexusMods.Abstractions.Diagnostics.References.DataReferenceDescription.From("ModA"), messageData.ModA },
				{ global::NexusMods.Abstractions.Diagnostics.References.DataReferenceDescription.From("ModB"), messageData.ModB },
			}

			,
		}

		;
	}

	internal readonly struct Diagnostic1MessageData : global::NexusMods.Abstractions.Diagnostics.IDiagnosticMessageData
	{
		public readonly global::NexusMods.Abstractions.Diagnostics.References.ModReference ModA;
		public readonly global::NexusMods.Abstractions.Diagnostics.References.ModReference ModB;
		public readonly global::System.String Something;
		public readonly global::System.Int32 Count;

		public Diagnostic1MessageData(global::NexusMods.Abstractions.Diagnostics.References.ModReference ModA, global::NexusMods.Abstractions.Diagnostics.References.ModReference ModB, global::System.String Something, global::System.Int32 Count)
		{
			this.ModA = ModA;
			this.ModB = ModB;
			this.Something = Something;
			this.Count = Count;
		}

		public void Format(global::NexusMods.Abstractions.Diagnostics.IDiagnosticWriter writer, ref global::NexusMods.Abstractions.Diagnostics.DiagnosticWriterState state, global::NexusMods.Abstractions.Diagnostics.DiagnosticMessage message)
		{
			var value = message.Value;
			var span = value.AsSpan();
			int i;
			var bracesStartIndex = -1;
			var bracesEndIndex = -1;
			for (i = 0; i < value.Length; i++)
			{
				var c = value[i];
				if (bracesStartIndex == -1)
				{
					if (c != '{') continue;
					bracesStartIndex = i;
					var slice = span.Slice(bracesEndIndex + 1, i - bracesEndIndex - 1);
					writer.Write(ref state, slice);
					continue;
				}

				if (c != '}') continue;
				var fieldName = span.Slice(bracesStartIndex + 1, i - bracesStartIndex - 1);
				if (fieldName.Equals(nameof(ModA), global::System.StringComparison.Ordinal))
				{
					writer.Write(ref state, ModA);
				}

				 else if (fieldName.Equals(nameof(ModB), global::System.StringComparison.Ordinal))
				{
					writer.Write(ref state, ModB);
				}

				 else if (fieldName.Equals(nameof(Something), global::System.StringComparison.Ordinal))
				{
					writer.Write(ref state, Something);
				}

				 else if (fieldName.Equals(nameof(Count), global::System.StringComparison.Ordinal))
				{
					writer.WriteValueType(ref state, Count);
				}

				else
				{
					throw new global::System.NotImplementedException();
				}

				bracesStartIndex = -1;
				bracesEndIndex = i;
			}

			if (bracesEndIndex == i - 1) return;
			if (bracesEndIndex == -1)
			{
				writer.Write(ref state, span);
				return;
			}

			var endSlice = span.Slice(bracesEndIndex + 1, i - bracesEndIndex - 1);
			writer.Write(ref state, endSlice);
			return;
		}

	}

}

