using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;
using Unity.Services.Core.Editor;
using Unity.Services.DeploymentApi.Editor;
using Unity.Services.Leaderboards.Authoring.Client;
using Unity.Services.Leaderboards.Authoring.Client.Apis.Default;
using Unity.Services.Leaderboards.Authoring.Client.Default;
using Unity.Services.Leaderboards.Authoring.Client.Http;
using Unity.Services.Leaderboards.Authoring.Client.Models;
using Unity.Services.Leaderboards.Authoring.Core.Deploy;
using Unity.Services.Leaderboards.Clients;
using Unity.Services.Leaderboards.Authoring.Core.Model;
using Unity.Services.Leaderboards.Authoring.Core.Service;
using UnityEditor;
using UnityEngine;
using Logger = Unity.Services.Leaderboards.Logging.Logger;

namespace Unity.Services.Leaderboards.Editor.Authoring.AdminApi
{
    class LeaderboardsClient : ILeaderboardsClient
    {
        readonly object m_TokenUpdateLock;
        readonly ObservableCollection<IDeploymentItem> m_Assets;
        readonly IAccessTokens m_AccessTokens;
        AdminApiHeaders<LeaderboardsClient> m_Headers;
        IDefaultApiClient m_Client;
        string projectId => CloudProjectSettings.projectId;
        string environmentId => Deployments.Instance.EnvironmentProvider.Current;
        readonly IEnvironmentProvider m_EnvironmentProvider;
        const int k_PaginationLimit = 100;

        public LeaderboardsClient(
            ObservableCollection<IDeploymentItem> assets,
            IAccessTokens accessTokens,
            IDefaultApiClient client
        )
        {
            m_Assets = assets;
            m_AccessTokens = accessTokens;
            m_Client = client;
            m_TokenUpdateLock = new object();
        }

        public async void Initialize(
            string environmentId,
            string projectId,
            CancellationToken cancellationToken)
        {
            try
            {
                await UpdateToken();
            }
            catch (Exception e)
            {
                Logger.LogException(e);
            }
        }

        async Task UpdateToken()
        {
            var client = m_Client as DefaultApiClient;
            if (client == null)
                return;

            string token = await m_AccessTokens.GetServicesGatewayTokenAsync();
            var headers = new AdminApiHeaders<LeaderboardsClient>(token);
            lock (m_TokenUpdateLock)
            {
                client.Configuration = new Configuration(
                    null,
                    null,
                    null,
                    headers.ToDictionary());
            }
        }

        public async Task<ILeaderboardConfig> Get(string name, CancellationToken _)
        {
            await UpdateToken();

            var request = new GetLeaderboardConfigRequest(
                Guid.Parse(projectId),
                Guid.Parse(environmentId),
                name
            );
            var result = await WrapException(m_Client.GetLeaderboardConfigAsync(request));
            return LeaderboardsConfigUtil.UpdatedLeaderboardConfig1ToLeaderboardConfig(result.Result);
        }

        public async Task Update(ILeaderboardConfig resource, CancellationToken _)
        {
            await UpdateToken();

            var cfg = LeaderboardsConfigUtil.ToLeaderboardConfig1(resource);

            var request = new UpdateLeaderboardConfigRequest(
                Guid.Parse(projectId),
                Guid.Parse(environmentId),
                resource.Id,
                cfg
            );
            await WrapException(m_Client.UpdateLeaderboardConfigAsync(request));
        }

        public async Task Create(ILeaderboardConfig resource, CancellationToken _)
        {
            await UpdateToken();

            var cfg = LeaderboardsConfigUtil.ToLeaderboardIdConfig1(resource);
            var request = new CreateLeaderboardRequest(
                Guid.Parse(projectId),
                Guid.Parse(environmentId),
                cfg
            );
            await WrapException(m_Client.CreateLeaderboardAsync(request));
        }

        public async Task Delete(ILeaderboardConfig resource, CancellationToken _)
        {
            await UpdateToken();

            var request = new DeleteLeaderboardRequest(
                Guid.Parse(projectId),
                Guid.Parse(environmentId),
                resource.Id
            );
            await WrapException(m_Client.DeleteLeaderboardAsync(request));
        }

        internal static async Task<T> WrapException<T>(Task<T> query)
        {
            T res;
            try
            {
                res = await query;
            }
            catch (HttpException<BasicErrorResponse1> e)
            {
                var msg = $"[{e.ActualError.Title}] The following errors were encountered: \n\t* {string.Join("\n\t* ", e.ActualError.Details.ConvertAll(d => d.GetAsString()))}";
                throw new LeaderboardsDeploymentHandler.ApiException(msg, e);
            }
            catch (HttpException<BasicErrorResponse> e)
            {
                var msg = $"[{e.ActualError.Title}] The following errors were encountered: \n\t* {string.Join("\n\t* ", e.ActualError.Details.ConvertAll(d => d.GetAsString()))}";
                throw new LeaderboardsDeploymentHandler.ApiException(msg, e);
            }
            catch (HttpException<ValidationErrorResponse1> e)
            {
                List<string> errors = new List<string>();
                e.ActualError.Errors.ForEach(err => err.Messages.ForEach(msg => errors.Add(msg)));
                var msg = $"[{e.ActualError.Title}]: The following errors were encountered during validation: \n\t* {string.Join("\n\t* ", errors.ToArray())}";
                throw new LeaderboardsDeploymentHandler.ApiException(msg, e);
            }
            catch (HttpException<ValidationErrorResponse> e)
            {
                List<string> errors = new List<string>();
                e.ActualError.Errors.ForEach(err => err.Messages.ForEach(msg => errors.Add(msg)));
                var msg = $"[{e.ActualError.Title}]: The following errors were encountered during validation: \n\t* {string.Join("\n\t* ", errors.ToArray())}";
                throw new LeaderboardsDeploymentHandler.ApiException(msg, e);
            }
            catch (HttpException httpException)
            {
                throw new LeaderboardsDeploymentHandler.ApiException(httpException.Response.ErrorMessage, httpException);
            }

            return res;
        }

        public async Task<IReadOnlyList<ILeaderboardConfig>> List(CancellationToken cancellationToken)
        {
            await UpdateToken();
            string cursor = null;
            var results = new List<ILeaderboardConfig>();
            int resultsCount;

            do
            {
                cancellationToken.ThrowIfCancellationRequested();
                var request = new GetLeaderboardConfigsRequest(
                    Guid.Parse(projectId),
                    Guid.Parse(environmentId),
                    cursor,
                    k_PaginationLimit
                );
                var response = await m_Client.GetLeaderboardConfigsAsync(request);
                resultsCount = response.Result.Results.Count;
                results.AddRange(response.Result.Results
                    .ConvertAll<ILeaderboardConfig>(LeaderboardsConfigUtil.UpdatedLeaderboardConfig1ToLeaderboardConfig));
                cursor = response.Result.PageInfo.EndCursor;
            }
            while (resultsCount == k_PaginationLimit);

            return results;
        }

        public async Task Reset(ILeaderboardConfig leaderboardConfig)
        {
            await UpdateToken();
            var request = new ResetLeaderboardScoresRequest(
                Guid.Parse(projectId),
                Guid.Parse(environmentId),
                leaderboardConfig.Id);
            await m_Client.ResetLeaderboardScoresAsync(request);
        }
    }
}
