using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Orchard.ModuleBase
{
    public record ResourceDescriptor(
        string Name,
        string VirtualPath,
        string[]? Dependencies = null,
        string? Version = null,
        ResourceLocation Location = ResourceLocation.Footer);

    public enum ResourceLocation { Head, Footer }

    public interface IResourceRegistrar
    {
        void RegisterScript(ResourceDescriptor descriptor);
        void RegisterStyle(ResourceDescriptor descriptor);

        IReadOnlyList<ResourceDescriptor> GetScripts();
        IReadOnlyList<ResourceDescriptor> GetStyles();
    }

}
