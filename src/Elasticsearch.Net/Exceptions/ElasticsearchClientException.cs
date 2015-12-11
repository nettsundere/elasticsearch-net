﻿using Elasticsearch.Net.Connection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Elasticsearch.Net
{
	// TODO come up with a better name for this?
	public class ElasticsearchClientException : Exception
	{
		public IApiCallDetails Response { get; internal set; }

		public List<Audit> AuditTrail { get; internal set; }

		public ElasticsearchClientException(string message, Exception innerException)
			: base(message, innerException) { }
	}
}
