﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Elasticsearch.Net;

namespace Nest
{
	using GetAliasesConverter = Func<IElasticsearchResponse, Stream, GetAliasesResponse>;
	using CrazyAliasesResponse = Dictionary<string, Dictionary<string, Dictionary<string, AliasDefinition>>>;

	public partial class ElasticClient
	{
		/// <inheritdoc />
		public IIndicesOperationResponse Alias(Func<AliasDescriptor, AliasDescriptor> aliasSelector)
		{
			return this.Dispatch<AliasDescriptor, AliasRequestParameters, IndicesOperationResponse>(
				aliasSelector,
				(p, d) => this.RawDispatch.IndicesUpdateAliasesDispatch<IndicesOperationResponse>(p, d)
			);
		}

		/// <inheritdoc />
		public Task<IIndicesOperationResponse> AliasAsync(Func<AliasDescriptor, AliasDescriptor> aliasSelector)
		{
			return this.DispatchAsync<AliasDescriptor, AliasRequestParameters, IndicesOperationResponse, IIndicesOperationResponse>(
				aliasSelector,
				(p, d) => this.RawDispatch.IndicesUpdateAliasesDispatchAsync<IndicesOperationResponse>(p, d)
			);
		}

		/// <inheritdoc />
		public IGetAliasesResponse GetAliases(Func<GetAliasesDescriptor, GetAliasesDescriptor> getAliasesDescriptor)
		{
			return this.Dispatch<GetAliasesDescriptor, GetAliasesRequestParameters, GetAliasesResponse>(
				getAliasesDescriptor,
				(p, d) => this.RawDispatch.IndicesGetAliasDispatch<GetAliasesResponse>(
					p.DeserializationState(new GetAliasesConverter(DeserializeGetAliasesResponse))
				)
			);
		}

		/// <inheritdoc />
		public Task<IGetAliasesResponse> GetAliasesAsync(Func<GetAliasesDescriptor, GetAliasesDescriptor> getAliasesDescriptor)
		{
			return this.DispatchAsync<GetAliasesDescriptor, GetAliasesRequestParameters, GetAliasesResponse, IGetAliasesResponse>(
				getAliasesDescriptor,
				(p, d) => this.RawDispatch.IndicesGetAliasDispatchAsync<GetAliasesResponse>(
					p.DeserializationState(new GetAliasesConverter(DeserializeGetAliasesResponse))
				)
			);
		}
		
		/// <inheritdoc />
		private GetAliasesResponse DeserializeGetAliasesResponse(IElasticsearchResponse connectionStatus, Stream stream)
		{
			if (!connectionStatus.Success)
				return new GetAliasesResponse() { IsValid = false};

			var dict = this.Serializer.Deserialize<CrazyAliasesResponse>(stream);

			var d = new Dictionary<string, IList<AliasDefinition>>();

			foreach (var kv in dict)
			{
				var indexDict = kv.Key;
				var aliases = new List<AliasDefinition>();
				if (kv.Value != null && kv.Value.ContainsKey("aliases"))
				{
					var aliasDict = kv.Value["aliases"];
					if (aliasDict != null)
						aliases = aliasDict.Select(kva =>
						{
							var alias = kva.Value;
							alias.Name = kva.Key;
							return alias;
						}).ToList();
				}

				d.Add(indexDict, aliases);
			}

			return new GetAliasesResponse()
			{
				IsValid = true,
				Indices = d
			};
		}
	}
}