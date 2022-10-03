const txtDescription = mdc.textField.MDCTextField.attachTo(document.querySelector('#description'));
const tbTodo = document.getElementById('tbTodo');

function expandDesc(div) {
    let cells = tbTodo.querySelectorAll('.cell');
    for (let i = 0; i < cells.length; i++) {
        cells[i].classList.remove('expand');
    }
    /** @type {HTMLDivElement} */
    let cell = div;
    cell.classList.add('expand');
}