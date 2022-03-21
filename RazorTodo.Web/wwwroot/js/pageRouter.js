/** @type {string} */
const root = window.location.protocol + "//" + window.location.host;
let page = 1;
window.addEventListener('DOMContentLoaded', (e) => {
    redirect();
});

window.addEventListener('popstate', (e) => {
    redirect();
});

async function redirect() {
    let pageRe = /^[/]\d+$/;
    if (!window.location.href.includes("/#!/")) {
        window.location.href = "/#!/1";
        return;
    } else if (!window.location.href.split("/#!").slice(-1)[0].match(pageRe)) {
        window.location.href = "/#!/1";
        return;
    }
    let targetPath = window.location.href.split("/#!/").slice(-1)[0];
    page = Number(targetPath);
    if (page == 1) return;
    let html = await fetch(`/IndexAsync?p=${page + 1}`, {
        method: "GET",
    }).then(function (prop) {
        return prop.text();
    });
    let ulTodoItems = document.getElementById('ulTodoItems');
    ulTodoItems.innerHTML = html;
    let pageNumber = document.getElementById('pageNumber');
    pageNumber.value = page;
}