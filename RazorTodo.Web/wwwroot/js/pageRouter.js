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

    //登入驗證
    let isAuthed = await fetch('/Login', {
        method: 'get'
    }).then(function (res) {
        return res.json();
    }).catch(function (err) {
        console.log(err);
        return { isAuthed: false };
    });
    if (!isAuthed.isAuthed) {
        let password = prompt("請輸入通關密語:", "");
        if (password == null) {
            return;
        }
        let formData = new FormData();
        formData.append("Password", password);
        let loginRes = await fetch('/Login', {
            method: 'post',
            body: formData,
        }).then(function (res) {
            return res.json();
        }).catch(function (err) {
            console.log(err);
            return { isAuthed: false };
        });
        if (!loginRes.isAuthed) {
            alert("請輸入正確的通關密語！");
            return;
        }
    }

    //下一頁
    let html = await fetch(`/IndexAsync?p=${page}`, {
        method: "GET",
    }).then(function (prop) {
        return prop.text();
    });
    let ulTodoItems = document.getElementById('ulTodoItems');
    ulTodoItems.innerHTML = html;
    let pageNumber = document.getElementById('pageNumber');
    pageNumber.value = page;
}