const txtTodoId = mdc.textField.MDCTextField.attachTo(document.querySelector('#todoId'));
const txtTodoName = mdc.textField.MDCTextField.attachTo(document.querySelector('#todoName'));
const txtCreatedDate = mdc.textField.MDCTextField.attachTo(document.querySelector('#createdDate'));
const txtEstDate = mdc.textField.MDCTextField.attachTo(document.querySelector('#estDate'));
const txtDescription = mdc.textField.MDCTextField.attachTo(document.querySelector('#description'));
const isDoneSwitch = mdc.switchControl.MDCSwitch.attachTo(document.querySelector('.mdc-switch'));
const cbMoveToTOp = mdc.checkbox.MDCCheckbox.attachTo(document.querySelector('.mdc-checkbox'));
const alert = mdc.dialog.MDCDialog.attachTo(document.querySelector('.mdc-dialog'));
const isDone = document.getElementById("isDone");
/**  @@param e {MouseEvent} */
function isDoneChange(e){
    event.preventDefault();
    if(isDoneSwitch.selected == false){
        isDone.value = 1;
    }else{
        isDone.value = 0;
    }
}
function cancel(){
    event.preventDefault();
    window.location.href = "/Index";
}
window.addEventListener("load",function(){
    let alertType = document.getElementById("alertType").innerHTML;
    if(alertType == "1"){
        alert.open();
    }
});