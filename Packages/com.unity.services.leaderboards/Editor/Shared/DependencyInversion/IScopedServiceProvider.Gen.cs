// WARNING: Auto generated code. Modifications will be lost!
// Original source 'com.unity.services.shared' @0.0.11.
using System;

namespace Unity.Services.Leaderboards.DependencyInversion
{
    interface IScopedServiceProvider : IServiceProvider, IDisposable
    {
        IScopedServiceProvider CreateScope();
    }
}
