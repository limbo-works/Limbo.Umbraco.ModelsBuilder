using Limbo.Umbraco.ModelsBuilder.Models;
using System.Collections.Generic;

namespace Limbo.Umbraco.ModelsBuilder.Events {
    
    public class GetModelsEventArgs {

        public IEnumerable<TypeModel> Types { get; set; }

    }

}