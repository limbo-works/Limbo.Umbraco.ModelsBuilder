using Newtonsoft.Json;

namespace Limbo.Umbraco.ModelsBuilder.Models.Json {

    public class JsonNetPropertySettings {

        public bool Ignore { get; set; }

        public string PropertyName { get; set; }

        public NullValueHandling NullValueHandling { get; set; }

        public DefaultValueHandling DefaultValueHandling { get; set; }

        public int? Order { get; set; }

    }

}