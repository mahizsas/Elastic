(function () {
    app.controller('contactCtrl', ['$scope', '$http', '$routeParams', '$localStorage', function ($scope, $http, $routeParams, $localStorage) {

        //$scope.msg = "Loading Contacts...";
        $scope.localStorage = $localStorage;
        $scope.contacts = $localStorage.getObject("contacts", {});
               
        var query = contactTable;

        $scope.myGroupId = $routeParams.id;
        

        if ($scope.contacts.length != undefined) {
            $scope.msg = "Contacts Loaded locally!";
        }
        else {
            query.read().then(function (contacts) {
                $scope.contacts = contacts;
                $scope.localStorage.setObject("contacts", contacts);
                $scope.msg = "Contacts Loaded!";
            });
        }

        $scope.sync = function (redirect) {
            query.read().then(function (contacts) {
                $scope.contacts = contacts;
                $scope.localStorage.setObject("contacts", contacts);
                $scope.msg = "Synchronized!";

                $scope.$apply();

                if (redirect != undefined) {
                    window.location.href = '/#/contacts';
                }
            });
        }

        $scope.editContact = function (contact) {
            query.update({ id: contact.id, firstName: contact.firstName, lastName: contact.lastName, email: contact.email, phone: contact.phone })
                .then(function () {
                    console.log('Contact successfully edited!');
                    $scope.sync("#/contacts");

                },
                function (error) {
                    console.log(error);
                }
            );
        }

        $scope.deleteContact = function (contact) {
            query.update({ id: contact.id, deleted: true})
                .then(function () {
                    console.log('Contact Deleted');
                    $scope.sync("#/contacts");

                },
                function (error) {
                    console.log(error);
                }
            );
        }

        $scope.insertContact = function (contact) {
            query.insert({ groupID: contact.groupId, firstName: contact.firstName, lastName: contact.lastName, email: contact.email, phone: contact.phone , visible : true})
                .then(function () {
                    console.log('Contact Inserted!');
                    $scope.sync("#/contacts");
                },
                function (error) {
                    console.log(error);
                }
            );
        }
     
  
    }]);
})();