using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ranger.Common;
using Ranger.InternalHttpClient;

namespace Ranger.Services.Operations
{
    public static class Utilities
    {
        public static async Task<IEnumerable<string>> GetProjectNamesForAuthorizedProjectsAsync(string domain, string email, RolesEnum role, IEnumerable<Guid> authorizedProjects, ProjectsHttpClient projectsClient)
        {
            var apiResponse = await projectsClient.GetAllProjectsForUserAsync<IEnumerable<ProjectModel>>(domain, email).ConfigureAwait(false);

            IEnumerable<string> authorizedProjectNames = null;
            if (role != RolesEnum.User)
            {
                authorizedProjectNames = apiResponse.Result.Select(_ => _.Name);
            }
            else
            {
                authorizedProjectNames = apiResponse.Result.Where(_ => authorizedProjects.Contains(_.Id)).Select(_ => _.Name);
            }

            return authorizedProjectNames;
        }
    }
}