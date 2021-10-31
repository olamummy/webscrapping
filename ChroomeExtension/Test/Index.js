

generateFile.addEventListener("click", async () => {

    setPageBackgroundColor();
});

function setPageBackgroundColor() {
    var url = "";
    chrome.tabs.query({ active: true, lastFocusedWindow: true }, tabs => {
        url = tabs[0].url;

        var newURL = "https://localhost:44355/home/GenerateData/?url=" + url;
        chrome.tabs.create({ url: newURL });
    });
}

