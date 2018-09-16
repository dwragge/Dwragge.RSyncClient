function postData(url = ``, data = {}) {
    // Default options are marked with *
    return fetch(url, {
        method: "POST", // *GET, POST, PUT, DELETE, etc.
        mode: "cors", // no-cors, cors, *same-origin
        cache: "no-cache", // *default, no-cache, reload, force-cache, only-if-cached
        credentials: "same-origin", // include, same-origin, *omit
        headers: {
            "Content-Type": "application/json; charset=utf-8",
            // "Content-Type": "application/x-www-form-urlencoded",
        },
        referrer: "no-referrer", // no-referrer, *client
        body: JSON.stringify(data), // body data type must match "Content-Type" header
    })
}

function httpDelete(url = ``) {
    // Default options are marked with *
    return fetch(url, {
        method: "DELETE", // *GET, POST, PUT, DELETE, etc.
        mode: "cors", // no-cors, cors, *same-origin
        cache: "no-cache", // *default, no-cache, reload, force-cache, only-if-cached
        credentials: "same-origin", // include, same-origin, *omit
        headers: {
            "Content-Type": "application/json; charset=utf-8",
            // "Content-Type": "application/x-www-form-urlencoded",
        },
        referrer: "no-referrer", // no-referrer, *client
    })
}

function normalizeSlashes(url) {
    if (!url.startsWith("/")) {
        url = "/" + url
    }
    if (!url.endsWith("/")) {
        url = url + "/"
    }
    return url
}

export {
    postData,
    normalizeSlashes,
    httpDelete
}