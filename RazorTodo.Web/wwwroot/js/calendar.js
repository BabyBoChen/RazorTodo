
class Dimension {

    constructor(w = 0, h = 0) {
        /** @type {Number}*/
        this.w = w;
        /** @type {Number}*/
        this.h = h;
    }
}

class Vector2D {

    constructor(x = 0, y = 0) {
        /** @type {Number}*/
        this.x = x;
        /** @type {Number}*/
        this.y = y;
    }
}

class CalendarDay {

    /** @param {Date} _date
    * @param {Boolean} isHoliday
    * @param {string} description */
    constructor(date = new Date(), isHoliday = false, description = null) {
        this._date = date;
        this.isHoliday = isHoliday;
        this.description = description;
    }

    /**@param {Date} _date */
    set date(_date) {
        this._date = _date;
    }

    /** @return {Date} */
    get date() {
        return this._date;
    }
}

class BBLJCalendar {

    constructor() {
        /** @type {HTMLCanvasElement} */
        this.canvas = null;
        /** @type {CanvasRenderingContext2D} */
        this.ctx = null;
        /** @type {Number}*/
        this.lineWidth = 1;
        /** @type {Number}*/
        this.width = 0;
        /** @type {Number}*/
        this.height = 0;
        /** @type {Number}*/
        this.columnWidth = 0;
        /** @type {Number}*/
        this.rowHeight = 0;
        /** @type {[string]}*/
        this.WEEKDAYS = ["Sun", "Mon", "Tue", "Wed", "Thr", "Fri", "Sat"];
        /** @type {Number}*/
        this._fontSize = 12;
        /** @type {Number}*/
        this.year = new Date().getUTCFullYear();
        /** @type {Number}*/
        this.month = new Date().getUTCMonth();
        /** @type {Date}*/
        this.firstDateOfThisCalendar = new Date(this.year, this.month, 1, 0, 0, 0);
        /** @type {[Number]}*/
        this.dates = [];
        /** @type {Number}*/
        this.howManyWeeksInThisCalendar = 5;
        /** @type {[CalendarDay]} */
        this._calendarDayInfos = [];
        /** @type {Boolean}*/
        this.isMouseOver = false;
        /** @type {Number}*/
        this.cursorX = 0;
        /** @type {Number}*/
        this.cursorY = 0;
        /** @type {Vector2D} */
        this.cursorPos = new Vector2D(0, 0);
        /** @type {Function} */
        this._onCalendarClick = new function () { };
    }

    /** @param canvasId {string}
    * @param {Number} width
    * @param {Number} height
    * @return {BBLJCalendar} */
    static init(canvasId, width, height) {
        let calendar = new BBLJCalendar();
        calendar.canvas = document.getElementById(canvasId);
        calendar.ctx = calendar.canvas.getContext('2d');
        calendar.setDay();
        calendar.width = width;
        calendar.columnWidth = calendar.width / 7;
        calendar.height = height;
        calendar.rowHeight = calendar.height / (calendar.howManyWeeksInThisCalendar + 1);
        calendar.canvas.addEventListener("mouseover", function () {
            calendar.isMouseOver = true;
            calendar.draw();
        });
        calendar.canvas.addEventListener("mouseout", function () {
            calendar.isMouseOver = false;
            calendar.draw();
        });
        calendar.canvas.addEventListener("resize", function () {
            console.log('resize');
        });
        calendar.canvas.addEventListener("mousemove", function (e) {
            let coord = new Vector2D(0, 0);
            let x = e.offsetX;
            let y = e.offsetY;
            coord.x = Math.floor(x / calendar.columnWidth);
            coord.y = Math.floor(y / calendar.rowHeight);
            calendar.cursorPos = coord;
            calendar.draw();
        });
        calendar.canvas.addEventListener("click", function (e) {
            let coord = new Vector2D(0, 0);
            let x = e.offsetX;
            let y = e.offsetY;
            coord.x = Math.floor(x / calendar.columnWidth);
            coord.y = Math.floor(y / calendar.rowHeight);
            if (coord.y == 0) {
                calendar.onCalendarClick(-1);
            } else {
                coord.y -= 1;
                let nthDate = coord.y * 7 + coord.x;
                calendar.onCalendarClick(calendar.dates[nthDate]);
            }
        });
        calendar.draw();
        return calendar;
    }

    /** @param {Number} px */
    set fontSize(px) {
        this._fontSize = px;
        this.draw();
    }

    /** @return {Number} */
    get fontSize() {
        return this._fontSize;
    }

    /** @param {Dimension} dimension */
    set size(dimension) {
        this.width = dimension.w;
        this.columnWidth = this.width / 7;
        this.height = dimension.h;
        this.rowHeight = this.height / (this.howManyWeeksInThisCalendar + 1);
        this.draw();
    }

    /** @param {Number} y
    * @param {Number} m */
    setYearAndMonth(y, m) {
        this.year = y;
        this.month = m;
        this.setDay();
        this.draw();
    }

    setDay() {
        let thisMonth = new Date(this.year, this.month, 1, 0, 0, 0);
        let firstDayOfThisMonth = thisMonth.getDay();
        let firstDateOfThisCalendar = new Date(this.year, this.month, 1 - firstDayOfThisMonth, 0, 0, 0);
        this.firstDateOfThisCalendar = firstDateOfThisCalendar;
        let howManyDaysInThisMonth = (new Date(this.year, this.month + 1, 0, 0, 0, 0)).getDate();

        if (firstDayOfThisMonth > 4 && howManyDaysInThisMonth > 30) {
            this.howManyWeeksInThisCalendar = 6;
        } else if (firstDayOfThisMonth > 5 && howManyDaysInThisMonth >= 30) {
            this.howManyWeeksInThisCalendar = 6;
        } else {
            this.howManyWeeksInThisCalendar = 5;
        }
        this.rowHeight = this.height / (this.howManyWeeksInThisCalendar + 1);

        this.dates = [];
        for (let i = 0; i < 7 * this.howManyWeeksInThisCalendar; i++) {
            let date = new Date(this.year, this.month, 1 - firstDayOfThisMonth + i, 0, 0, 0);
            this.dates.push(date);
        }
        //console.log(this.dates);
    }

    /** @param {[CalendarDay]} calendarDayInfos */
    set calendarDayInfos(calendarDayInfos) {
        this._calendarDayInfos = calendarDayInfos;
        this.setDay();
        this.draw();
    }

    /** @return {[CalendarDay]} */
    get calendarDayInfos() {
        return this._calendarDayInfos;
    }

    /** @param {Function} callback */
    set onCalendarClick(callback) {
        this._onCalendarClick = callback;
    }

    /** @return {Function} */
    get onCalendarClick() {
        return this._onCalendarClick;
    }

    draw() {
        this.canvas.style = null;

        this.ctx.clearRect(0, 0, this.width, this.height);
        this.canvas.width = this.width;
        this.canvas.height = this.height;
        this.ctx.lineWidth = this.lineWidth;
        this.ctx.font = `${this.fontSize}px Arial`;
        this.ctx.textAlign = 'center';
        this.ctx.textBaseline = "middle";

        //weekdays
        this.ctx.beginPath();
        this.ctx.fillStyle = '#f361af';
        this.ctx.fillRect(0, 0, this.width, this.rowHeight);
        this.ctx.closePath();
        this.ctx.fillStyle = 'white';
        for (let i = 0; i < 8; i++) {
            this.ctx.fillText(this.WEEKDAYS[i], this.columnWidth / 2 + this.columnWidth * i, this.rowHeight / 2, this.columnWidth);
        }

        //days
        /** @type {Date} */
        let currentDate = this.firstDateOfThisCalendar;
        for (let w = 1; w < this.howManyWeeksInThisCalendar + 1; w++) {

            let y = this.rowHeight / 2 + this.rowHeight * w;

            for (let d = 0; d < 7; d++) {

                if (currentDate.getMonth() == this.month) {
                    this.ctx.fillStyle = 'rgb(255, 255, 255)';
                } else {
                    this.ctx.fillStyle = 'rgb(220, 220, 220)';
                }

                if (this.cursorPos.x == d && this.cursorPos.y == w && this.isMouseOver) {
                    this.ctx.fillStyle = 'yellow';
                }

                this.ctx.fillRect(this.columnWidth * d, this.rowHeight * w, this.columnWidth, this.rowHeight);
                let x = this.columnWidth / 2 + this.columnWidth * d;
                this.ctx.fillStyle = 'black';

                let ctxRef = this.ctx;
                this.calendarDayInfos.forEach(function (dayInfo) {
                    if (dayInfo.date.getFullYear() == currentDate.getFullYear() &&
                        dayInfo.date.getMonth() == currentDate.getMonth() &&
                        dayInfo.date.getDate() == currentDate.getDate()) {
                        if (dayInfo.isHoliday) {
                            ctxRef.fillStyle = 'red';
                        }
                    }
                });

                this.ctx.fillText(currentDate.getDate(), x, y, this.columnWidth);
                currentDate = new Date(currentDate.getFullYear(), currentDate.getMonth(), currentDate.getDate() + 1, 0, 0, 0);
            }
        }

        //boundry
        this.ctx.beginPath();
        this.ctx.moveTo(0, 0);
        this.ctx.lineTo(this.width - this.lineWidth, 0);
        this.ctx.lineTo(this.width - this.lineWidth, this.height - this.lineWidth);
        this.ctx.lineTo(0, this.height - this.lineWidth);
        this.ctx.lineTo(0, 0);
        this.ctx.closePath();
        this.ctx.stroke();

        //columns
        for (let i = 1; i < 8; i++) {
            this.ctx.beginPath();
            this.ctx.moveTo(this.columnWidth * i, 0);
            this.ctx.lineTo(this.columnWidth * i, this.height);
            this.ctx.closePath();
            this.ctx.stroke();
        }

        //rows
        for (let i = 1; i < 7; i++) {
            this.ctx.beginPath();
            this.ctx.moveTo(0, this.rowHeight * i);
            this.ctx.lineTo(this.width, this.rowHeight * i);
            this.ctx.closePath();
            this.ctx.stroke();
        }

    }

    /** @param {BBLJCalendar} calendar */
    /** @param {Boolean} logFps */
    static renderCalendar(calendar, logFps) {
        let frame = 0;
        if (logFps) {
            window.setInterval(function () {
                console.log(frame);
                frame = 0;
            }, 1000);
        }
        window.setInterval(function () {
            calendar.draw();
            frame++;
        }, 1000 / 30);
    }
}

