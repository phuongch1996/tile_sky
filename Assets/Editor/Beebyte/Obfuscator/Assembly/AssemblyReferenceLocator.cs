#if UNITY_2017_3_OR_NEWER
/*
 * Copyright (c) 2018-2021 Beebyte Limited. All rights reserved.
 */
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.Compilation;

namespace Beebyte.Obfuscator.Assembly
{
	public class AssemblyReferenceLocator
	{
		public static IEnumerable<string> GetAssemblyReferenceDirectories()
		{
			HashSet<string> directories = new HashSet<string>();

			foreach (UnityEditor.Compilation.Assembly assembly in CompilationPipeline.GetAssemblies())
			{
				directories.UnionWith(GetAssemblyReferenceDirectories(assembly));
			}
			directories.Add("./Library/PlayerScriptAssemblies".Replace('/', Path.DirectorySeparatorChar));
			directories.Add("./Library/Bee/PlayerScriptAssemblies".Replace('/', Path.DirectorySeparatorChar));
			directories.UnionWith(GetUnityEditorLibDirectories());
			return directories;
		}

		private static IEnumerable<string> GetUnityEditorLibDirectories()
{
    HashSet<string> libDirectories = new HashSet<string>();

    string applicationContentsPath = EditorApplication.applicationContentsPath;

    // Unity 2017-2022 và một số phiên bản cũ
    string monoBleedingEdgePath = Path.Combine(
        applicationContentsPath,
        "MonoBleedingEdge",
        "lib",
        "mono"
    );

    AddDllDirectories(monoBleedingEdgePath, libDirectories);

    // Unity 6 có thể không còn sử dụng cấu trúc MonoBleedingEdge
    // nên kiểm tra thêm các thư mục .NET/Mono phổ biến.
    string monoPath = Path.Combine(
        applicationContentsPath,
        "Mono",
        "lib",
        "mono"
    );

    AddDllDirectories(monoPath, libDirectories);

    string monoBleedingEdgeRoot = Path.Combine(
        applicationContentsPath,
        "MonoBleedingEdge"
    );

    AddDllDirectories(monoBleedingEdgeRoot, libDirectories);

    return libDirectories;
}

private static void AddDllDirectories(
    string rootDirectory,
    HashSet<string> directories)
{
    if (string.IsNullOrEmpty(rootDirectory) ||
        !Directory.Exists(rootDirectory))
    {
        return;
    }

    Stack<string> stack = new Stack<string>();
    stack.Push(rootDirectory);

    while (stack.Count > 0)
    {
        string dir = stack.Pop();

        if (!Directory.Exists(dir))
            continue;

        try
        {
            if (Directory.GetFiles(dir, "*.dll").Length > 0)
            {
                directories.Add(dir);
            }

            foreach (string childDirectory in Directory.GetDirectories(dir))
            {
                stack.Push(childDirectory);
            }
        }
        catch (DirectoryNotFoundException)
        {
            // Unity có thể thay đổi/xóa thư mục trong lúc refresh.
        }
        catch (IOException)
        {
            // Bỏ qua thư mục không thể đọc.
        }
    }
}


		private static IEnumerable<string> GetAssemblyReferenceDirectories(UnityEditor.Compilation.Assembly assembly)
		{
			HashSet<string> directories = new HashSet<string>();

			if (assembly == null) return directories;

			if (assembly.compiledAssemblyReferences == null || assembly.compiledAssemblyReferences.Length <= 0)
			{
				return directories;
			}
			
			foreach (string assemblyRef in assembly.compiledAssemblyReferences)
			{
				directories.Add(Path.GetDirectoryName(assemblyRef));
			}
			return directories;
		}
	}
}
#endif
