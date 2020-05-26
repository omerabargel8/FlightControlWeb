function initMap() {
    // Map options
    let options = {
        zoom: 8,
        center: { lat: 42.3601, lng: -71.0589 }
    }

    // New map
    let map = new google.maps.Map(document.getElementById('map'), options);

    // Add marker
    let marker = new google.maps.Marker({
        position: { lat: 42.4668, lng: -70.9495 },
        map: map,
        icon: 'https://developers.google.com/maps/documentation/javascript/examples/full/images/beachflag.png'
    });
}
/**
async function blaa() {
    let flightsUrl = "../api/flights";
    $.getJSON(flightsUrl, function (data) {
        $('#internalFlightTable tr:gt(0)').remove()
        data.forEach(function (flight) {
            console.log(flight);
            //$("#flight_table").append("<tr><td>" + flight.flight_id + "</td>" + "<td>" + flight.company_name + "</td>" + "<td>" + flight.is_extetanl + "</td></tr>");
        });
    });
}
*/
