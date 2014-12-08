(function () {
    app.controller('groupCtrl', ['$rootScope', '$scope', '$http', '$routeParams', '$localStorage', function ($rootScope,$scope, $http, $routeParams, $localStorage) {
        $scope.msg = "Loading..";
        $scope.localStorage = $localStorage;
        $scope.groups = $localStorage.getObject("groups", {});
        
        $rootScope.currentTitle = "All Groups";

        $scope.groupId = $routeParams.id;
        $scope.order = "name";
        $scope.reverse = false;
        
        var query = groupTable;

        $scope.sync = function (redirect) {
            query.read().then(function (groups) {
                $scope.groups = groups;
                $scope.localStorage.setObject("groups", groups);
                $scope.msg = "Synced!";
                $scope.$apply();
                if (redirect != undefined) {
                    window.location.href = redirect;
                }
            });
        }

        if ($scope.groups.length != undefined) {
            $scope.msg = "Loaded locally!";
        } else {
            $scope.sync();
        }
        
        $scope.editGroup = function (group) {
            query.update({ id: group.id, name: group.name, address: group.address })
                .then(function () {
                    console.log("Success!");
                    $scope.sync("#/");
                }, function (err) {
                    console.log(err);
                });
        };

        $scope.deleteGroup = function (group) {
            query.update({ id: group.id, visible: false })  //deleted: true OR 1
                .then(function () {
                    console.log("Deleted Success!");

                    $scope.sync("#/");
                }, function (err) {
                    console.log(err);
                });
        };

        $scope.insertGroup = function (group) {
            query.insert({ name: group.name, address: group.address })
                .then(function () {
                    console.log("Success!");
                    $scope.sync("#/");
                }, function (err) {
                    console.log(err);
                });
        }
    }]);
})();