/** @@type {HTMLDivElement} */
const menu_el = document.querySelector('.mdc-menu');
const menu = mdc.menu.MDCMenu.attachTo(document.querySelector('.mdc-menu'));
const alertModal = mdc.dialog.MDCDialog.attachTo(document.querySelector('.mdc-dialog'));
const alertMsg = document.getElementById("my-dialog-content");
function addTodo(){
    window.location.href="/TodoDetail?m=a";
}
async function save(){
    let isAuthed = await fetch('/Login', {
        method:'get'
    }).then(function (res) {
        return res.json();
    }).catch(function (err) {
        console.log(err);
        return { isAuthed : false};
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
            return {isAuthed : false};
        });

        if (!loginRes.isAuthed) {
            alert("請輸入正確的通關密語！");
            return;
        }
    }

    let hasModified = false;
    /** @@type {HTMLFormElement} */
    let frmTodo = document.getElementById("frmTodo");
    let todos = document.querySelectorAll(".hidden-group");
    todos.forEach(function(todo){
        let todoId = todo.querySelector(`input[name='TodoId']`);
        let isDone = todo.querySelector(`input[name='IsDone']`);
        let rowState = todo.querySelector(`input[name='RowState']`);
        let rowStateInt = parseInt(rowState.value);
        if(rowStateInt == 16 || rowStateInt == 8){
            frmTodo.append(todoId);
            frmTodo.append(isDone);
            frmTodo.append(rowState);
            hasModified = true;
        }
    });
    if(hasModified){
        frmTodo.submit();
    }
}
/** 
@param div {HTMLDivElement} 
@param e {MouseEvent}
*/
function showMenu(div, e){
    e.preventDefault();
    e.stopPropagation();
    let id = div.dataset.id;
    menu.open = true;
    menu.todoId = id;
    let menuWidth = menu_el.offsetWidth;
    let menuHeight = menu_el.offsetHeight;
    let screenWidth = window.innerWidth;
    let screenHeight = window.innerHeight;
    let left = e.clientX;
    let top = e.clientY;
    if(top + menuHeight > screenHeight - 150){
        top = e.clientY - menuHeight - 20;
    }
    menu.setAbsolutePosition(left, top);
    let pos = {
        menuWidth: menuWidth,
        menuHeight : menuHeight,
        screenWidth : screenWidth,
        screenHeight : screenHeight,
        left : left,
        top : top,
        clientX : e.clientX,
        clientY : e.clientY,
    };
    //console.log(pos);
}
/** 
@param item {HTMLUListElement} 
*/
function itemClicked(item){
    let id = item.dataset.id;
    window.location.href =`TodoDetail?id=${id}&m=e`;
}

function markDone(){
    let id = menu.todoId;
    let leading = document.getElementById(`leading_${id}`);
    let hidden = document.getElementById(`isDone_${id}`);
    let rowState = document.getElementById(`rowState_${id}`);
    if(hidden.value == 0){
        hidden.value = 1;
        rowState.value = 16;
        leading.innerHTML = `<i class="fas fa-check-circle fa-3x" style="color:green;"></i>`;
    }
}

function deleteTodo() {
    let id = menu.todoId;
    let leading = document.getElementById(`leading_${id}`);
    let hidden = document.getElementById(`isDone_${id}`);
    let rowState = document.getElementById(`rowState_${id}`);
    rowState.value = 8;
    leading.innerHTML = `<i class="fas fa-times-circle fa-3x" style="color:red;"></i>`;
}

//Get the button:
btnGoTop = document.getElementById("btnGoTop");

function scrollFunction() {
    if (document.body.scrollTop > 20 || document.documentElement.scrollTop > 20) {
        btnGoTop.style.display = "block";
    } else {
        btnGoTop.style.display = "none";
    }
}

// When the user clicks on the button, scroll to the top of the document
function topFunction() {
    document.body.scrollTop = 0; // For Safari
    document.documentElement.scrollTop = 0; // For Chrome, Firefox, IE and Opera
}

// When the user scrolls down 20px from the top of the document, show the button
window.onscroll = function() {scrollFunction()};

window.addEventListener("load",function(){
    let alertType = document.getElementById("alertType").innerHTML;
    if(alertType == "1"){
        alertModal.open();
    }
    if (alertType == "2") {
        alertMsg.innerHTML = "儲存失敗！請輸入正確的通關密語！";
        alertModal.open();
    }
});