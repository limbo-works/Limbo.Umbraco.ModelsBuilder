﻿using System.Collections.Generic;
using System.Linq;
using Limbo.Umbraco.ModelsBuilder.Models;

#pragma warning disable 1591

namespace Limbo.Umbraco.ModelsBuilder.Containers;

public class ContainerModel {

    public string Directory { get; }

    public TypeModel[] Types { get; }

    public ContainerModel(string directory, IEnumerable<TypeModel> types) {
        Directory = directory;
        Types = types.ToArray();
    }

}