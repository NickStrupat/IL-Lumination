using System.IO;
using System.Reflection;
using System.Reflection.Emit;
using System.Reflection.Metadata;
using System.Reflection.Metadata.Ecma335;
using System.Reflection.PortableExecutable;

namespace Illumination;

public static class PersistedAssemblyBuilderExtensions
{
	public static void Save(this PersistedAssemblyBuilder assemblyBuilder, string filePath, MethodInfo? entryPoint = null)
	{
		MetadataBuilder metadataBuilder = assemblyBuilder.GenerateMetadata(out BlobBuilder ilStream, out BlobBuilder fieldData);

		(MethodDefinitionHandle mdh, Characteristics c) = entryPoint switch
		{
			null => (default, Characteristics.Dll),
			_ => (MetadataTokens.MethodDefinitionHandle(entryPoint.MetadataToken), Characteristics.ExecutableImage)
		};
		
		ManagedPEBuilder peBuilder = new(
			header: new PEHeaderBuilder(imageCharacteristics: c),
			metadataRootBuilder: new MetadataRootBuilder(metadataBuilder),
			ilStream: ilStream,
			mappedFieldData: fieldData,
			entryPoint: mdh
		);

		BlobBuilder peBlob = new();
		peBuilder.Serialize(peBlob);
		
		using FileStream fileStream = new(filePath, FileMode.Create, FileAccess.Write);
		peBlob.WriteContentTo(fileStream);
	}
}