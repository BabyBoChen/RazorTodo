const txtPassword = mdc.textField.MDCTextField.attachTo(document.querySelector('#txtPassword'));
function cancel() {
    event.preventDefault();
    window.location.href = "/";
}