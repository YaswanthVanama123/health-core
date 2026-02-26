// IApplicationUpdateService.cs — Phase 7 stub
// WCF [ServiceContract]/[OperationContract] attributes removed.
// In Phase 7, this interface will be implemented as a REST API controller.
using GeniView.Cloud.Models;
using System;

namespace GeniView.Cloud.Services
{
    public interface IApplicationUpdateService
    {
        ApplicationUpdate CheckForUpdate(Guid appId, Version currentVersion);
    }
}
