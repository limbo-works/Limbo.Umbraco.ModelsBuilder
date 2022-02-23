angular.module("umbraco").controller("Limbo.Umbraco.ModelsBuilder.Dashboard.Controller", function ($http, $q, $timeout) {

    const vm = this;

    vm.update = function () {

        vm.loading = true;
        vm.reloadButtonState = "busy";

        $q.all([
            $timeout(250),
            $http.get(Umbraco.Sys.ServerVariables.umbracoSettings.umbracoPath + "/backoffice/Limbo/ModelsBuilder/GetStatus")
        ]).then(function (data) {
            vm.loading = false;
            vm.status = data[1].data;
            vm.reloadButtonState = "init";
        });

    };

    vm.reload = function () {
        vm.reloadButtonState = "busy";
        vm.update();
    };

    vm.generate = function() {

        vm.loading = true;
        vm.generateButtonState = "busy";

        $q.all([
            $timeout(250),
            $http.get(Umbraco.Sys.ServerVariables.umbracoSettings.umbracoPath + "/backoffice/Limbo/ModelsBuilder/GenerateModels")
        ]).then(function () {
            vm.loading = false;
            vm.generateButtonState = "init";
        });

    };

    vm.update();

});