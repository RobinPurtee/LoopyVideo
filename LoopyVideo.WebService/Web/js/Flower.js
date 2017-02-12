var stateStrings = [
    "Unknown",
    "Error",
    "Play",
    "Stop",
    "Media"
];

function setButtonState(state) {
    if (state === 2 || state === 3) {
        $("#actionButton")
            /*.removeAttr("disabled")*/
            .Text(stateStrings[state]);
    }
    else {
        $("#actionButton")
            /*.attr("disabled", "disabled")*/
            .Text(stateStrings[state]);
    }

}


var flowerStatus;

function getFlowerState() {
    $.getJSON("/loopy/state", null, function (flowerState) {
        flowerStatus = flowerState.Command;
        $("#flowerStatus").Text(stateStrings[flowerState.Command]);
        setButtonState(flowerState.Command);
    });
}



$(function () {
    FlowerState();
    $("#actionButton").clicked(function () {
        var url = "/loopy/";

        if (flowerStatus === 2){ // Playing
            url += "stop";
        }
        else{
            url += "play";
        }
        $.post(url, null, function (data, status) {
            flowerStatus = data.Command;
            setButtonState(flowerStatus);
        });
    });
});