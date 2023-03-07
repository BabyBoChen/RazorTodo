class BBLJCalendar {

    //public:

    /** @type {BBLJCalendarConfig} */
    config = new BBLJCalendarConfig();
    /** 
     * @param canvas {HTMLCanvasElement} 
     * @param config {BBLJCalendarConfig}
     * @returns {BBLJCalendar}
     * */
    static mount(canvas, config) {
        let calendar = new BBLJCalendar();
        calendar.#canvas = canvas;
        calendar.#ctx = canvas.getContext('2d');
        if (config) {
            calendar.config = config;
        }
        calendar.#canvas.addEventListener("mousemove", function () {
            calendar.#onMouseMove.bind(calendar)(event);
        });
        calendar.#canvas.addEventListener("mouseleave", function () {
            calendar.#onMouseLeave.bind(calendar)(event);
        });
        calendar.#canvas.addEventListener("click", function () {
            let d = calendar.#getClickedDay.bind(calendar)();
            //console.log(d);
            calendar.config.onDateClicked(d);
        });
        window.addEventListener("resize", function () {
            //console.log("resize");
            calendar.#draw();
        });
        calendar.updateCalendar();  //initial render
        return calendar;
    }
    //function: call this function to refresh the calendar
    updateCalendar() {
        this.#populateCalendarDates();
        this.#draw();
    }
    /** @param {Number} y @param {Number} m */
    setYearMonth(y, m) {
        this.config.year = y;
        this.config.month = m;
        this.updateCalendar();
    }

    //private:

    /** @type {HTMLCanvasElement} */
    #canvas = null;
    /** @type {CanvasRenderingContext2D} */
    #ctx = null;
    /** @type {Dimension2D}*/
    #size = new Dimension2D(300, 150);
    /** @type {Number}*/
    #columnWidth = 0;
    /** @type {Number}*/
    #rowHeight = 0;
    /** @type {Number}*/
    #weekCount = 6;
    /** @type {[string]}*/
    #weekDays = ["Sun", "Mon", "Tue", "Wed", "Thr", "Fri", "Sat"];
    /** @type {[BBLJCalendarDateInfo]} */
    #calendarDates = [];
    /** @type {Vector2D?} */
    #cursorPos = null;
    //function
    #populateCalendarDates() {
        this.#calendarDates = [];
        /** @type {Date} */
        let firstDateOfTheCalendar = this.#getFirstDateOfTheCalendar();
        for (let i = 0; i < 42; i++) {
            let d = new BBLJCalendarDateInfo();
            d.date = new Date(firstDateOfTheCalendar.getFullYear(), firstDateOfTheCalendar.getMonth(), firstDateOfTheCalendar.getDate());
            d.date.setDate(firstDateOfTheCalendar.getDate() + i);
            //ignore the 6th week if the month does not have 6 weeks
            if (d.date.getDay() == 0) {
                let nextMonth = (new Date(this.config.year, this.config.month + 1, 1)).getMonth();
                if (d.date.getMonth() + 1 == nextMonth) {
                    break;
                }
            }
            //mapping the date with the calendar date info that user provided (config.dateInfos)
            if (this.config.dateInfos != null && Array.isArray(this.config.dateInfos) && this.config.dateInfos.length > 0) {
                //console.log(d);
                for (let j = 0; j < this.config.dateInfos.length; j++) {
                    let dateInfo = this.config.dateInfos[j];
                    //console.log(dateInfo);
                    let dateOfdateInfo = new Date(dateInfo.date.toDateString());
                    if (d.date.getTime() == dateOfdateInfo.getTime()) {
                        d.description = dateInfo.description;
                        d.isHoliday = dateInfo.isHoliday;
                        break;
                    }
                }
            }
            this.#calendarDates.push(d);
        }
        //exclude the 6th week if the month does not have 6 weeks
        if (this.#calendarDates.length < 36) {
            this.#weekCount = 5;
        } else {
            this.#weekCount = 6;
        }
    }
    /** @return {Date} */
    #getFirstDateOfTheCalendar() {
        let firstDateOfTheMonth = new Date(this.config.year, this.config.month - 1, 1);
        let firstDateOfTheCalendar = firstDateOfTheMonth;
        let firstDayOfTheMonth = firstDateOfTheMonth.getDay();
        for (let i = 0; i < firstDayOfTheMonth; i++) {
            firstDateOfTheCalendar.setDate(firstDateOfTheCalendar.getDate() - 1);
        }
        return firstDateOfTheCalendar;
    }
    //function
    #draw() {
        //1. Clear All
        this.#ctx.clearRect(0, 0, this.#canvas.offsetWidth, this.#canvas.offsetHeight);
        //2. Calculate width and height
        this.#size.w = this.#canvas.offsetWidth;
        this.#size.h = this.#canvas.offsetHeight;
        this.#canvas.width = this.#size.w;
        this.#canvas.height = this.#size.h;
        this.#columnWidth = this.#canvas.width / 7;
        this.#rowHeight = this.#canvas.height / (this.#weekCount + 1);  // + 1 for Headers (WeekDays)
        //3. draw
        this.#drawWeekDays();
        this.#drawCalendarDates();
        this.#drawBorder();
    }
    //function
    #drawWeekDays() {
        this.#ctx.fillStyle = '#f361af';
        this.#ctx.fillRect(0, 0, this.#size.w, this.#rowHeight);
        this.#ctx.fillStyle = 'white';
        this.#ctx.font = `${this.config.fontSize} Arial`;
        this.#ctx.textAlign = 'center';
        this.#ctx.textBaseline = "middle";
        for (let i = 0; i < this.#weekDays.length; i++) {
            this.#ctx.fillText(this.#weekDays[i], this.#columnWidth / 2 + this.#columnWidth * i, this.#rowHeight / 2, this.#columnWidth);
        }
    }
    //function
    #drawCalendarDates() {
        for (let i = 0; i < this.#calendarDates.length; i++) {
            let theDate = this.#calendarDates[i].date;
            let theDay = theDate.getDay();
            let week = Math.ceil((i + 1) / 7);
            let x = (theDay) * this.#columnWidth;
            let y = this.#rowHeight * (week);
            this.#ctx.fillStyle = 'white';
            if (theDate.getMonth() + 1 != this.config.month) {
                this.#ctx.fillStyle = 'rgb(220, 220, 220)';
            }
            if (this.#cursorPos != null) {
                if (this.#cursorPos.x == theDay && this.#cursorPos.y == week) {
                    this.#ctx.fillStyle = 'yellow';
                }
            }
            this.#ctx.fillRect(this.#columnWidth * theDay, this.#rowHeight * week, this.#columnWidth, this.#rowHeight);
            this.#ctx.fillStyle = 'black';
            if (this.#calendarDates[i].isHoliday) {
                this.#ctx.fillStyle = 'red';
            }
            this.#ctx.fillText(theDate.getDate(), x + this.#columnWidth / 2, y + this.#rowHeight / 2, this.#columnWidth);
        }
    }
    //function
    #drawBorder() {
        this.#ctx.moveTo(0 + this.#ctx.lineWidth, 0 + this.#ctx.lineWidth);
        this.#ctx.beginPath();
        this.#ctx.lineTo(this.#size.w - this.#ctx.lineWidth, 0 + this.#ctx.lineWidth);
        this.#ctx.lineTo(this.#size.w - this.#ctx.lineWidth, this.#size.h - this.#ctx.lineWidth);
        this.#ctx.lineTo(0 + this.#ctx.lineWidth, this.#size.h - this.#ctx.lineWidth);
        this.#ctx.lineTo(0 + this.#ctx.lineWidth, 0 + this.#ctx.lineWidth);
        this.#ctx.closePath();
        this.#ctx.stroke();
        //columns
        for (let i = 1; i < 8; i++) {
            this.#ctx.beginPath();
            this.#ctx.moveTo(this.#columnWidth * i, 0);
            this.#ctx.lineTo(this.#columnWidth * i, this.#size.h);
            this.#ctx.closePath();
            this.#ctx.stroke();
        }
        //rows
        for (let i = 1; i < this.#weekCount + 1; i++) {
            this.#ctx.beginPath();
            this.#ctx.moveTo(0, this.#rowHeight * i);
            this.#ctx.lineTo(this.#size.w, this.#rowHeight * i);
            this.#ctx.closePath();
            this.#ctx.stroke();
        }
    }
    /** @param {MouseEvent} e */
    #onMouseMove(e) {
        let coord = new Vector2D(0, 0);
        let x = e.offsetX;
        let y = e.offsetY;
        coord.x = Math.floor(x / this.#columnWidth);
        coord.y = Math.floor(y / this.#rowHeight);
        this.#cursorPos = coord;
        this.#draw();
    }
    /** @param {MouseEvent} e */
    #onMouseLeave(e) {
        this.#cursorPos = null;
        this.#draw();
    }
    /** @returns {BBLJCalendarDateInfo} */
    #getClickedDay() {
        let clickedDateInfo = null;
        //if cusor is inside the canvas
        if (this.#cursorPos != null) {
            let i = (this.#cursorPos.y - 1) * 7 + this.#cursorPos.x;
            //if user clicked on a date, not on the headers (i.e. Sun, Mon, Tue...)
            if (i >= 0) {
                let clickedDate = this.#calendarDates[i].date;
                if (this.config.dateInfos != null && Array.isArray(this.config.dateInfos) && this.config.dateInfos.length > 0) {
                    for (let j = 0; j < this.config.dateInfos.length; j++) {
                        let dateOfDateInfo = new Date(this.config.dateInfos[j].date.toDateString());
                        if (dateOfDateInfo.getTime() == clickedDate.getTime()) {
                            clickedDateInfo = this.config.dateInfos[j];
                            break;
                        }
                    }
                }
            }
        }
        return clickedDateInfo;
    }
}

class BBLJCalendarConfig {
    /** @type {Number} */
    year = (new Date().getFullYear());
    /** @type {Number} */
    month = (new Date().getMonth());
    /** @type {String} */
    fontSize = "1.25em";
    /** @type {[BBLJCalendarDateInfo]} */
    dateInfos = [];
    //function
    onDateClicked = /** @param calendarDay {BBLJCalendarDateInfo} */ function (calendarDay) {

    }
    constructor() { }
}

class BBLJCalendarDateInfo {
    /** @type {Date} */
    date = new Date();
    /** @type {Boolean} */
    isHoliday = false;
    /** @type {String} */
    description = "";
    constructor() { }
}

class Dimension2D {
    /** @type {Number} */
    w = 0;
    /** @type {Number} */
    h = 0;
    constructor(w = 0, h = 0) {
        this.w = w;
        this.h = h;
    }
}

class Vector2D {
    /** @type {Number} */
    x = 0;
    /** @type {Number} */
    y = 0;
    constructor(x = 0, y = 0) {
        this.x = x;
        this.y = y;
    }
}

//export { BBLJCalendar, BBLJCalendarConfig, BBLJCalendarDateInfo }