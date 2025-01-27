﻿using LibGit2Sharp;
using Microsoft.TeamFoundation.SourceControl.WebApi;
using Microsoft.VisualStudio.Services.Common;
using Microsoft.VisualStudio.Services.Organization.Client;
using Microsoft.VisualStudio.Services.WebApi;

namespace REBUSS.AzureDevOps.PullRequestAPI.Git.Model
{
    public class GitClient : IGitClient
    {
        private readonly string repo;
        private readonly string projectName;
        private readonly string organization;
        private readonly string personalAccessToken;

        public GitClient(string organization, string repositoryName, string projectName, string pat)
        {
            repo = repositoryName;
            this.projectName = projectName;
            this.organization = organization;
            this.personalAccessToken = pat;
        }

        public async Task<GitPullRequest> GetPullRequestAsync(int pullRequestId)
        {
            using (var gitClient = GetGitClient())
            {
                return await gitClient.GetPullRequestAsync(projectName, repo, pullRequestId);
            }
        }

        public async Task<GitPullRequestIteration> GetLastIterationAsync(int pullRequestId)
        {
            using (var gitClient = GetGitClient())
            {
                var iterations = await gitClient.GetPullRequestIterationsAsync(projectName, repo, pullRequestId);
                return iterations.Last();
            }
        }

        public async Task<GitPullRequestIterationChanges> GetIterationChangesAsync(int pullRequestId, int iterationId)
        {
            using (var gitClient = GetGitClient())
            {
                return await gitClient.GetPullRequestIterationChangesAsync(projectName, repo, pullRequestId, iterationId);
            }
        }

        public void FetchBranches(IRepository repo, GitPullRequest pullRequest, string[] branchNames)
        {
            var remote = repo.Network.Remotes["origin"];
            var fetchOptions = new FetchOptions
            {
                CredentialsProvider = (url, usernameFromUrl, types) =>
                    new UsernamePasswordCredentials
                    {
                        Username = string.Empty,
                        Password = personalAccessToken
                    }
            };

            Fetch(repo,
                remote.Name,
                branchNames,
                fetchOptions);
        }

        private void Fetch(IRepository repository, string remoteName, string[] branchNames, FetchOptions options = null)
        {
            var repo = repository as Repository;
            if (repo is not null)
            {
                Commands.Fetch(
                repo,
                remoteName,
                branchNames,
                options,
                null);
            }
        }

        private GitHttpClient GetGitClient()
        {
            var orgUrl = new Uri($"https://dev.azure.com/{organization}");
            var credentials = new VssBasicCredential(string.Empty, personalAccessToken);
            var connection = new VssConnection(orgUrl, credentials);
            return connection.GetClient<GitHttpClient>();
        }
    }
}