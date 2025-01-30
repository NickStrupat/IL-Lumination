// Get the current AppDomain.

using System.Reflection;
using System.Reflection.Emit;

AssemblyName myAssemblyName = new AssemblyName();
myAssemblyName.Name = "SampleAssembly";

// Create a dynamic assembly 'myAssembly' with access mode 'Run'.
AssemblyBuilder myAssembly = AssemblyBuilder.DefineDynamicAssembly(
                        myAssemblyName, AssemblyBuilderAccess.Run);
// Create a dynamic module 'myModule' in 'myAssembly'.
ModuleBuilder myModule=myAssembly.DefineDynamicModule("MyDynamicModule");
// Define a public class 'MyDynamicClass'.
TypeBuilder myTypeBuilder = myModule.DefineType("MyDynamicClass",
                                 TypeAttributes.Public);
// Define a public string field.
FieldBuilder myField = myTypeBuilder.DefineField("MyDynamicField",
                         typeof(String), FieldAttributes.Public);
// Create the constructor.
Type[] myConstructorArgs = {typeof(String)};
ConstructorBuilder myConstructor = myTypeBuilder.DefineConstructor(
   MethodAttributes.Public, CallingConventions.Standard, myConstructorArgs);

// Generate IL for 'myConstructor'.
ILGenerator myConstructorIL = myConstructor.GetILGenerator();
// Emit the necessary opcodes.
myConstructorIL.Emit(OpCodes.Ldarg_0);
ConstructorInfo mySuperConstructor = typeof(Object).GetConstructor(new Type[0]);
myConstructorIL.Emit(OpCodes.Call, mySuperConstructor);
myConstructorIL.Emit(OpCodes.Ldarg_0);
myConstructorIL.Emit(OpCodes.Ldarg_1);
myConstructorIL.Emit(OpCodes.Stfld, myField);
myConstructorIL.Emit(OpCodes.Ret);

// Define a dynamic method named 'MyDynamicMethod'.
MethodBuilder myMethod = myTypeBuilder.DefineMethod(
    name: "MyDynamicMethod", 
    attributes: MethodAttributes.Public,
    returnType: typeof(String),
    parameterTypes: Type.EmptyTypes
);
// Generate IL for 'myMethod'.
ILGenerator il = myMethod.GetILGenerator();

// Begin the scope for a local variable.
il.BeginScope();

LocalBuilder myLocalBuilder = il.DeclareLocal(typeof(string));
// Console.WriteLine("\nTrying to access the local variable within the scope.");
// Console.WriteLine("'myLocalBuilder' type is: {0}", myLocalBuilder.LocalType);
il.Emit(OpCodes.Ldarg_0);
il.Emit(OpCodes.Ldfld, myField);
il.Emit(OpCodes.Stloc, myLocalBuilder);

// End the scope of 'myLocalBuilder'.
il.EndScope();

// Access the local variable outside the scope.
// Console.WriteLine("\nTrying to access the local variable outside the scope:");
// il.Emit(OpCodes.Stloc_0, myLocalBuilder);
il.Emit(OpCodes.Ldloc, myLocalBuilder);
//il.Emit(OpCodes.Ldfld, myField);
//il.Emit(OpCodes.Ldstr, "Local value");
il.Emit(OpCodes.Ret);

// Create 'MyDynamicClass' class.
Type myType1 = myTypeBuilder.CreateType();
var x = myType1.GetMethod("MyDynamicMethod").Invoke(Activator.CreateInstance(myType1, "asdf"), null);
Console.WriteLine(x);

class Foo
{
    public string asdf;
    public string Asdf()
    {
        string x = asdf;
        return x;
    }
}