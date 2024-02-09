using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace Limbo.Umbraco.ModelsBuilder.Models;

/// <summary>
/// Class representing a list of <see cref="TypeModel"/>.
/// </summary>
public class TypeModelList : IEnumerable<TypeModel> {

    private readonly List<TypeModel> _list;

    private readonly IDictionary<string, TypeModel> _dictionary;

    /// <summary>
    /// Returns the amount of models in the list.
    /// </summary>
    public int Count => _list.Count;

    /// <summary>
    /// Initializes a new list based on the specified <paramref name="models"/>.
    /// </summary>
    /// <param name="models">The models that should make up the list.</param>
    public TypeModelList(IEnumerable<TypeModel> models) {
        _list = models.ToList();
        _dictionary = _list.ToDictionary(x => x.Alias);
    }

    /// <summary>
    /// Gets the model with the specified <paramref name="alias"/>.
    /// </summary>
    /// <param name="alias">The alias of the model.</param>
    /// <param name="model">When this method returns, contains the model with the specified <paramref name="alias"/>, if the model is found; otherwise, <c>null</c>. This parameter is passed uninitialized.</param>
    /// <returns><c>true</c> if the <see cref="TypeModelList"/> contains an element with the specified key; otherwise, <c>false</c>.</returns>
    public bool TryGetModel(string alias, [NotNullWhen(true)] out TypeModel? model) {
        return _dictionary.TryGetValue(alias, out model);
    }

    /// <inheritdoc />
    public IEnumerator<TypeModel> GetEnumerator() {
        return _list.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator() {
        return GetEnumerator();
    }

}