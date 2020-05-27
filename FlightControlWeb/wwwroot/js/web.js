let map;
var markersDic = {};
var markedRowId = "rand";

function initMap() {
    // Map options
    let options = {
        zoom: 8,
        center: { lat: 42.3601, lng: -71.0589 }
    }

    // New map
    map = new google.maps.Map(document.getElementById('map'), options);
    google.maps.event.addListener(map, 'click', function () {
        markedRowId = "rand";
        showFlightDetails(markedRowId, "", "", "", "");
    });
    // Add marker
    let marker = new google.maps.Marker({
        position: { lat: 42.4668, lng: -70.9495 },
        map: map,
        icon: 'https://developers.google.com/maps/documentation/javascript/examples/full/images/beachflag.png'
    });


}
function addMarker(id, lat, lng, selected) {
    if (selected == true || id == markedRowId) {
        var icon = {
            url: '../css/markedIcon.png', // url
            scaledSize: new google.maps.Size(20, 20), // scaled size
        }
    }else {
        var icon = {
            url: '../css/icon.png', // url
            scaledSize: new google.maps.Size(20, 20), // scaled size
        }
    }
    let marker = new google.maps.Marker({
        position: {lat, lng},
        map: map,
        icon: icon
    });
    markersDic[id] = marker;
    marker.addListener('click', function () {
        markedRowId = id;
        markerClick(id);
        flightChoosen(id);
    });
}
function markerClick(id) {
    let internalTable = document.getElementById("internalFlightTable");
    let externalTable = document.getElementById("externalFlightTable");
    for (var i = 0, row; row = internalTable.rows[i]; i++) {
        if (row.cells[0].innerHTML === id) {
            //delete this
            row.style.backgroundColor = "aquamarine";
        } else 
            row.style.backgroundColor = "white";
    }
    for (var i = 0, row; row = externalTable.rows[i]; i++) {
        if (row.cells[0].innerHTML === id) {
            //delete this
            row.style.backgroundColor = "aquamarine";
        } else
            row.style.backgroundColor = "white";
    }
}
function rowClick(row) {
    markedRowId = row.cells[0].innerHTML;
    let lat = markersDic[markedRowId].position.lat;
    let lng = markersDic[markedRowId].position.lng;
    delete markersDic[markedRowId];
    addMarker(markedRowId, lat, lng, true);
    flightChoosen(markedRowId);
}
function flightChoosen(id) {
    let flightsUrl = "../api/FlightPlan/" + id + "&";
    $.getJSON(flightsUrl, function (data) {
        showFlightDetails(id, data.passengers, data.company_name, data.initial_location.date_time, data.segments);
        drawFlightRoute(data.initial_location, data.segments);
    });
}

function showFlightDetails(id, passengers, company_name, date_time, segments) {
    let panelHeading = document.getElementById("panelHeading");
    let panelBody = document.getElementById("panelBody");
    if (markedRowId == "rand") {
        panelHeading.textContent = "Please choose a flight to view more details";
        company_nameP.textContent = "";
        passengersP.textContent = "";
        take_offP.textContent = "";
        landingP.textContent = "";
        panelBody = "";
    } else {
        panelHeading.textContent = "Flight: " + id; 
        company_nameP.textContent = "Company Name: " + company_name;
        passengersP.textContent = "Passengers: " + passengers;
        take_offP.textContent = "Take off: " + date_time;
        landingP.textContent = "Flight: " + date_time; 
    }
}
function drawFlightRoute() {

}