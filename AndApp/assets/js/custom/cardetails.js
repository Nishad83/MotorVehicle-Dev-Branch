 
    $(document).ready(function () {
        $('#hdf_step').val('0');
        $("#ddlmake").select2({
            minimumInputLength: 0,
        });
        $('#regno').mask('AA-00-AA-0000');
        $('#enqno').mask('ENQ0000000000');
    });

 

    var app = angular.module("myApp", ["ngStorage"]);
app.controller("myCtrl", function ($scope, $http, $window, $compile, $sessionStorage) {
    delete $sessionStorage.FirstQuote;
    delete $sessionStorage.PCQuotation;
    delete $sessionStorage.PCQuotationResPonse;
    $scope.isState = false;
    $scope.divinfo = true;
    //$scope.PolicyType = "Rollover";
    //var APIURL = 'http://localhost:17676/api/PrivateCar/';


         

    $scope.selectenqinfo = function (enq) {
         
        if (enq == "Back") {
            $scope.divinfo = true;
            $scope.divenq = false;
               
        }
        else {
                 
            var enqno = $('#enqno').val();
               
            if (enqno == '') {
                $('#v_enqno').text("Please Enter Enquiry No.");
                return false;
            }
            else if (enqno.length!=13) {
                $('#v_enqno').text("Enquiry No Is Not Valid");
                return false;
            }
            else {
                $('#v_enqno').text("");
            }
               
                
            $http({
                url: '/PrivateCar/GetQuoteByEnquiryNo',
                method: "POST",
                data: { enqno:enqno}
            }).then(function (response) {
                alert("ok");

                if (!angular.isUndefined(response && response.data)) {


                    $scope.ProgressTotal = JSON.parse(response.data.enquiryrequest).length;
                    $scope.ProgressCount = 0;


                    $sessionStorage.PCQuotationResPonse = angular.copy(JSON.parse(response.data.enquiryresponse));
                    window.location.href = '/Motor/PrivateCar/PolicyDetails';
                    //return false;
                    //angular.forEach(JSON.parse(response.data.enquiryrequest), function (companywisequote) {
                    //    console.log(companywisequote)

                    //    $scope.QuoteResponse = [];
                    //    var newQuote = {};



                    //    $http({
                    //        url: '/PrivateCar/GetQuoteCompanyWise',
                    //        method: "POST",
                    //        data: JSON.stringify(companywisequote)
                    //    }).then(function (response) {
                    //        if (!angular.isUndefined(response && response.data.length > 0)) {
                    //            $scope.ProgressCount++;
                    //            $scope.CompanyResData = response.data;

                    //            if (response.data.Status === 1) {
                    //                $scope.QuoteResponse.push(response.data);
                    //            }

                    //            //   console.log($scope.CompanyResData);

                    //            if ($scope.ProgressTotal == $scope.ProgressCount) {

                    //                $sessionStorage.PCQuotation = angular.copy($scope.QuoteReqModel);
                    //                $sessionStorage.PCQuotationResPonse = angular.copy($scope.QuoteResponse);
                    //                window.location.href = '/Motor/PrivateCar/PolicyDetails';
                    //            }
                    //        }
                    //        else {
                    //        }
                    //    }, function (error) {
                    //    });

                    //    //}

                    //});//f
                }
                else {
                    //growl.error('Error while fetching state details!!!');
                }

            }, function (error) {
            });
        }
    };

    $scope.selectVinfo = function (Policy) {
        $("#policytype").val(Policy);
        $scope.PolicyType = Policy;
        $scope.divstate = true;
        $scope.divinfo = false;
        $('#hdf_step').val('2');
    };

    $scope.enqnobtn = function () {
        $scope.divinfo = false;

        $scope.divenq = true;
            
         
           
    };
      

    //if (angular.isUndefined($scope.PolicyType)) {
    //    alert("Please select vehicle type.");
    //}
    $scope.statelist = [];
    $scope.makelist = [];
    $scope.citylist = [];
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

            $scope.makelist = result.data;

        })
    $scope.selectState = function (stateid, statename) {

        $('.state').removeClass('active');
        $("#state_" + stateid).addClass('active');
        $("#stateid").val(stateid);
        $('#statetext').val(statename);
        $('#hdf_step').val('3');
        FirstQuote.StateName = statename;
        $scope.divstate = false;
        $scope.divcity = true;
        GetCity(stateid);
    }
    function GetCity(stateid) {

        var url = '@Url.Action("GetRtoCity", "PrivateCar")';
        $http({
            url: url,
            method: "POST",
            data: { stateid: stateid },
        })
        .then(function (data) {
            //  console.log(data.data);
            $scope.citylist = data.data;
        })

    }
    $scope.getrto = function (rtodesc,id) {
        var stateid = $("#stateid").val();
        $("#cityname").val(rtodesc);
        $('#divrto_' + stateid+"_"+id).html("");
        var url = '@Url.Action("GetRto","PrivateCar")';
        $.ajax({
            type: "GET",
            url: url,
            data: { stateid: stateid, rtodesc: rtodesc },
            success: function (data) {
                var html = "";
                $.each(data, function (k) {
                    html += '<li ><span class="spanrto" rtocode=' + data[k].rtocode + ' ng-click="SelectRTO($event)" >' + data[k].rtocode + '</span></li>';
                });

                $htmlstr = $(html).appendTo('#divrto_' + stateid + "_" + id);
                $compile($htmlstr)($scope);
            }
        });
        FirstQuote.RTOCode = rtodesc;

    }

    var QuoteModel = {
        "PolicyType": '',//$scope.PolicyType,
        "PolicyStartDate": new Date(),//"2021-04-12T00:00:00+05:30",
        "PolicyEndDate": "2022-04-20T00:00:00+05:30",
        "PlanName": "",
        "PlanId": "",
        "CompanyName": '',
        "CustomerType": "Individual",
        "OrganizationName": "",
        "NomineeName": "",
        "NomineeDateOfBirth": "",
        "NomineeRelationShip": "",
        "NomineeGender": "",
        "AppointeeName": "",
        "AppointeeRelationShip": "",
        "IsOwnerChanged": false,
        "DontKnowPreviousInsurer": false,
        "IsThirdPartyOnly": false,
        "IsODOnly": false,
        "IsValidLicence": true,
        "IDV": "0",
        "RequestType": "Quote",
        "VehicleDetails":
        {

            "VariantId": "12",
            "RtoId": "121",
            "RtoZone": "A",
            "PurchaseDate": "22 Mar 2021",
            "ManufaturingDate": "01 Mar 2021",
            "RegistrationDate": "22 Mar 2021",
            "RegistrationNumber": "",
            "EngineNumber": "",
            "ChassisNumber": "",
            "MakeName": "HONDA",
            "ModelName": "BRIO",
            "VariantName": "V MT",
            "MakeCode": "HONDA",
            "ModelCode": "BRIO",
            "VariantCode": "V MT",
            "BiFuelType": "",
            "IsVehicleLoan": false,
            "LoanCompanyName": "",
            "LoanAmount": "0",
            "IsValidPUC": true,
            "PUCNumber": "",
            "PUCStartDate": "",
            "PUCEndDate": "",
            "CC": "1198",
            "SC": "5",
            "ExShowroomPrice": "0",
            "Segment": "",
            "Fuel": "Petrol",
            "BodyType": "Sedan",
            "VehicleColor": "Black"
        },
        "VehicleAddressDetails":
        {
            "Address1": "address one",
            "Address2": "address two",
            "Address3": "address three",
            "Pincode": "380051",
            "Country": "India",
            "State": "Gujarat",
            "City": "Ahmedabad"
        },
        "PreviousPolicyDetails": null,
        "CustomerAddressDetails":
        {
            "IsRegistrationAddressSame": true,
            "Address1": "address one",
            "Address2": "address two",
            "Address3": "address three",
            "Pincode": "380051",
            "Country": "India",
            "State": "Gujarat",
            "City": "Ahmedabad"
        },
        "ClientDetails":
        {

        },
        "DiscountDetails":
        {
            "IsVoluntaryExcess": false,
            "IsAntiTheftDevice": false,
            "IsMemberOfAutomobileAssociation": false,
            "IsTPPDRestrictedto6000": false,
            "IsUseForHandicap": false,
            "AssociationName": "",
            "MembershipNumber": "",
            "VoluntaryExcessAmount": 0
        },
        "CoverageDetails":
        {
            "IsElectricalAccessories": false,
            "IsNonElectricalAccessories": false,
            "SIElectricalAccessories": 0,
            "SINonElectricalAccessories": 0,
            "IsBiFuelKit": false,
            "BiFuelKitAmount": 0,
            "IsFiberGlassFuelTank": false,
            "IsLegalLiablityPaidDriver": false,
            "NoOfLLPaidDriver": 0,
            "IsEmployeeLiability": false,
            "IsPACoverUnnamedPerson": false,
            "IsPACoverPaidDriver": false,
            "PACoverPaidDriverAmount": 0,
            "PACoverUnnamedPersonAmount": 0,
            "ElectricalAccessoriesDetails": null,
            "NonElectricalAccessoriesDetails": null
        },
        "CustomIDV": {},
        "CurrentNcb": "0",
        "AddonCover": null
    };

    $scope.QuoteReqModel = [];

    var FirstQuote = {};

    $scope.QualifyCompany = function () {
        $("#pageloader").fadeIn();
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
                FirstQuote.EnqId = response.data.enquiryid;
                $scope.ProgressTotal = $scope.QualifyCompanyResData.length;
                $scope.ProgressCount = 0;

                $sessionStorage.QualifyCompanyResData = $scope.QualifyCompanyResData;
                angular.forEach($scope.QualifyCompanyResData, function (companylist) {
                    //if (companylist.comname == "ICICI") {
                    QuoteModel.PolicyType = "New";//$scope.PolicyType;
                    QuoteModel.CompanyName = companylist.comname;
                    QuoteModel.VehicleDetails.VariantId = "12";
                    QuoteModel.VehicleDetails.RtoId = "121";
                    QuoteModel.VehicleDetails.RtoZone = "A";
                    QuoteModel.VehicleDetails.PurchaseDate = "22 Mar 2021";
                    QuoteModel.VehicleDetails.ManufaturingDate = "01 Mar 2021";
                    QuoteModel.VehicleDetails.RegistrationDate = "22 Mar 2021";
                    //QuoteModel.VehicleDetails.RegistrationNumber = "";
                    //QuoteModel.VehicleDetails.EngineNumber = "";
                    //QuoteModel.VehicleDetails.ChassisNumber = "";
                    QuoteModel.VehicleDetails.MakeName = companylist.makename;
                    QuoteModel.VehicleDetails.ModelName = companylist.modelname;
                    QuoteModel.VehicleDetails.VariantName = companylist.variantname;
                    QuoteModel.VehicleDetails.MakeCode = companylist.makecode;
                    QuoteModel.VehicleDetails.ModelCode = companylist.modelcode;
                    QuoteModel.VehicleDetails.VariantCode = companylist.variantcode;
                    //QuoteModel.VehicleDetails.BiFuelType = "";
                    //QuoteModel.VehicleDetails.IsVehicleLoan = false;
                    //QuoteModel.VehicleDetails.LoanCompanyName = "";
                    //QuoteModel.VehicleDetails.LoanAmount = "0";
                    //QuoteModel.VehicleDetails.IsValidPUC = true;
                    //QuoteModel.VehicleDetails.PUCNumber = "";
                    //QuoteModel.VehicleDetails.PUCStartDate = "";
                    //QuoteModel.VehicleDetails.PUCEndDate = "";
                    QuoteModel.VehicleDetails.CC = companylist.cc;
                    QuoteModel.VehicleDetails.SC = companylist.sc;
                    QuoteModel.VehicleDetails.ExShowroomPrice = "0";
                    QuoteModel.VehicleDetails.Segment = companylist.segment;
                    QuoteModel.VehicleDetails.Fuel = companylist.fuel;
                    QuoteModel.VehicleDetails.BodyType = companylist.bodytype;
                    QuoteModel.VehicleDetails.VehicleColor = "Black";
                    $scope.QuoteResponse = [];
                    var newQuote = {};
                    newQuote = angular.copy(QuoteModel);
                    $scope.QuoteReqModel.push(newQuote);
                        
                    //angular.extend(QuoteModel, VehicleDetails);
                    //$scope.APIURL=APIURL + companylist.comname;
                    $http({
                        url: '/PrivateCar/GetQuoteCompanyWise',
                        method: "POST",
                        data: JSON.stringify(QuoteModel)
                    }).then(function (response) {
                        if (!angular.isUndefined(response && response.data.length > 0)) {
                            $scope.ProgressCount++;
                            $scope.CompanyResData = response.data;

                            if (response.data.Status === 1) {
                                $scope.QuoteResponse.push(response.data);
                            }

                            //   console.log($scope.CompanyResData);

                            if ($scope.ProgressTotal == $scope.ProgressCount) {

                                $http({
                                    url: '/PrivateCar/InsertEnq',
                                    method: "POST",
                                    data: { EnqId: FirstQuote.EnqId, req: JSON.stringify($scope.QuoteReqModel), res: JSON.stringify($scope.QuoteResponse), type: 1 }
                                })
                                .then(function (result) {
                                })
                                //$sessionStorage.PCQuotation = angular.copy(QuoteModel);
                                $sessionStorage.FirstQuote = angular.copy(FirstQuote);
                                //$scope.QuoteReqModel.append();
                                $sessionStorage.PCQuotation = angular.copy($scope.QuoteReqModel);
                                $sessionStorage.PCQuotationResPonse = angular.copy($scope.QuoteResponse);
                                window.location.href = '/Motor/PrivateCar/PolicyDetails';
                            }
                        }
                        else {
                        }
                    }, function (error) {
                    });

                    //}

                });//f
            }
            else {
                //growl.error('Error while fetching state details!!!');
            }
        }, function (error) {
            //growl.error('error');
        });

    }
    $scope.SelectRTO = function ($event) {
        var code = ($event.target).attributes["rtocode"].value;
        FirstQuote.RTOCode = code;
        $(".spanrto").removeClass('active');
        $(event.target).addClass('active');

        $('#rtocode').val(code);
@*@Model.rtocode = code;*@
$scope.divcity = false;
        $scope.divmake = true;
        $('#hdf_step').val('4');
    };
    $scope.selectMake = function (makeid, makename) {
        $('.make').removeClass('active');
        $("#make_" + makeid).addClass('active');
        $("#makeid").val(makeid);
        $('#ddlmake').val(makeid);
        $('#select2-chosen-1').html(makename);
        FirstQuote.MakeName = makename;
        $('#hdf_step').val('5');
        $scope.divmake = false;
        $scope.divmodel = true;
        GetModel(makeid);

    };
    $scope.selectModel = function (modelid, modelname) {

        $('.model').removeClass('active');
        $("#model_" + modelid).addClass('active');
        $("#modelid").val(modelid);
        $('#modeltext').val(modelname);
        FirstQuote.ModelName = modelname;
        $scope.divmodel = false;
        $scope.divfueltype = true;
        $('#hdf_step').val('6');
    };
    function GetModel(makeid) {
        var url = '@Url.Action("GetModel", "PrivateCar")';
        $http({
            url: url,
            method: "POST",
            data: { makeid: makeid }
        })
        .then(function (result) {
            //  console.log(result.data.model);
            $scope.modellist = result.data.model;
        })

    }
    $("#ddlmake").change(function () {
        var makeid = $('#ddlmake').val();
        $("#makeid").val(makeid);
        $('.make').removeClass('active');
        $("#make_" + makeid).addClass('active');
        $('#hdf_step').val('5');
        $scope.divmake = false;
        $scope.divmodel = true;
        GetModel(makeid);

    });
    $scope.GetFuel = function ($event, fueltype) {
        @*'@Model.fueltype' = fueltype;*@
        $('#fueltypeid').val(fueltype);
        $('.fuel').removeClass('active');
        $(event.target).addClass('active');
        FirstQuote.Fuel = fueltype;
        $scope.divfueltype = false;
        $scope.divvariant = true;
        GetVariant(fueltype);
        $('#hdf_step').val('7');
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
        FirstQuote.VariantId = variantid;
        FirstQuote.VariantName = variantname;
        $scope.divvariant = false;
        $scope.divmanufactureyear = true;
        GetManufacturingYear();
        $('#hdf_step').val('8');
    }
    function GetManufacturingYear() {

        if ($('#policytype').val() == 'New') {
            var year = new Date().getFullYear();
            $('#manufacturingyear').val(year);
            FirstQuote.ManufactureYear = year;
        }
        else {

            var url = '@Url.Action("GetManufacturingYear", "PrivateCar")';
            $http({
                url: url,
                method: "POST"
            })
            .then(function (result) {
                // console.log(result.data);
                $scope.myearlist = result.data;
            })
        }
    }
    $scope.SelectMyear = function (year) {
        $('#manufacturingyear').val(year);
           
        FirstQuote.ManufactureYear = year;
    }
    $scope.SetFocus = function (element) {

        if (element == 'makeid')
            $('#' + element).select2("open");
        else
            $('#' + element).focus();
    }
    $scope.PrevStep = function () {

        var stepval = $('#hdf_step').val();
        stepval = stepval - 1;
        $('#hdf_step').val(stepval);
        if (stepval == 1) {
            $scope.divstate = false;
            $scope.divcity = false;
            $scope.divmake = false;
            $scope.divmodel = false;
            $scope.divvariant = false;
            $scope.divfueltype = false;
            $scope.divmanufactureyear = false;
            $scope.divinfo = true;
        }
        else if (stepval == 2) {
            $scope.divinfo = false;
            $scope.divcity = false;
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
            $scope.divmake = false;
            $scope.divcity = false;
            $scope.divmodel = false;
            $scope.divfueltype = false;
            $scope.divvariant = false;
            $scope.divmanufactureyear = false;
            $scope.divcity = true;
            $('#btnnext_divmake').show();
        }
        else if (stepval == 4) {
            $scope.divinfo = false;
            $scope.divstate = false;
            $scope.divcity = false;
            $scope.divcity = false;
            $scope.divmodel = false;
            $scope.divfueltype = false;
            $scope.divvariant = false;
            $scope.divmanufactureyear = false;
            $scope.divmake = true;
            $('#btnnext_divmake').show();
        }
        else if (stepval == 5) {
            $scope.divinfo = false;
            $scope.divstate = false;
            $scope.divfueltype = false;
            $scope.divcity = false;
            $scope.divmake = false;
            $scope.divvariant = false;
            $scope.divmanufactureyear = false;
            $scope.divmodel = true;
            $('#btnnext_divmodel').show();
        }
        else if (stepval == 6) {
            $scope.divinfo = false;
            $scope.divstate = false;
            $scope.divcity = false;
            $scope.divmake = false;
            $scope.divvariant = false;
            $scope.divmanufactureyear = false;
            $scope.divmodel = false;
            $scope.divfueltype = true;
            $('#btnnext_divfueltype').show();
        }
        else if (stepval == 7) {
            $scope.divinfo = false;
            $scope.divstate = false;
            $scope.divcity = false;
            $scope.divmake = false;
            $scope.divmodel = false;
            $scope.divfueltype = false;
            $scope.divmanufactureyear = false;
            $scope.divvariant = true;
            $('#btnnext_divvariant').show();
        }
        else if (stepval == 8) {
            $scope.divinfo = false;
            $scope.divstate = false;
            $scope.divcity = false;
            $scope.divmake = false;
            $scope.divmodel = false;
            $scope.divfueltype = false;
            $scope.divvariant = false;
            $scope.divmanufactureyear = true;
            $('#btnnext_divmanufactureyear').show();
        }
    }

    $scope.NextStep = function () {

        var stepval = $('#hdf_step').val();
        stepval = parseInt(stepval) + 1;

        $('#hdf_step').val(stepval);
        if (stepval == 1) {
            $scope.divstate = false;
            $scope.divcity = false;
            $scope.divmake = false;
            $scope.divmodel = false;
            $scope.divvariant = false;
            $scope.divfueltype = false;
            $scope.divmanufactureyear = false;
            $scope.divinfo = true;
        }
        else if (stepval == 2) {
            $scope.divcity = false;
            $scope.divmake = false;
            $scope.divmodel = false;
            $scope.divvariant = false;
            $scope.divmanufactureyear = false;
            $scope.divfueltype = false;
            $scope.divinfo = false;
            $scope.divstate = true;
        }
        else if (stepval == 3) {
            $scope.divmake = false;
            $scope.divmodel = false;
            $scope.divvariant = false;
            $scope.divmanufactureyear = false;
            $scope.divfueltype = false;
            $scope.divinfo = false;
            $scope.divstate = false;
            $scope.divcity = true;
        }
        else if (stepval == 4) {
            $scope.divcity = false;
            $scope.divmodel = false;
            $scope.divvariant = false;
            $scope.divmanufactureyear = false;
            $scope.divfueltype = false;
            $scope.divinfo = false;
            $scope.divstate = false;
            $scope.divmake = true;
        }
        else if (stepval == 5) {
            $scope.divcity = false;
            $scope.divvariant = false;
            $scope.divmanufactureyear = false;
            $scope.divfueltype = false;
            $scope.divinfo = false;
            $scope.divstate = false;
            $scope.divmake = false;
            $scope.divmodel = true;
        }
        else if (stepval == 6) {
            $scope.divcity = false;
            $scope.divvariant = false;
            $scope.divmanufactureyear = false;
            $scope.divinfo = false;
            $scope.divstate = false;
            $scope.divmake = false;
            $scope.divmodel = false;
            $scope.divfueltype = true;
        }
        else if (stepval == 7) {
            $scope.divcity = false;
            $scope.divinfo = false;
            $scope.divstate = false;
            $scope.divmake = false;
            $scope.divmodel = false;
            $scope.divfueltype = false;
            $scope.divmanufactureyear = false;
            $scope.divvariant = true;
        }
        else if (stepval == 8) {
            $scope.divcity = false;
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
 