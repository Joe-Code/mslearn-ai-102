using System;
using Azure.Search.Documents.Indexes;
using Azure.Search.Documents.Indexes.Models;

namespace MargiesTravel.Models
{
    public partial class SearchResult
    {
        [SearchableField(IsFilterable=true)]
        public string url { get; set; } = string.Empty;

        [SearchableField()]
        public string merged_content { get; set; } = string.Empty;

        [SearchableField(IsFilterable=true, IsSortable=true)]
        public string metadata_storage_name { get; set; } = string.Empty;

        [SearchableField(IsFilterable=true, IsSortable=true, IsFacetable=true)]
        public string metadata_author { get; set; } = string.Empty;

        [SearchableField(IsFilterable=true, IsSortable=true)]
        public int metadata_storage_size { get; set; }

        [SearchableField(IsFilterable=true, IsSortable=true)]
        public DateTime metadata_storage_last_modified { get; set; }

        [SimpleField(IsFilterable=true, IsSortable=true)]
        public string sentiment { get; set; } = string.Empty;

        [SearchableField(IsFilterable=true)]
        public string language { get; set; } = string.Empty;

        [SearchableField(IsFilterable=true)]
        public string[] locations { get; set; } = new string[0];

        [SearchableField()]
        public string[] keyphrases { get; set; } = new string[0];

        [SearchableField()]
        public string[] imageTags { get; set; } = new string[0];

        [SearchableField()]
        public string[] imageCaption { get; set; } = new string[0];
    }

}
