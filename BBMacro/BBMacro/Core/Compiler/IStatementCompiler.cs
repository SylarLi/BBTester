using System.Reflection;

public interface IStatementCompiler
{
    MethodInfo[] Compile(string[] statements, string library);
}