(function () {
	app.controller('accountCtrl', ['$scope', '$http', '$routeParams', '$localStorage', function ($scope, $http, $routeParams, $localStorage) {
		$scope.msg = "Account Ctrl...";
		$scope.localStorage = $localStorage;
		$scope.query = client.getTable('Users');

		$scope.firstName = "";
		$scope.lastName = "";

		client.currentUser = JSON.parse(window.localStorage['auth'] || null);

		$scope.isLoggedIn = function () {
			return client.currentUser !== null;
		}

		$scope.refreshAuthDisplay  = function() {
		    var isLoggedIn = $scope.isLoggedIn();
			window.localStorage['auth'] = JSON.stringify(client.currentUser);
			if (isLoggedIn) {
			    $scope.query.lookup(client.currentUser.userId).then(function (resp) {
			        //console.log("DATA:");
			        //console.log(resp);
			        $scope.firstName = resp.firstName;
			        $scope.lastName = resp.lastName;
			        $scope.$digest();
			    }, function (err) {
			        console.log("Error:");
			        console.log(err);
			    });
			}
		}
		$scope.logIn = function () {
			client.login("facebook").then($scope.refreshAuthDisplay, function (error) {
				alert(error);
			});
		}
		$scope.logOut = function () {
		    client.logout();

		    // remove localStorage values when logout
		    window.localStorage.removeItem('auth');
		    window.localStorage.removeItem('groups');
		    window.localStorage.removeItem('contacts');

			$scope.refreshAuthDisplay();
		}
		$scope.refreshAuthDisplay();
	}]);
})();