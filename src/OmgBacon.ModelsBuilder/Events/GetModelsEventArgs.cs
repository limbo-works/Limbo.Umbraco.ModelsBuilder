using System.Collections.Generic;
using OmgBacon.ModelsBuilder.Models;

namespace OmgBacon.ModelsBuilder.Events {
    
    public class GetModelsEventArgs {

        public IEnumerable<TypeModel> Types { get; set; }

    }

}