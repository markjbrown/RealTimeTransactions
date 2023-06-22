﻿using CorePayments.Infrastructure.Domain.Entities;
using Microsoft.Azure.Cosmos;
using Newtonsoft.Json.Linq;

namespace CorePayments.Infrastructure.Repository
{
    public class MemberRepository : CosmosDbRepository, IMemberRepository
    {
        public MemberRepository(CosmosClient client) :
            base(client, containerName: Environment.GetEnvironmentVariable("memberContainer") ?? string.Empty)
        {
        }

        public async Task CreateItem(Member member)
        {
            await Container.CreateItemAsync(member);
        }

        public async Task<(IEnumerable<Member>? members, string? continuationToken)> GetPagedMembers(int pageSize, string continuationToken)
        {
            QueryDefinition query = new QueryDefinition("select * from c order by c.lastName desc");

            return await PagedQuery<Member>(query, pageSize, null, continuationToken);
        }

        public async Task<int> PatchMember(Member member, string memberId)
        {
            JObject obj = JObject.FromObject(member);

            var ops = new List<PatchOperation>();

            foreach (JToken item in obj.Values())
            {
                if (item.Path == "id" || item.Path == "memberId" || string.IsNullOrEmpty(item.ToString()))
                    continue;

                ops.Add(PatchOperation.Add($"/{item.Path}", item.ToString()));
            }

            if (ops.Count == 0)
                return 0;

            await Container.PatchItemAsync<Member>(memberId, new PartitionKey(memberId), ops);

            return ops.Count;
        }
    }
}