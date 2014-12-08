var groupTable;
var contactTable;
var client;
(function () {

    $ = jQuery;

    // This MobileServiceClient has been configured to communicate with your local
    // test project for debugging purposes.
    //var client = new WindowsAzure.MobileServiceClient(
    //    "http://localhost:50001"
    //);

    // This MobileServiceClient has been configured to communicate with your Mobile Service's url
    // and application key. You're all set to start working with your Mobile Service!
    //var client = new WindowsAzure.MobileServiceClient(
    //    "https://myphonebook.azure-mobile.net/",
    //    "adDmAlviaMitJGsPSayMovXJMsbmbY29"
    //);

    client = new WindowsAzure.MobileServiceClient(
        "http://mpbdm.azure-mobile.net/",
        "aQJqIIVmcCiyknYeQCeQkJyYEUfUWE63"
    );


    var accountTable = client.getTable('accounts');

    groupTable = client.getTable('Groups');
    contactTable = client.getTable('Contacts');

    function calculaScreenHeight() {
        var screen_length = $(window).height();
        $('#dashboard-left').css('min-height', screen_length - 350);

    }

    window.onresize = function () {
        calculaScreenHeight();
    }

    calculaScreenHeight();

})();