angular.module("umbraco").controller("Limbo.Umbraco.ModelsBuilder.Dashboard.Controller", function ($http, $q, $timeout) {

    const vm = this;

    vm.update = function (success) {

        vm.loading = true;

        $q.all([
            $timeout(250),
            $http.get(Umbraco.Sys.ServerVariables.umbracoSettings.umbracoPath + "/backoffice/Limbo/ModelsBuilder/GetStatus")
        ]).then(function (data) {
            vm.loading = false;
            vm.reloadButtonState = "init";
            vm.status = data[1].data;
            if (vm.status.lastBuildDate) vm.status.lastBuildDateFrom = moment(vm.status.lastBuildDate).locale("en").fromNow();
            if (success) success();
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

            // Depending on a few different factors (eg. how the website is hosted), the response
            // might not include an updated models status, so we need to make a another request to
            // get the status
            vm.update(function () {
                vm.generateButtonState = "init";
            });

        });

    };

    vm.update();

});