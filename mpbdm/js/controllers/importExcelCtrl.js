(function () {
    app.controller('importExcelCtrl', ['$scope', '$http', '$routeParams', '$localStorage', 'fileUpload', function ($scope, $http, $routeParams, $localStorage, fileUpload) {

        console.log('into the importCtrl');
       
        $scope.uploadFile = function () {
            var file = $scope.myFile;
           
            console.log('File extension: ' + file.name.split('.').pop());
            console.log('File is ' + JSON.stringify(file));

            var uploadUrl = "/fileUpload";
            fileUpload.uploadFileToUrl(file, uploadUrl);





            //$scope.excel_toJson = function (workbook) {
            //    var result = {};
            //    workbook.SheetNames.forEach(function (sheetName) {
            //        var roa = XLS.utils.sheet_to_row_object_array(workbook.Sheets[sheetName]);
            //        if (roa.length > 0) {
            //            result[sheetName] = roa;
            //        }
            //    });
            //    return result;
            //}

            //if (file.name.split('.').pop() != ('xls' || 'xlsx')) {
            //    console.log('Something gonne wrong! Current type of file is not supported');
            //}
            //else {
            //    var returnJson = $scope.excel_toJson(file);
            //    console.log(returnJson);
            //}

            
        };


    }]);
})();

