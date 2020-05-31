
let map;
var markersDic = {};
var markedRowId = "rand";
var flightPaths = {};

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
        resetMarkers();
        showFlightDetails(markedRowId, "", "", "", "");
        deleteFlightRoute();
    });
    // Add marker
    let marker = new google.maps.Marker({
        position: { lat: 42.4668, lng: -70.9495 },
        map: map,
        icon: 'https://developers.google.com/maps/documentation/javascript/examples/full/images/beachflag.png'
    });
}
/**
function addMarker(id, lat, lng, selected) {
    //##################not in use
    if (selected == true) {
        //console.log(markersDic[Object.keys(markersDic)[id]]);
        var giveValue = function (id) {
            console.log(markersDic[id]);
            return markersDic[id];
        };
        console.log(markersDic[giveValue]);
        if (markersDic[id]) {
            console.log(markersDic[id]);
            console.log("ZZZZZ");
        }
        delete markersDic[id].icon;
        var newIcon = {
            url: '../css/markedIcon.png', // url
            scaledSize: new google.maps.Size(20, 20), // scaled size
        }
        markersDic[id]["icon"] = newIcon;
    }
    /**else {
        //if (!markersDic[id]) {
            console.log("CCCCCCCCCC");
            var icon = {
                url: '../css/icon.png', // url
                scaledSize: new google.maps.Size(20, 20), // scaled size
            }
            let marker = new google.maps.Marker({
                position: { lat, lng },
                map: map,
                icon: icon
            });
            markersDic[id] = marker;
            marker.addListener('click', function () {
                markedRowId = id;
                markerClick(id);
                flightChoosen(id);
            });
        //}
    }
}
*/
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
    console.log("###" + markedRowId);
    //let lat = markersDic[markedRowId].position.lat;
    //let lng = markersDic[markedRowId].position.lng;
    //delete markersDic[markedRowId];
    //addMarker(markedRowId, lat, lng, true);
    resetMarkers();
    changeMarkerIcon(markedRowId);
    flightChoosen(markedRowId);

}
function DeleteFlight(id) {
    fetch("../api/Flights/" + id, {
        method: 'DELETE',
    });
    markersDic[id].setMap(null);
    delete markersDic[id];
    for (var key in flightPaths) {
        if (key == id) {
            flightPaths[key].setMap(null);
            delete flightPaths[id];
        }
    }
    showFlightDetails("flightDeleted", null, null, null, null);
}
function flightChoosen(id) {
    let flightsUrl = "../api/FlightPlan/" + id + "&";
    $.getJSON(flightsUrl, function (data) {
        showFlightDetails(id, data.passengers, data.company_name, data.initial_location.date_time, data.segments);
        drawFlightRoute(id, data.initial_location, data.segments);
    });
}
function showFlightDetails(id, passengers, company_name, date_time, segments) {
    let panelHeading = document.getElementById("panelHeading");
    let panelBody = document.getElementById("panelBody");
    if (markedRowId == "rand" || id == "flightDeleted") {
        panelHeading.textContent = "Please choose a flight to view more details";
        company_nameP.textContent = "";
        passengersP.textContent = "";
        take_offP.textContent = "";
        landingP.textContent = "";
        panelBody = "";
    } else {
        var time = date_time.replace("T", "  ");
        panelHeading.textContent = "Flight: " + id;
        company_nameP.textContent = "Company Name: " + company_name;
        passengersP.textContent = "Passengers: " + passengers;
        take_offP.textContent = "Take off: " + time;
        landingP.textContent = "Flight: " + date_time;
    }
}
function drawFlightRoute(id, initial_location, segments) {
    var exist = false;
    for (var key in flightPaths) {
        if (key == id) {
            exist = true;
            continue;
        } else
            flightPaths[key].setMap(null);
    }
    if (exist == false) {
        var flightPlanCoordinates = [];
        flightPlanCoordinates[0] = { lat: initial_location.latitude, lng: initial_location.longitude };
        for (var i = 0, size = segments.length; i < size; i++) {
            flightPlanCoordinates[i + 1] = { lat: segments[i].latitude, lng: segments[i].longitude };
        }
        var flightPath = new google.maps.Polyline({
            path: flightPlanCoordinates,
            geodesic: false,
            strokeColor: '#FF0000',
            strokeOpacity: 1.0,
            strokeWeight: 2
        });
        flightPaths[id] = flightPath;
        flightPath.setMap(map);
    } else
        flightPaths[id].setMap(map);
}
function addMarker(id, lat, lng) {
    var icon = {
        url: '../css/icon.png', // url
        scaledSize: new google.maps.Size(20, 20), // scaled size
    }
    let marker = new google.maps.Marker({
        position: { lat, lng },
        icon: icon
    });
    if (!markersDic[id]) {
        markersDic[id] = marker;
        markersDic[id].setMap(map);
    } else {
        updateMarkerPosition(id, lat, lng);
    }
    marker.addListener('click', function () {
        markerClick(id);
        resetMarkers();
        changeMarkerIcon(id);
        flightChoosen(id);
        markedRowId = id;
    });
}
function changeMarkerIcon(id) {
    var newIcon = {
        url: '../css/markedIcon.png', // url
        scaledSize: new google.maps.Size(20, 20), // scaled size
    }
    delete markersDic[id].icon;
    markersDic[id].setMap(null);
    markersDic[id]["icon"] = newIcon
    markersDic[id].setMap(map);
}
function resetMarkers() {
    for (var key in markersDic) {
        var oldIcon = {
            url: '../css/icon.png', // url
            scaledSize: new google.maps.Size(20, 20), // scaled size
        }
        delete markersDic[key].icon;
        markersDic[key].setMap(null);
        markersDic[key]["icon"] = oldIcon
        markersDic[key].setMap(map);
    }
}
function updateMarkerPosition(id, lat, lng) {
    // console.log({ lat, lng });
    markersDic[id].setPosition({ lat, lng });
}
function deleteFlightRoute() {
    for (var key in flightPaths)
        flightPaths[key].setMap(null);
}
function uploadFiles(files) {
    for (i = 0; i < files.length; i++) {
        (function (file) {
            let fileReader = new FileReader();
            fileReader.readAsText(file, 'utf-8');
            fileReader.onload = function () { 
                let jsonFile = JSON.parse(fileReader.result);
                sendFile(jsonFile);   
            };
        })(files[i]);
    }
}
function sendFile(file) {
    let f = JSON.stringify(file);
    $.ajax({
        type: "POST",
        url: "api/FlightPlan",
        data: f,
        contentType: "application/json",
        error: function (XMLHttpRequest, textStatus, errorThrown) {
            alert("unable to upload json file");
        }

    });
}
