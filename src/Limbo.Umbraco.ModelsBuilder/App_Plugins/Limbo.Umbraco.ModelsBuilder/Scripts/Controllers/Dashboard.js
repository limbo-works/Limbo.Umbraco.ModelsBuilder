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
            vm.reloadButtonState = "init";
            vm.status = data[1].data;
            if (vm.status.lastBuildDate) vm.status.lastBuildDateFrom = moment(vm.status.lastBuildDate).locale("en").fromNow();
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
            vm.status = data[1].data;
            if (vm.status.lastBuildDate) vm.status.lastBuildDateFrom = moment(vm.status.lastBuildDate).locale("en").fromNow();
        });

    };

    vm.update();

});