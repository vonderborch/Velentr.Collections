using System.Reflection;
using System.Runtime.InteropServices;

namespace Velentr.INDIVIDUAL_SUPPORT.Test;

/// <summary>
///     A helper for FNA dependencies.
/// </summary>
public static class FnaDependencyHelper
{
    /// <summary>
    ///     The default platform map.
    /// </summary>
    private static readonly Dictionary<Architecture, Dictionary<OSPlatform, string>> DefaultPlatformMap = new()
    {
        [Architecture.X64] = new Dictionary<OSPlatform, string>
        {
            [OSPlatform.Windows] = "x64",
            [OSPlatform.Linux] = "lib64",
            [OSPlatform.OSX] = "osx"
        },
        [Architecture.X86] = new Dictionary<OSPlatform, string>
        {
            [OSPlatform.Windows] = "x86"
        },
        [Architecture.Arm64] = new Dictionary<OSPlatform, string>
        {
            [OSPlatform.OSX] = "osx"
        }
    };

    /// <summary>
    ///     Moves the FNA dependencies to the correct location based on the current OS and architecture.
    /// </summary>
    /// <param name="platformMap">An optional platform DLL map.</param>
    /// <param name="vulkanPath">The path to the MoltenVK ICD.</param>
    /// <exception cref="PlatformNotSupportedException">Raised if the platform is unsupported.</exception>
    public static void HandleDependencies(Dictionary<Architecture, Dictionary<OSPlatform, string>>? platformMap = null,
        string vulkanPath = "vulkan")
    {
        Dictionary<Architecture, Dictionary<OSPlatform, string>> map = platformMap ?? DefaultPlatformMap;

        Architecture architecture = RuntimeInformation.OSArchitecture;
        if (!map.TryGetValue(architecture, out Dictionary<OSPlatform, string>? operatingSystems))
        {
            throw new PlatformNotSupportedException();
        }

        foreach ((OSPlatform os, var path) in operatingSystems)
        {
            if (RuntimeInformation.IsOSPlatform(os))
            {
                MoveDlls(path);
                return;
            }
        }

        throw new PlatformNotSupportedException();
    }

    /// <summary>
    ///     Moves the DLLs from the specified path to the executing assembly path.
    /// </summary>
    /// <param name="path">The DLL directory path.</param>
    /// <exception cref="DirectoryNotFoundException">Raised if we can't find the DLL directory.</exception>
    private static void MoveDlls(string path)
    {
        var assemblyPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

        var directory = Path.Combine(assemblyPath, path);
        if (!Directory.Exists(directory))
        {
            throw new DirectoryNotFoundException($"The DLL directory for the platform `{directory}` does not exist.");
        }

        foreach (var file in Directory.GetFiles(directory))
        {
            var destination = Path.Combine(assemblyPath, Path.GetFileName(file));
            if (File.Exists(destination))
            {
                File.Delete(destination);
            }

            File.Copy(file, destination);
        }
    }
}
