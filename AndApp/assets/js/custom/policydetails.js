 
    var companylist = [];
var app = angular.module("myApp", ["ngStorage"]);
app.controller("myResultCtrl", function ($scope, $http, $window, $compile, $sessionStorage) {
    $scope.companylistName = [];
    $scope.CustomIDV = [];
    $scope.FirstQuote = !angular.isUndefined($sessionStorage.FirstQuote) ? (angular.copy($sessionStorage.FirstQuote)) : $scope.FirstQuote;
    $scope.QuotationReq = !angular.isUndefined($sessionStorage.PCQuotation) ? (angular.copy($sessionStorage.PCQuotation)) : $scope.QuotationReq;
    $scope.QuotationRes = !angular.isUndefined($sessionStorage.PCQuotationResPonse) ? (angular.copy($sessionStorage.PCQuotationResPonse)) : $scope.QuotationRes;
    //$scope.QuotationReq = angular.copy($sessionStorage.PCQuotation);
    $scope.QuotationRes = angular.copy($sessionStorage.PCQuotationResPonse);
    angular.forEach($scope.QuotationRes, function (res) {
        $scope.companylistName.push(res.CompanyName);
        var newIDV = {};
        newIDV.MinIDV = res.MinIDV;
        newIDV.MaxIDV = res.MaxIDV;
        newIDV.CompanyName = res.CompanyName;
        $scope.CustomIDV.push(newIDV);
    });
    companylist = angular.copy($scope.companylistName);
    //console.log($scope.QuotationRes);
    $scope.minIDV = Math.min.apply(Math, $scope.QuotationRes.map(function (item) { return item.MinIDV; }));
    $scope.maxIDV = Math.max.apply(Math, $scope.QuotationRes.map(function (item) { return item.MaxIDV; }));

    $scope.UpdateIdvModel = function (idv) {
        idv = $("#txtIDV").text();
        ReCalculateQuotation(idv);

    }

    $scope.UpdateIdvModel = function (idv) {
        idv = $("#txtIDV").text();
        ReCalculateQuotation(idv);

    }

    function ReCalculateQuotation(idv, accessory) {
        $scope.QuotationRes = undefined;
        angular.forEach($scope.QuotationReq, function (companyQuote) {
            $scope.QuoteResponse = [];
            if (idv) {
                companyQuote.IDV = idv;
                companyQuote.CustomIDV = $scope.CustomIDV;
            }
            else {
                var coveragedetail = companyQuote.CoverageDetails;
                coveragedetail.IsElectricalAccessories = false;
                coveragedetail.SIElectricalAccessories = 0;
                coveragedetail.IsNonElectricalAccessories = false;
                coveragedetail.SINonElectricalAccessories = 0;
                coveragedetail.IsBiFuelKit = false;
                coveragedetail.BiFuelKitAmount = 0;
                for (i = 0; i < accessory.length; i++) {
                    if (accessory[i] == "elec") {
                        coveragedetail.IsElectricalAccessories = true;
                        coveragedetail.SIElectricalAccessories = $('#txt_elec').val();
                    }
                    else if (accessory[i] == "nonelec") {
                        coveragedetail.IsNonElectricalAccessories = true;
                        coveragedetail.SINonElectricalAccessories = $('#txt_nonelec').val();
                    }
                    else {
                        coveragedetail.IsBiFuelKit = true;
                        coveragedetail.BiFuelKitAmount = $('#txt_bifuel').val();
                    }
                }
                companyQuote.CoverageDetails = coveragedetail;

                console.log(coveragedetail);
            }
            $http({
                url: '/PrivateCar/GetQuoteCompanyWise',
                method: "POST",
                data: JSON.stringify(companyQuote)
            }).then(function (response) {
                if (!angular.isUndefined(response && response.data.length > 0)) {
                    $scope.CompanyResData = response.data;
                    //alert(response.data.Status);
                    if (response.data.Status === 1) {
                        $scope.QuoteResponse.push(response.data);
                        $scope.QuotationRes = angular.copy($scope.QuoteResponse)
                    }
                }
                else {
                }
            }, function (error) {
            });
        });
    }
    $scope.UpdateAccessory = function () {
        debugger;
        var flag = true;
        var ischecked = false;
        var accessory = [];
        $('.accchk:checked').map(function () {

            ischecked = true;
            var id = $(this).attr('id').substring(3);
            valid = $('#txt_' + id).hasClass('ng-valid');
            notempty = $('#txt_' + id).hasClass('ng-not-empty');
            if (!valid || !notempty) {
                flag = false;
            }
            else
                accessory.push(id);
        });
        if (flag == false) {
            $('#acc_error').text("Please Enter Value within given range!");
            return false;
        }
        else if (!ischecked) {
            $('#acc_error').text("");
            $scope.btnaccupdate = false;
            return false;
        }
        else {
            $('#acc_error').text("");
            ReCalculateQuotation('', accessory);

        }

    }
    $scope.showsubmit = function () {
        $scope.btnaccupdate = true;
    }
    $scope.UpdateAntitheft = function (value) {
        debugger;
        for (var j = 0; j < companylist.length; j++) {
            var i = companylist[j];
            var tot = $('.prem_' + i).text();
            totalpremium = parseInt(tot);
            antitheftvalue = $('#div_' + i).attr('data-antitheft');
            if (value == 'Y') {
                $scope.anti = true;
                totalpremium = totalpremium - parseInt(antitheftvalue);
            }
            else {
                $scope.anti = false;
                totalpremium = totalpremium + parseInt(antitheftvalue);
            }
            $('.prem_' + i).text(totalpremium);
        }
    }
    //$scope.UpdateIdvModel = function (idv) {
    //    idv = $("#txtIDV").text();
    //    $scope.QuotationRes = undefined;
    //    angular.forEach($scope.QuotationReq, function (companyQuote) {
    //        $scope.QuoteResponse = [];
    //        companyQuote.IDV = idv;
    //        companyQuote.CustomIDV = $scope.CustomIDV;
    //        $http({
    //            url: '/PrivateCar/GetQuoteCompanyWise',
    //            method: "POST",
    //            data: JSON.stringify(companyQuote)
    //        }).then(function (response) {
    //            if (!angular.isUndefined(response && response.data.length > 0)) {
    //                $scope.CompanyResData = response.data;
    //                //alert(response.data.Status);
    //                if (response.data.Status === 1) {
    //                    $scope.QuoteResponse.push(response.data);
    //                    $scope.QuotationRes = angular.copy($scope.QuoteResponse)
    //                }
    //            }
    //            else {
    //            }
    //        }, function (error) {
    //        });
    //    });
    //}


});
 
 
    $('.addonchk').click(function () {

        var html = "";
        var html1 = "";
        var planname = "";
        var ischecked = $(this).prop('checked');
        var value = $(this).val();
        var name = $(this).attr('addonname');
        for (var j = 0; j < companylist.length; j++) {
            var i = companylist[j];
            var tot = $('.prem_' + i).text();
            var totaladdon = parseFloat($('#' + i + '_totaladdon').val());

            totalpremium = parseFloat(tot);   //parseFloat(tot.substring(2));

            if (ischecked) {

                if (value == "zerodep") {
                    var zero = $('#div_' + i).attr('data-zerodep');
                    if (typeof zero !== typeof undefined && zero !== "0" && zero != 0) {
                        html = "<p style='color:green; font-size:12px;' class='mb-0 " + value + "'>" + name + "</p>";
                        $('#addondiv_' + i).append(html);

                        html1 = "<p class='" + value + "'><span>" + name + "</span><span style='float:right;'>₹ " + parseFloat(zero) + "</span></p>";
                        $('#' + i + '_addonwithval').append(html1);
                        totaladdon = totaladdon + parseFloat(zero);
                        totalpremium = totalpremium + parseFloat(zero);
                    }
                }
                else if (value == "engine") {
                    var engine = $('#div_' + i).attr('data-engineprot');
                    if (typeof engine !== typeof undefined && engine !== "0" && engine != 0) {
                        html = "<p style='color:green; font-size:12px;' class='mb-0 " + value + "'>" + name + "</p>";
                        $('#addondiv_' + i).append(html);


                        html1 = "<p class='" + value + "'><span>" + name + "</span><span style='float:right;'>₹ " + parseFloat(engine) + "</span></p>";
                        $('#' + i + '_addonwithval').append(html1);
                        totaladdon = totaladdon + parseFloat(engine);
                        totalpremium = totalpremium + parseFloat(engine);
                    }
                }
                else if (value == "rim") {
                    var rim = $('#div_' + i).attr('data-rimprot');
                    if (typeof rim !== typeof undefined && rim !== "0" && rim != 0) {
                        html = "<p style='color:green; font-size:12px;' class='mb-0 " + value + "'>" + name + "</p>";
                        $('#addondiv_' + i).append(html);

                        html1 = "< class='" + value + "'p><span>" + name + "</span><span style='float:right;'>₹ " + parseFloat(rim) + "</span></p>"; // this will be display in breakup add by akash one remove it
                        $('#' + i + '_addonwithval').append(html1);
                        totaladdon = totaladdon + parseFloat(rim);
                        totalpremium = totalpremium + parseFloat(rim);
                    }
                }
                else if (value == "invprice") {
                    var invoice = $('#div_' + i).attr('data-invoiceprot');
                    if (typeof invoice !== typeof undefined && invoice !== "0" && invoice != 0) {
                        html = "<p style='color:green; font-size:12px;' class='mb-0 " + value + "'>" + name + "</p>";
                        $('#addondiv_' + i).append(html);



                        html1 = "<p class='" + value + "'><span>" + name + "</span><span style='float:right;'>₹ " + parseFloat(invoice) + "</span></p>"; // this will be display in breakup add by akash one remove it
                        $('#' + i + '_addonwithval').append(html1);
                        totaladdon = totaladdon + parseFloat(invoice);
                        totalpremium = totalpremium + parseFloat(invoice);
                    }
                }
                else if (value == "keylock") {
                    var keylock = $('#div_' + i).attr('data-keylock');
                    if (typeof keylock !== typeof undefined && keynlock !== "0" && keylock != 0) {
                        html = "<p style='color:green; font-size:12px;' class='mb-0 " + value + "'>" + name + "</p>";
                        $('#addondiv_' + i).append(html);


                        html1 = "<p class='" + value + "'><span>" + name + "</span><span style='float:right;'>₹ " + parseFloat(keylock) + "</span></p>"; // this will be display in breakup add by akash one remove it
                        $('#' + i + '_addonwithval').append(html1);
                        totaladdon = totaladdon + parseFloat(keylock);
                        totalpremium = totalpremium + parseFloat(keylock);
                    }
                }
                else if (value == "ncbprotect") {
                    var ncb = $('#div_' + i).attr('data-ncbprotect');
                    if (typeof ncb !== typeof undefined && ncb !== "0" && ncb != 0) {
                        html = "<p style='color:green; font-size:12px;' class='mb-0 " + value + "'>" + name + "</p>";
                        $('#addondiv_' + i).append(html);




                        html1 = "<p class='" + value + "'><span>" + name + "</span><span style='float:right;'>₹ " + parseFloat(ncb) + "</span></p>"; // this will be display in breakup add by akash one remove it
                        $('#' + i + '_addonwithval').append(html1);
                        totaladdon = totaladdon + parseFloat(ncb);
                        totalpremium = totalpremium + parseFloat(ncb);
                    }
                }
                else if (value == "consumables") {
                    var consumables = $('#div_' + i).attr('data-consumables');
                    if (typeof consumables !== typeof undefined && consumables !== "0" && consumables != 0) {
                        html = "<p style='color:green; font-size:12px;' class='mb-0 " + value + "'>" + name + "</p>";
                        $('#addondiv_' + i).append(html);


                        html1 = "<p class='" + value + "'><span>" + name + "</span><span style='float:right;'>₹ " + parseFloat(consumables) + "</span></p>"; // this will be display in breakup add by akash one remove it
                        $('#' + i + '_addonwithval').append(html1);
                        totaladdon = totaladdon + parseFloat(consumables);
                        totalpremium = totalpremium + parseFloat(consumables);
                    }
                }
                else if (value == "dailyallowance") {
                    var dailyallowance = $('#div_' + i).attr('data-dailyallowance');
                    if (typeof dailyallowance !== typeof undefined && dailyallowance !== "0" && dailyallowance != 0) {
                        html = "<p style='color:green; font-size:12px;' class='mb-0 " + value + "'>" + name + "</p>";
                        $('#addondiv_' + i).append(html);



                        html1 = "<p class='" + value + "'><span>" + name + "</span><span style='float:right;'>₹ " + parseFloat(dailyallowance) + "</span></p>"; // this will be display in breakup add by akash one remove it
                        $('#' + i + '_addonwithval').append(html1);
                        totaladdon = totaladdon + parseFloat(dailyallowance);
                        totalpremium = totalpremium + parseFloat(dailyallowance);
                    }
                }
                else if (value == "tyre") {
                    var tyre = $('#div_' + i).attr('data-tyre');
                    if (typeof tyre !== typeof undefined && tyre !== "0" && tyre != 0) {
                        html = "<p style='color:green; font-size:12px;' class='mb-0 " + value + "'>" + name + "</p>";
                        $('#addondiv_' + i).append(html);



                        html1 = "<p class='" + value + "'><span>" + name + "</span><span style='float:right;'>₹ " + parseFloat(tyre) + "</span></p>"; // this will be display in breakup add by akash one remove it
                        $('#' + i + '_addonwithval').append(html1);
                        totaladdon = totaladdon + parseFloat(tyre);
                        totalpremium = totalpremium + parseFloat(tyre);
                    }
                }
                else if (value == "personalloss") {
                    var loss = $('#div_' + i).attr('data-personalloss');
                    if (typeof loss !== typeof undefined && loss !== "0" && loss != 0) {
                        html = "<p style='color:green; font-size:12px;' class='mb-0 " + value + "'>" + name + "</p>";
                        $('#addondiv_' + i).append(html);

                        html1 = "<p class='" + value + "'><span>" + name + "</span><span style='float:right;'>₹ " + parseFloat(loss) + "</span></p>"; // this will be display in breakup add by akash one remove it
                        $('#' + i + '_addonwithval').append(html1);
                        totaladdon = totaladdon + parseFloat(loss);
                        totalpremium = totalpremium + parseFloat(loss);
                    }
                }
                else if (value == "roadside") {
                    var roadside = $('#div_' + i).attr('data-roadside');
                    if (typeof roadside !== typeof undefined && roadside !== "0" && roadside != 0) {
                        html = "<p style='color:green; font-size:12px;' class='mb-0 " + value + "'>" + name + "</p>";
                        $('#addondiv_' + i).append(html);


                        html1 = "<p class='" + value + "'><span>" + name + "</span><span style='float:right;'>₹ " + parseFloat(roadside) + "</span></p>"; // this will be display in breakup add by akash one remove it
                        $('#' + i + '_addonwithval').append(html1);
                        totaladdon = totaladdon + parseFloat(roadside);
                        totalpremium = totalpremium + parseFloat(roadside);
                    }
                }
            }
            else {
                if (value == "zerodep") {
                    var zero = $('#div_' + i).attr('data-zerodep');
                    if (typeof zero !== typeof undefined && zero !== "0" && zero != 0) {
                        totaladdon = totaladdon - parseFloat(zero);
                        totalpremium = totalpremium - parseFloat(zero);
                    }
                }
                else if (value == "engine") {
                    var engine = $('#div_' + i).attr('data-engineprot');
                    if (typeof engine !== typeof undefined && engine !== "0" && engine != 0) {
                        totaladdon = totaladdon - parseFloat(engine);
                        totalpremium = totalpremium - parseFloat(engine);
                    }
                }
                else if (value == "rim") {
                    var rim = $('#div_' + i).attr('data-rimprot');
                    if (typeof rim !== typeof undefined && rim !== "0" && rim != 0) {
                        totaladdon = totaladdon - parseFloat(rim);
                        totalpremium = totalpremium - parseFloat(rim);
                    }
                }
                else if (value == "invprice") {
                    var invoice = $('#div_' + i).attr('data-invoiceprot');
                    if (typeof invoice !== typeof undefined && invoice !== "0" && invoice != 0) {
                        totaladdon = totaladdon - parseFloat(invoice);
                        totalpremium = totalpremium - parseFloat(invoice);
                    }
                }
                else if (value == "keylock") {
                    var keynlock = $('#div_' + i).attr('data-keylock');
                    if (typeof keynlock !== typeof undefined && keynlock !== "0" && keynlock != 0) {
                        totaladdon = totaladdon - parseFloat(keynlock);
                        totalpremium = totalpremium - parseFloat(keynlock);
                    }
                }
                else if (value == "ncbprotect") {
                    var ncb = $('#div_' + i).attr('data-ncbprotect');
                    if (typeof ncb !== typeof undefined && ncb !== "0" && ncb != 0) {
                        totaladdon = totaladdon - parseFloat(ncb);
                        totalpremium = totalpremium - parseFloat(ncb);
                    }
                }
                else if (value == "consumables") {
                    var consumable = $('#div_' + i).attr('data-consumables');
                    if (typeof consumable !== typeof undefined && consumable !== "0" && consumable != 0) {
                        totaladdon = totaladdon - parseFloat(consumable);
                        totalpremium = totalpremium - parseFloat(consumable);
                    }
                }
                else if (value == "dailyallowance") {
                    var dailyallowance = $('#div_' + i).attr('data-dailyallowance');
                    if (typeof dailyallowance !== typeof undefined && dailyallowance !== "0" && dailyallowance != 0) {
                        totaladdon = totaladdon - parseFloat(dailyallowance);
                        totalpremium = totalpremium - parseFloat(dailyallowance);
                    }
                }
                else if (value == "tyre") {
                    var tyre = $('#div_' + i).attr('data-tyre');
                    if (typeof tyre !== typeof undefined && tyre !== "0" && tyre != 0) {
                        totaladdon = totaladdon - parseFloat(tyre);
                        totalpremium = totalpremium - parseFloat(tyre);
                    }
                }
                else if (value == "personalloss") {
                    var loss = $('#div_' + i).attr('data-personalloss');
                    if (typeof loss !== typeof undefined && loss !== "0" && loss != 0) {
                        totaladdon = totaladdon - parseFloat(loss);
                        totalpremium = totalpremium - parseFloat(loss);
                    }
                }
                else if (value == "roadside") {
                    var roadside = $('#div_' + i).attr('data-roadside');
                    if (typeof roadside !== typeof undefined && roadside !== "0" && roadside != 0) {
                        totaladdon = totaladdon - parseFloat(roadside);
                        totalpremium = totalpremium - parseFloat(roadside);
                    }
                }
                $("." + value).remove();

            }
            $('.prem_' + i).text(totalpremium);

            $('#' + i + '_totaladdon').val(totaladdon);
        }
    })


    $('#chkowndriver').click(function () {

        var ischecked = $(this).prop('checked');
        UpdateCover(ischecked, 'owndriver');
    })
    $('#chkpaiddriver').click(function () {

        var ischecked = $(this).prop('checked');
        UpdateCover(ischecked, 'paiddriver');
    });
    $('#chkllpaiddriver').click(function () {

        var ischecked = $(this).prop('checked');
        UpdateCover(ischecked, 'LLpaiddriver');
    });


    $('.accchk').click(function () {
        debugger;
        var ischecked = $(this).prop('checked');
        var id = $(this).attr('id').substring(3);
        if (ischecked)
            $('#div_' + id).show();
        else {
            $('#div_' + id).hide();
            $('#txt_' + id).val("");
        }
    });
    function UpdateCover(ischecked, id) {
        for (var j = 0; j < companylist.length; j++) {
            debugger;
            var html = "";
            var coveramount = 0;
            var i = companylist[j];
            var tot = $('.prem_' + i).text();
            totalpremium = parseInt(tot);
            if (id == "owndriver")
                coveramount = $('#div_' + i).attr('data-pacover');
            else if (id == "paiddriver")
                coveramount = $('#div_' + i).attr('data-paiddriver');
            else if (id == "LLpaiddriver")
                coveramount = $('#div_' + i).attr('data-LLpaiddriver');
            if (ischecked) {
                totalpremium = totalpremium + parseInt(coveramount);
            }
            else {
                totalpremium = totalpremium - parseInt(coveramount);
            }
            $('.prem_' + i).text(totalpremium);
        }
    }

$(document).ready(function () {
    $('input[type="checkbox"]').click(function () {
        var inputValue = $(this).attr("value");
        if ($(this).is(":checked")) {
            $('.div_' + inputValue).show();
        }
        else {
            $('.div_' + inputValue).hide();
        }
    });
});
$(document).on('click', '.selectMe', function () {

    $("#prm_ADDON").html("");
    $("#modal-policy").modal("show");
    var totalcover = 0;
    var productID = $(this).attr('data-title');
    var basicod = $("#div_" + productID).attr('data-basicod');
    var idv = $("#div_" + productID).attr('data-idv');
    var basictp = $("#div_" + productID).attr('data-thirdpartycover');
    var owndrivercover = $("#div_" + productID).attr('data-pacover');
    var paiddrivercover = $("#div_" + productID).attr('data-paiddriver');
    var LLpaiddrivercover = $("#div_" + productID).attr('data-LLpaiddriver');
    var image = "/assets/images/" + productID + ".png";
    $("#modalimg").attr('src', image);
    $("#modalimg").attr('style', 'width:50%');
    $("#prm_IDV").text(idv);
    $("#prm_OD").text(basicod);

    $("#prm_TP").text(basictp);
    var totaladdon = $('#' + productID + '_totaladdon').val();
    if (totaladdon > 0) {
        $("#divaddonprm").show();
        $("#prm_Totaladdon").text(totaladdon);
    }
    else
        $("#divaddonprm").hide();
    var addons = $('#' + productID + '_addonwithval').html();
    $("#prm_ADDON").append(addons);
    //owndriverlogic start

    if ($('#chkowndriver').prop('checked')) {
        $("#div_OWNDRIVER").show();
        $("#prm_Owndirver").text(owndrivercover);
        totalcover = totalcover + parseInt(owndrivercover);

    }
    else {
        $("#div_OWNDRIVER").hide();
    }
    // end owndriverlogic
    //Start paiddriverlogic
    if ($('#chkpaiddriver').prop('checked')) {
        $("#div_PAIDDRIVER").show();
        $("#prm_Paiddriver").text(paiddrivercover);
        totalcover = totalcover + parseInt(paiddrivercover);

    }
    else {
        $("#div_PAIDDRIVER").hide();
    }
    //end paiddriverlogic
    //Start LLpaiddriverlogic
    if ($('#chkllpaiddriver').prop('checked')) {
        $("#div_LLPAIDDRIVER").show();
        $("#prm_LLPaiddriver").text(LLpaiddrivercover);
        totalcover = totalcover + parseInt(LLpaiddrivercover);

    }
    else {
        $("#div_LLPAIDDRIVER").hide();
    }
    //end LLpaiddriverlogic
    $("#prm_NTP").text(parseFloat(basictp) + totalcover);
    var totalpremium = $('.prem_' + productID).text();
    var Prem_GST = parseFloat(totalpremium) * parseFloat(0.18);
    var Prem_With_GST = parseFloat(Prem_GST.toFixed(0)) + parseFloat(totalpremium);
    $("#prm_TOT").text(totalpremium);
    $("#prm_Net").text(totalpremium);
    $("#prm_service").text(Prem_GST.toFixed(0));
    $("#prm_Final").text(Prem_With_GST);

});
 
 
	$('.datepicker, .drop-avoid').click(function(e) {
	    e.stopPropagation();
	});
 
 
var today = new Date(new Date().getFullYear(), new Date().getMonth(), new Date().getDate());
$('.startDate').datepicker({
    uiLibrary: 'bootstrap4',
    iconsLibrary: 'fontawesome',
    minDate: today,
    maxDate: function () {
        return $('.endDate').val();
    }
});
$('.endDate').datepicker({
    uiLibrary: 'bootstrap4',
    iconsLibrary: 'fontawesome',
    minDate: function () {
        return $('.startDate').val();
    }
});
 
var slider = document.getElementById("myRange");
var output = document.getElementById("txtIDV");
output.innerHTML = slider.value;

slider.oninput = function() {
    output.innerHTML = this.value;
} 