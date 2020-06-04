//global variables
let map;
var markersDic = {};
var markedId = "rand";
var flightPaths = {};

function initMap() {
    // Map options
    let options = {
        zoom: 8,
        center: { lat: 32.000061, lng: 34.870609 }
    }

    // create new map
    map = new google.maps.Map(document.getElementById('map'), options);
    //define map-on click event-reaset all display
    google.maps.event.addListener(map, 'click', function () {
        markedId = "rand";
        resetMarkers();
        showFlightDetails(markedId, "", "", "", "");
        deleteFlightRoute();
    });
    dragAndDrop();
}
//definding drag and drop events
function dragAndDrop() {
    var dropzone = document.getElementById('dropzone');
    dropzone.ondrop = function (e) {
        e.preventDefault();
        this.className = 'dropZone';
        uploadFiles(e.dataTransfer.files);
    }
    dropzone.ondragover = function () {
        this.className = 'dropZone dragover';
        return false;
    }
    dropzone.ondragleave = function () {
        this.className = 'dropZone';
        return false;
    }
}
//this function recieve relevant flights every second and updates the table accordingly
function loadingTable() {
    setInterval(function () {
        let date = new Date().toISOString().substr(0, 19) + "Z";
        let flightsUrl = "../api/flights?relative_to=" + date + "&sync_all";
        //send the request from server
        $.getJSON(flightsUrl, function (data) {
            //reset the table 
            $('#internalBody').empty();
            $('#externalBody').empty();
            //delete irrelevant flights(ended flights)
            deleteIrrelevantFlights(data);
            data.forEach(function (flight) {
                //for each flight-add marker and row
                addMarker(flight.flight_id, flight.latitude, flight.longitude);
                if (flight.is_extetanl) {
                    if (markedId == flight.flight_id)
                        $("#externalFlightTable").append('<tr onClick="rowClick(this)" style="background-color:#fff099"><td>' + flight.flight_id + "</td>" + "<td>" + flight.company_name + "</td>" + '></tr>');
                    else
                        $("#externalFlightTable").append('<tr onClick="rowClick(this)"><td>' + flight.flight_id + "</td>" + "<td>" + flight.company_name + "</td>" + '></tr>');
                } else {
                    if (markedId == flight.flight_id)
                        $("#internalFlightTable").append('<tr onClick="rowClick(this)" style="background-color:#fff099"><td>' + flight.flight_id + "</td>" + '<td>' + flight.company_name + "</td>" + "<td>" + '<td><button type="button" class="btn btn-danger" onClick="event.stopPropagation();DeleteFlight(\'' + flight.flight_id + '\')">X</button></td>></tr>');
                    else
                        $("#internalFlightTable").append('<tr onClick="rowClick(this)"><td>' + flight.flight_id + "</td>" + '<td>' + flight.company_name + "</td>" + "<td>" + '<td><button type="button" class="btn btn-danger" onClick="event.stopPropagation();DeleteFlight(\'' + flight.flight_id + '\')">X</button></td>></tr>');

                }
            });
        });

    }
        , 1000);
}
//this function define marker-on click event
//updates the appropriate row in the table according to the selected marker
function markerClick(id) {
    let internalTable = document.getElementById("internalFlightTable");
    let externalTable = document.getElementById("externalFlightTable");
    //pass on the internal flights table
    for (var i = 0, row; row = internalTable.rows[i]; i++) {
        if (row.cells[0].innerHTML === id) {
            row.style.backgroundColor = "#fff099";
        } else
            row.style.backgroundColor = "white";
    }
    //pass on the external flights table
    for (var i = 0, row; row = externalTable.rows[i]; i++) {
        if (row.cells[0].innerHTML === id) {
            row.style.backgroundColor = "#fff099";
        } else
            row.style.backgroundColor = "white";
    }
}
//this function define table row-on click event
function rowClick(row) {
    markedId = row.cells[0].innerHTML;
    resetMarkers();
    changeMarkerIcon(markedId);
    flightChoosen(markedId);
}
//delete flight from the server and display
function DeleteFlight(id) {
    fetch("../api/Flights/" + id, {
        method: 'DELETE',
    });
    deleteFlightMarks(id);
}
//gets the flight plan from the server according to the flight choosen id
function flightChoosen(id) {
    let flightsUrl = "../api/FlightPlan/" + id + "&";
    $.getJSON(flightsUrl, function (data) {
        showFlightDetails(id, data.passengers, data.company_name, data.initial_location.date_time, data.segments);
        drawFlightRoute(id, data.initial_location, data.segments);
    });
}
//displays flight details
function showFlightDetails(id, passengers, company_name, initialTime, segments) {
    let panelHeading = document.getElementById("panelHeading");
    //if there is no selected flight or flight is deleted- reset the display
    if (markedId == "rand" || id == "flightDeleted") {
        panelHeading.textContent = "Please choose a flight to view more details";
        company_nameP.textContent = "";
        passengersP.textContent = "";
        take_offP.textContent = "";
        landingP.textContent = "";
        panelBody = "";
    } else {
        let landingTime = calculateLandingTime(initialTime, segments);
        var takeOff = new Date(initialTime).toUTCString().substr(0, 25);
        panelHeading.textContent = "Flight: " + id;
        company_nameP.textContent = "Company Name: " + company_name;
        passengersP.textContent = "Passengers: " + passengers;
        take_offP.textContent = "Take off: " + takeOff;
        landingP.textContent = "Landing Time: " + landingTime;
    }
}
//draw flight route according to the latitude and longitude given
function drawFlightRoute(id, initial_location, segments) {
    var exist = false;
    //checking if there is already a route for this flight
    //and deletes the other routes from the map
    for (var key in flightPaths) {
        if (key == id) {
            exist = true;
            continue;
        } else
            flightPaths[key].setMap(null);
    }
    //if no flight route exists
    if (exist == false) {
        //creates array of coordinate
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
//adding new marker to map
function addMarker(id, lat, lng) {
    var icon = {
        url: '../css/icon.png', // url
        scaledSize: new google.maps.Size(20, 20), // scaled size
    }
    let marker = new google.maps.Marker({
        position: { lat, lng },
        icon: icon
    });
    //if there is a marker for this flight, it updates its location
    if (!markersDic[id]) {
        markersDic[id] = marker;
        markersDic[id].setMap(map);
    } else {
        updateMarkerPosition(id, lat, lng);
    }
    //define marker-on click event
    marker.addListener('click', function () {
        markerClick(id);
        resetMarkers();
        changeMarkerIcon(id);
        flightChoosen(id);
        markedId = id;
    });
}
//change marker icon to bold icon
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
//reset all markers icon to initial icon
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
//updates marker position according to latitude and longitude 
function updateMarkerPosition(id, lat, lng) {
    markersDic[id].setPosition({ lat, lng });
}
//delete flight route from map
function deleteFlightRoute() {
    for (var key in flightPaths)
        flightPaths[key].setMap(null);
}
//uploading files received from drag & drop
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
//send the flight plan to server
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
//calculate landing time according t initial location and segments
function calculateLandingTime(initialTime, segments) {
    let endlTime = new Date(initialTime);
    segments.forEach(function (segment) {
        endlTime.setSeconds(endlTime.getSeconds() + parseFloat(segment.timespan_Seconds));
    })
    return endlTime.toUTCString().substr(0, 25);
}
//delete all flight marks (marker, flight details and flight route)
function deleteFlightMarks(id) {
    markersDic[id].setMap(null);
    delete markersDic[id];
    for (var key in flightPaths) {
        if (key == id) {
            flightPaths[key].setMap(null);
            delete flightPaths[id];
        }
    }
    if (panelHeading.textContent == "Flight: " + id)
        showFlightDetails("flightDeleted", null, null, null, null);
}
//gets list of relevent flights from server and delets the irrelevant flights from display
function deleteIrrelevantFlights(flights) {
    for (var key in markersDic) {
        if (!flights.some(flight => flight.flight_id === key))
            deleteFlightMarks(key);
    }
}