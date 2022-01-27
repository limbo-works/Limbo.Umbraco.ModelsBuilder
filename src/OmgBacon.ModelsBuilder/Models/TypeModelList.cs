using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace OmgBacon.ModelsBuilder.Models {
    
    public class TypeModelList : IEnumerable<TypeModel> {

        private readonly List<TypeModel> _list;

        private readonly IDictionary<string, TypeModel> _dictionary;

        public TypeModelList(IEnumerable<TypeModel> models) {
            _list = models.ToList();
            _dictionary = _list.ToDictionary(x => x.Alias);
        }

        public bool TryGetModel(string alias, out TypeModel model) {
            return _dictionary.TryGetValue(alias, out model);
        }

        public IEnumerator<TypeModel> GetEnumerator() {
            return _list.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return GetEnumerator();
        }

    }

}