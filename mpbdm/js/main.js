var app;
(function () {
    app = angular.module("phonebook", ['ngResource', 'ngRoute'])
    .factory('$localStorage', ['$window', function ($window) {
        return {
            set: function (key, value) {
                $window.localStorage[key] = value;
            },
            get: function (key, defaultValue) {
                return $window.localStorage[key] || defaultValue;
            },
            setObject: function (key, value) {
                $window.localStorage[key] = JSON.stringify(value);
            },
            getObject: function (key) {
                return JSON.parse($window.localStorage[key] || '{}');
            }
        }
    }])
    .config(['$routeProvider', function ($routeProvider) {
        $routeProvider
            .when('/', {
                templateUrl: '../pages/group/groups.html',
                controller: 'groupCtrl'
            })
            .when('/group/:id/contacts', {
                templateUrl: '../pages/contact/contacts.html',
                controller: 'contactCtrl'
            })
            .when('/group/insert', {
                templateUrl: '../pages/group/group-insert.html',
                controller: 'groupCtrl'
            })
            .when('/group/:id', {
                templateUrl: '../pages/group/group-details.html',
                controller: 'groupCtrl'
            })
            .when('/contacts', {
                templateUrl: '../pages/contact/contacts.html',
                controller: 'contactCtrl'
            })
            .when('/contacts/insert', {
                templateUrl: '../pages/contact/contact-insert.html',
                controller: 'contactCtrl'
            })
            .when('/contacts/:id', {
                templateUrl: '../pages/contact/contact-details.html',
                controller: 'contactCtrl'
            })
            .when('/import', {
                templateUrl: '../pages/import/import.html',
                controller: 'importExcelCtrl'
            })
            .otherwise({
                redirectTo: '/'
            });

    }])
    .directive('account', function () {
        return {
            restrict: 'E',
            templateUrl: '../pages/account/account-top.html',
            controller: 'accountCtrl'
        };
    })
    .directive('fileModel', ['$parse', function ($parse) {
        return {
            restrict: 'A',
            link: function (scope, element, attrs) {
                var model = $parse(attrs.fileModel);
                var modelSetter = model.assign;

                element.bind('change', function () {
                    scope.$apply(function () {
                        modelSetter(scope, element[0].files[0]);
                    });
                });
            }
        };
    }])
    .service('fileUpload', ['$http', function ($http) {
        this.uploadFileToUrl = function (file, uploadUrl) {
            var fd = new FormData();
            fd.append('file', file);
            $http.post(uploadUrl, fd, {
                transformRequest: angular.identity,
                headers: { 'Content-Type': undefined }
            })
            .success(function () {
            })
            .error(function () {
            });
        }
    }]);


})();