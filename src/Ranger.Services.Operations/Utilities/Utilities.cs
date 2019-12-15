using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ranger.Common;
using Ranger.InternalHttpClient;

namespace Ranger.Services.Operations
{
    public static class Utilities
    {
        public static async Task<IEnumerable<string>> GetProjectNamesForAuthorizedProjectsAsync(string domain, string email, RolesEnum role, IEnumerable<string> authorizedProjects, IProjectsClient projectsClient)
        {
            var projects = await projectsClient.GetAllProjectsForUserAsync<IEnumerable<ProjectModel>>(domain, email).ConfigureAwait(false);
            IEnumerable<string> authorizedProjectNames = null;
            if (role != RolesEnum.User)
            {
                authorizedProjectNames = projects.Select(_ => _.Name);
            }
            else
            {
                authorizedProjectNames = projects.Where(_ => authorizedProjects.Contains(_.ProjectId)).Select(_ => _.Name);
            }

            return authorizedProjectNames;
        }
    }
}