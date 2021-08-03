@model AndApp.Models.CarDetails
@{
    ViewBag.Title = "CarDetails";
    Layout = "~/Views/Shared/_Layout.cshtml";

}
<link href="~/assets/select2/select2.css" rel="stylesheet" />
@using (@Html.BeginForm("CarDetails", "PrivateCar", FormMethod.Post))
{
    <div ng-app="myApp">
        <div ng-controller="myCtrl">
            <input id="hdf_step" type="hidden" />
            @Html.HiddenFor(m => m.policytype)
            @Html.HiddenFor(m => m.makeid)
            @Html.HiddenFor(m => m.modelid)
            @Html.HiddenFor(m => m.variantid)
            @Html.HiddenFor(m => m.stateid)
            <div class="row">
                <div class="col-md-12 col-lg-6 align-self-center">
                    <img class="img-fluid vect-img" src="~/assets/images/car-insurance.png" alt="car-insurance">
                </div>
                <div class="col-md-12 col-lg-6">
                    <div id="div_vhinfo" class="fadeIn animated br-box text-center" ng-show="divinfo">
                        <div class="padd">

                            <div class="form-group">
                                <label>Enter the Registration Number</label>
                                <input type="text" id="regno" name="regno" class="form-control fullWidht text-center" ng-model="RegiNo" placeholder="Enter Your Car Number (Example DL1AB1234)" />
                                <p style="color:red;font-style:italic" id="v_regno"></p>
                            </div>
                            <a href="#" ng-click="GetRegistrationNo()" class="btn btn-pink btn-fw mt-4">View Instant Quotes</a>
                        </div>
                        <div class="box-bottom">
                            <span class="or">or</span>
                            <div class="padd">
                                <a ng-click="selectVinfo('Rollover')" class="btn btn-br btn-fw">Proceed without car Number</a>
                                <a ng-click="selectVinfo('New')" class="btn b-link mt-3">New Vehicle</a>
                            </div>
                        </div>
                    </div>

                    <div id="div_state" class="fadeIn animated br-box" ng-show="divstate">
                        <div class="padd">
                            <div class="row-box ins-box-head">
                                <span class="left-arrow" id="btnprev_divstate" ng-click="PrevStep();"><i class="fa fa-angle-left" aria-hidden="true"></i></span>
                                <h5>Select City & RTO</h5>
                                <span class="right-arrow" id="btnnext_divstate" ng-click="NextStep();" style="display:none;"><i class="fa fa-angle-right" aria-hidden="true"></i></span>
                            </div>
                            <div class="form-group search-other">
                                <input id="rtocode" name="rtocode" type="text" ng-model="SearchRto" class="form-control fullWidht search-input" placeholder="Search RTO (E.g. MH02 Or Mumbai)">
                            </div>
                            <div id="accordion" class="acc-city">
                                <div ng-repeat="item in statelist|filter:SearchRto">
                                    <div class="card">
                                        <div class="card-header" id="heading_{{item.stateid}}">
                                            <button class="btn btn-accor collapsed" ng-click="getrto(item.stateid)" data-toggle="collapse" data-target="#collapse_{{item.stateid}}" aria-expanded="true" aria-controls="collapseOne">
    {{item.statename}}
    </button>
</div>

<div id="collapse_{{item.stateid}}" class="collapse" aria-labelledby="heading_{{item.stateid}}" data-parent="#accordion">
    <div class="card-body">

        <ul class="sec-nm" id="divrto_{{item.stateid}}"></ul>
    </div>
</div>
</div>
</div>

</div>
</div>

<div class="box-bottom">
<div class="padd">
<button class="btn btn-br btn-fw" ng-click="SetFocus('rtocode')">Others</button>
</div>
</div>
</div>

<div id="div_make" class="fadeIn animated br-box" ng-show="divmake">
<div class="padd">
<div class="row-box ins-box-head">
<span class="left-arrow" id="btnprev_divmake" ng-click="PrevStep();"><i class="fa fa-angle-left" aria-hidden="true"></i></span>
<h5>Select Car Brand</h5>
<span class="right-arrow" id="btnnext_divmake" ng-click="NextStep();" style="display:none;"><i class="fa fa-angle-right" aria-hidden="true"></i></span>
</div>
<div class="form-group search-other">
@*<select class="form-control fullWidht search-input" id="makeid" placeholder="Search Car Brand"  >
<option ng-model="item.Value" ng-selected="item.selected" ng-value="item.Value"  ng-repeat="item in selectmake">{{item.Text}}</option>
</select>*@

                                @Html.DropDownList("ddlmake", (IEnumerable<SelectListItem>)ViewBag.makelist, "", new { @class = "form-control fullWidht search-input", @placeholder = "Search Car Brand" })
</div>
<div class="row brand-sec">
    <div class="col-sm-3 col-xs-6" ng-repeat="item in makelist">
        <span class="brand make" id="make_{{item.makeid}}">
            <img class="img-fluid" src="~/assets/images/{{item.makename}}.png" alt="{{item.makename}}" ng-click="selectMake(item.makeid,item.makename)">
            <b>{{item.makename}}</b>
        </span>
    </div>
</div>
</div>
<div class="box-bottom">
<div class="padd">
    <button class="btn btn-br btn-fw" ng-click="SetFocus('ddlmake')">Others</button>
</div>
</div>
</div>

<div id="div_model" class="fadeIn animated br-box" ng-show="divmodel">
<div class="padd">
<div class="row-box ins-box-head">
    <span class="left-arrow" id="btnprev_divmodel" ng-click="PrevStep();"><i class="fa fa-angle-left" aria-hidden="true"></i></span>
    <h5>Select Car Model</h5>
    <span class="right-arrow" id="btnnext_divmodel" ng-click="NextStep();" style="display:none;"><i class="fa fa-angle-right" aria-hidden="true"></i></span>
</div>
<div class="form-group search-other">
    <input id="modeltext" type="text" ng-model="SearchModel" class="form-control fullWidht search-input" placeholder="Search Model ">
</div>
<div class="row model-list">
    <div class="col-md-6" ng-repeat="item in modellist|filter:SearchModel"><span class="model-b model" id="model_{{item.modelid}}" ng-click="selectModel(item.modelid,item.modelname)"><b>{{item.modelname}}</b></span></div>
</div>
</div>
<div class="box-bottom">
<div class="padd">
    <button class="btn btn-br btn-fw" ng-click="SetFocus('modeltext')">Others</button>
</div>
</div>
</div>
<div class="fadeIn animated br-box" id="div_FuelDetials" ng-show="divfueltype">
<div class="padd">
<div class="row-box ins-box-head">
    <span class="left-arrow" id="btnprev_divfueltype" ng-click="PrevStep();"><i class="fa fa-angle-left" aria-hidden="true"></i></span>
    <h5>Select Car Fuel Type</h5>
    <span class="right-arrow" id="btnnext_divfueltype" style="display:none;" ng-click="NextStep();"><i class="fa fa-angle-right" aria-hidden="true"></i></span>
</div>
<div class="form-group search-other">
    <input id="fueltype" type="text" name="fueltype" class="form-control fullWidht search-input" )">
</div>
<div class="row justify-content-center model-list">
    <div class="col-md-12"><span class="model-b fuel" ng-click="GetFuel($event,'Diesel')"><b>Diesel</b></span></div>
    <div class="col-md-12"><span class="model-b fuel" ng-click="GetFuel($event,'Petrol')"><b>Petrol</b></span></div>
    <div class="col-md-12"><span class="model-b fuel" ng-click="GetFuel($event,'CNG')"><b>External CNG Kit</b></span></div>
    <div class="col-md-12"><span class="model-b fuel" ng-click="GetFuel($event,'LPG')"><b>External LPG Kit</b></span></div>
</div>
</div>
</div>
<div class="fadeIn animated br-box" id="div_VariantDetails" ng-show="divvariant">
<div class="padd">
<div class="row-box ins-box-head">
    <span class="left-arrow" id="btnprev_divvariant" ng-click="PrevStep();"><i class="fa fa-angle-left" aria-hidden="true"></i></span>
    <h5>Select Car Variant</h5>
    <span class="right-arrow" id="btnnext_divvariant" style="display:none;" ng-click="NextStep();"><i class="fa fa-angle-right" aria-hidden="true"></i></span>
</div>

<div class="form-group search-other">
    <input id="varianttext" type="text" ng-model="SearchVariant" class="form-control fullWidht search-input" placeholder="Search Variant ">
</div>
<div class="row model-list">
    <div class="col-md-6" ng-repeat="item in variantlist|filter:SearchVariant"><span class="model-b variant" ng-click="selectVariant($event,item.variantid,item.variantname)"><b>{{item.variantname}}</b></span></div>

</div>
</div>
<div class="box-bottom">
<div class="padd">
    <button class="btn btn-br btn-fw" ng-click="SetFocus('varianttext')">Others</button>
</div>
</div>
</div>

<div class="fadeIn animated br-box" id="div_manufactureyear" ng-show="divmanufactureyear">
<div class="padd">
<div class="row-box ins-box-head">
    <span class="left-arrow" id="btnprev_divmanufactureyear" ng-click="PrevStep();"><i class="fa fa-angle-left" aria-hidden="true"></i></span>
    <h5>Select Car Registration Year</h5>
    <span class="right-arrow" id="btnnext_divmanufactureyear" style="display:none;" ng-click="NextStep();"><i class="fa fa-angle-right" aria-hidden="true"></i></span>
</div>
<div class="form-group search-other">
    <input id="manufacturingyear" name="manufacturingyear" type="text" ng-model="SearchYear" class="form-control fullWidht search-input" placeholder="Search ManufacturingYear " required="">
</div>
<div class="row justify-content-center year-list">
    <div class="col-md-4" ng-repeat="item in myearlist|filter:SearchYear"><span class="model-b" ng-click="SelectMyear(item.Value)"><b>{{item.Text}}</b></span></div>

</div>
<div class="box-bottom">
    <div class="padd">
        <input type="submit"  ng-click="QualifyCompany()" value="submit">

    </div>
</div>
</div>
</div>
</div>
</div>

</div>
</div>
}
<script src="https://ajax.googleapis.com/ajax/libs/angularjs/1.8.2/angular.min.js"></script>
<script src="~/assets/select2/select2.js"></script>
<script src="https://cdnjs.cloudflare.com/ajax/libs/jquery.mask/1.14.15/jquery.mask.min.js"></script>
<script type="text/javascript">
$(document).ready(function () {
$('#hdf_step').val('0');
$("#makeid").select2({
    minimumInputLength: 0,
});
$('#regno').mask('AA-00-AA-0000');
});

</script>
<script type="text/javascript">

var app = angular.module("myApp", []);
app.controller("myCtrl", function ($scope, $http, $window, $compile) {
    debugger;
    var APIURL = http://localhost:17676/api/PrivateCar/;
$scope.isState = false;
$scope.divinfo = true;
$scope.PolicyType = "Rollover";

$scope.selectVinfo = function (Policy) {
$("#policytype").val(Policy);
$scope.PolicyType = Policy;
$scope.divstate = true;
$scope.divinfo = false;
$('#hdf_step').val('2');
};


if (angular.isUndefined($scope.PolicyType)) {
alert("Please select vehicle type.");
}
$scope.statelist = [];
$scope.makelist = [];
$scope.modellist = [];
$scope.variantlist = [];
$scope.myearlist = [];
$http({
    url: '/PrivateCar/Getstate',
    method: "POST"
})
.then(function (result) {

$scope.statelist = result.data;

})
$http({
    url: '/PrivateCar/GetMake',
    method: "POST"
})
.then(function (result) {
console.log(result);
$scope.makelist = result.data.obj;

})


$scope.getrto = function (stateid) {
$("#stateid").val(stateid);
$('#divrto_' + stateid).html("");
var url = '@Url.Action("GetRto","PrivateCar")';
$.ajax({
    type: "GET",
    url: url,
    data: { stateid: stateid },
    success: function (data) {
var html = "";
$.each(data, function (k) {
html += '<li ><span class="spanrto" rtocode=' + data[k].rtocode + '  ng-click="SelectRTO($event)" >' + data[k].rtocode + ' </span></li>';
});

$htmlstr = $(html).appendTo('#divrto_' + stateid);
$compile($htmlstr)($scope);
}
});

}



$scope.QualifyCompany = function () {
var objQalifyCom = {
"IsBreakin": 1,
"VariantId": 12,
"RtoId": 205
};
$http({
    url: '/PrivateCar/QualifyCom',
    method: "POST",
    data: JSON.stringify(objQalifyCom)
}).then(function (response) {
if (!angular.isUndefined(response && response.data)) {
$scope.QualifyCompanyResData = response.data.data;
angular.forEach($scope.QualifyCompanyResData, function (companylist) {
    $scope.commmm=companylist.comname;
    //this.GetCompanyPlanResult = function () {
    //    return $http({
    //        method: 'POST',
    //        url: APIURL + companylist.comname,
    //        data: JSON.stringify(param)
    //    }).then(function(res)
    //    {
    //        if (!angular.isUndefined(response && response.data)) {
    //            $scope.CompanyResData = response.data.data;
    //    }
    //    );
    //};             

});
}
else {
    //growl.error('Error while fetching state details!!!');
}
}, function (error) {
    //growl.error('error');
});

}

    //$scope.QuoteRequest = {
    //    "PolicyType": $scope.PolicyType,
    //    "BPRtoId": $scope.CarData.RTODetail.Id,
    //    "RTOCityName": $scope.CarData.RTODetail.Name,
    //    "MakeCode": $scope.CarData.Make.Id,
    //    "MakeName": $scope.CarData.Make.Name,
    //    "ModelCode": $scope.CarData.Model.Id,
    //    "ModelName": $scope.CarData.Model.Name,
    //    "VariantCode": $scope.CarData.Variant.VariantId,
    //    "VariantName": $scope.CarData.Variant.VariantName,
    //    "RegistrationYear": $scope.CarData.RegistrationYear.Id,
    //    "BrokerId": $rootScope.BrokerId,
    //    "DontKnowPreviousInsurer": $scope.CarData.DontKnowPreviousInsurer,
    //    "PrevPolicyInsurer": $scope.CarData.PreviousInsurer,
    //    "PrevPolicyExpiryStatus": $scope.CarData.PolicyExpiry,
    //    "VehicleDetails": $scope.VehicleOtherDetails,
    //    "PreviousPolicyDetails": $scope.PreviousPolicyDetails,
    //    "QuotationParams": $scope.QuotationParams,
    //    "PolicyEndDate": $scope.CarData.DontKnowPreviousInsurer ? undefined : $scope.PolicyExpiryDate,
    //    'RegistrationNumber': $scope.RequestObject.RegistrationNumber ? $scope.RequestObject.RegistrationNumber : $scope.VehicleRegistrationNo,
    //    "PreviousPolicyType": $scope.CarData.PreviousPolicyType,
    //    "IsThirdPartyOnly": $scope.CarData.IsThirdPartyOnly,
    //    "IsODOnly": $scope.CarData.IsODOnly,
    //    "NewBusinessPolicyType": $scope.RequestObject.PolicyType == "Renewal" ? "0" : ($scope.CarData.IsThirdPartyOnly && $scope.RequestObject.PolicyType == "New" ? "6" : $scope.ddlBusinessType),
    //    "IsValidLicence": $scope.CarData.CustomerType == "Organization" ? false : $scope.CarData.IsValidLicence,
    //    "IsExistingPACover": $scope.CarData.CustomerType == "Organization" ? false : $scope.CarData.IsExistingPACover,
    //    "DeviceId": $rootScope.DeviceId,
    //    "RequestTime": new Date()
    //};

$scope.SelectRTO = function ($event) {
var code = ($event.target).attributes["rtocode"].value;
$(".spanrto").removeClass('active');
$(event.target).addClass('active');

$('#rtocode').val(code);
$scope.divstate = false;
$scope.divmake = true;
$('#hdf_step').val('3');
};
$scope.selectMake = function (makeid, makename) {
$('.make').removeClass('active');
$("#make_" + makeid).addClass('active');
$("#makeid").val(makeid);
$('#ddlmake').val(makeid);
$('#select2-chosen-1').html(makename);
$('#hdf_step').val('4');
$scope.divmake = false;
$scope.divmodel = true;
GetModel(makeid);

};
$scope.selectModel = function (modelid, modelname) {
debugger;
$('.model').removeClass('active');
$("#model_" + modelid).addClass('active');
$("#modelid").val(modelid);
$('#modeltext').val(modelname);
$scope.divmodel = false;
$scope.divfueltype = true;
$('#hdf_step').val('5');
};
function GetModel(makeid) {
var url = '@Url.Action("GetModel", "PrivateCar")';
$http({
    url: url,
    method: "POST",
    data: { makeid: makeid }
})
.then(function (result) {
console.log(result.data.model);
$scope.modellist = result.data.model;
})

}
$("#ddlmake").change(function () {
var makeid = $('#ddlmake').val();
$("#makeid").val(makeid);
$('.make').removeClass('active');
$("#make_" + makeid).addClass('active');
$('#hdf_step').val('4');
$scope.divmake = false;
$scope.divmodel = true;
GetModel(makeid);

});

$scope.GetFuel = function ($event, fueltype) {
@*'@Model.fueltype' = fueltype;*@
$('#fueltype').val(fueltype);
$('.fuel').removeClass('active');
$(event.target).addClass('active');
$scope.divfueltype = false;
$scope.divvariant = true;
GetVariant(fueltype);
$('#hdf_step').val('6');
}
function GetVariant(fueltype) {
var modelid = $('#modelid').val();
var url = '@Url.Action("GetVariant", "PrivateCar")';
$http({
    url: url,
    method: "POST",
    data: { modelid: modelid, fueltype: fueltype }
})
.then(function (result) {
$scope.variantlist = result.data.variant;
})
}
$scope.selectVariant = function ($event, variantid, variantname) {
$('.variant').removeClass('active');
$(event.target).addClass('active');
$("#variantid").val(variantid);
$('#varianttext').val(variantname);
$scope.divvariant = false;
$scope.divmanufactureyear = true;
GetManufacturingYear();
$('#hdf_step').val('7');
}
function GetManufacturingYear() {
debugger;
if ($('#policytype').val() == 'New') {
var year = new Date().getFullYear();
$('#manufacturingyear').val(year);
}
else {

var url = '@Url.Action("GetManufacturingYear", "PrivateCar")';
$http({
    url: url,
    method: "POST"
})
.then(function (result) {
console.log(result.data);
$scope.myearlist = result.data;
})
}
}
$scope.SelectMyear = function (year) {
$('#manufacturingyear').val(year);
}
$scope.SetFocus = function (element) {

if(element =='makeid')
$('#' + element).select2("open");
else
$('#' + element).focus();
}
$scope.PrevStep = function () {
debugger
var stepval = $('#hdf_step').val();
stepval = stepval - 1;
$('#hdf_step').val(stepval);
if (stepval == 1) {
$scope.divstate = false;
$scope.divmake = false;
$scope.divmodel = false;
$scope.divvariant = false;
$scope.divfueltype = false;
$scope.divmanufactureyear = false;
$scope.divinfo = true;
}
else if (stepval == 2) {
$scope.divinfo = false;
$scope.divmake = false;
$scope.divmodel = false
$scope.divfueltype = false;
$scope.divvariant = false;
$scope.divmanufactureyear = false;
$scope.divstate = true;
$('#btnnext_divstate').show();
}
else if (stepval == 3) {
$scope.divinfo = false;
$scope.divstate = false;
$scope.divmodel = false;
$scope.divfueltype = false;
$scope.divvariant = false;
$scope.divmanufactureyear = false;
$scope.divmake = true;
$('#btnnext_divmake').show();
}
else if (stepval == 4) {
$scope.divinfo = false;
$scope.divstate = false;
$scope.divfueltype = false;
$scope.divmake = false;
$scope.divvariant = false;
$scope.divmanufactureyear = false;
$scope.divmodel = true;
$('#btnnext_divmodel').show();
}
else if (stepval == 5) {
$scope.divinfo = false;
$scope.divstate = false;
$scope.divmake = false;
$scope.divvariant = false;
$scope.divmanufactureyear = false;
$scope.divmodel = false;
$scope.divfueltype = true;
$('#btnnext_divfueltype').show();
}
else if (stepval == 6) {
$scope.divinfo = false;
$scope.divstate = false;
$scope.divmake = false;
$scope.divmodel = false;
$scope.divfueltype = false;
$scope.divmanufactureyear = false;
$scope.divvariant = true;
$('#btnnext_divvariant').show();
}
else if (stepval == 7) {
$scope.divinfo = false;
$scope.divstate = false;
$scope.divmake = false;
$scope.divmodel = false;
$scope.divfueltype = false;
$scope.divvariant = false;
$scope.divmanufactureyear = true;
$('#btnnext_divmanufactureyear').show();
}
}

$scope.NextStep = function () {
debugger
var stepval = $('#hdf_step').val();
stepval = parseInt(stepval) + 1;

$('#hdf_step').val(stepval);
if (stepval == 1) {
$scope.divstate = false;
$scope.divmake = false;
$scope.divmodel = false;
$scope.divvariant = false;
$scope.divfueltype = false;
$scope.divmanufactureyear = false;
$scope.divinfo = true;
}
else if (stepval == 2) {
$scope.divmake = false;
$scope.divmodel = false;
$scope.divvariant = false;
$scope.divmanufactureyear = false;
$scope.divfueltype = false;
$scope.divinfo = false;
$scope.divstate = true;
}
else if (stepval == 3) {
$scope.divmodel = false;
$scope.divvariant = false;
$scope.divmanufactureyear = false;
$scope.divfueltype = false;
$scope.divinfo = false;
$scope.divstate = false;
$scope.divmake = true;
}
else if (stepval == 4) {
$scope.divvariant = false;
$scope.divmanufactureyear = false;
$scope.divfueltype = false;
$scope.divinfo = false;
$scope.divstate = false;
$scope.divmake = false;
$scope.divmodel = true;
}
else if (stepval == 5) {
$scope.divvariant = false;
$scope.divmanufactureyear = false;
$scope.divinfo = false;
$scope.divstate = false;
$scope.divmake = false;
$scope.divmodel = false;
$scope.divfueltype = true;
}
else if (stepval == 6) {
$scope.divinfo = false;
$scope.divstate = false;
$scope.divmake = false;
$scope.divmodel = false;
$scope.divfueltype = false;
$scope.divmanufactureyear = false;
$scope.divvariant = true;
}
else if (stepval == 7) {
$scope.divinfo = false;
$scope.divstate = false;
$scope.divmake = false;
$scope.divmodel = false;
$scope.divfueltype = false;
$scope.divvariant = false;
$scope.divmanufactureyear = true;
}
}
$scope.GetRegistrationNo = function () {
if (!($('#regno').val())) {
$('#v_regno').text("Please Input Required Details!");
}
else {
$('#v_regno').text("");
$("#policytype").val("Renewal");
$scope.divstate = true;
$scope.divinfo = false;
$('#hdf_step').val('2');
}
}
});
</script>


