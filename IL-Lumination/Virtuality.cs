using System.Reflection;

namespace Illumination;

public enum Virtuality
{
	NonVirtual = 0,
	Virtual = MethodAttributes.Virtual,
	Abstract = MethodAttributes.Abstract,
	Override = MethodAttributes.Virtual | MethodAttributes.NewSlot,
	SealedOverride = MethodAttributes.Virtual | MethodAttributes.NewSlot | MethodAttributes.Final
}