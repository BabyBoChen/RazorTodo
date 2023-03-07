const Jatatable = {
	/**
	 * @param {HTMLTableElement} tb
	 * @param {object} settings
	 */
	init : function(tb, settings){
		
		tb.classList.add("jatatable");
		tb.jatatableSettings = settings;
		
		if(!settings.autoColumnWidth){
			Jatatable._setJatatableDefaultColumnWidth(tb.jatatableSettings.defaultColumnWidth);
		}
		
		let colHeaders = tb.querySelectorAll("th");
		for(let i = 0; i < colHeaders.length; i++){
			let colHeader = colHeaders[i];
			colHeader.classList.add("jatatable-th");
			if(settings.autoColumnWidth){
				colHeader.classList.add("auto-column-width");
				if (!colHeader.classList.contains("unbound")) {
					Jatatable._autoColumnWidth(colHeader);
				}
			}
			let resizer = document.createElement("div");
			resizer.classList.add("resizer");
			if(i == colHeaders.length - 1){
				resizer.classList.add("resizer-last");
			}
			resizer.addEventListener("mousedown", function(e){
				Jatatable._resizerMouseDown(tb, resizer, e);
			});
			resizer.addEventListener("touchstart", function(e){
				Jatatable._resizerTouchStart(tb, resizer, e);
			});
			colHeader.append(resizer);
		}
		
		tb.addEventListener("mousemove", function(e){
			Jatatable._resizerMouseMove(tb, e);
		});
		tb.addEventListener("touchmove", function(e){
			Jatatable._resizerTouchMove(tb, e);
		});
		tb.addEventListener("mouseup", function(e){
			Jatatable._resizerMouseUp(tb, e);
		});
		tb.addEventListener("touchend", function(e){
			Jatatable._resizerTouchEnd(tb, e);
		});
		tb.addEventListener("mouseleave", function(e){
			Jatatable._resizerMouseLeave(tb,e);
		});
	},
	
	/**
	 * @param {Number} defaultColumnWidth
	 */
	_setJatatableDefaultColumnWidth(defaultColumnWidth){
		if(!defaultColumnWidth){
			defaultColumnWidth = 100;
		}
		let root = document.querySelector(':root');
		root.style.setProperty('--jatatable-default-column-width', `${defaultColumnWidth}px`);
	},
	
	/**
	 * @param {HTMLTableCellElement} th
	 */
	_autoColumnWidth : function(th){
		let textWidth = Jatatable._calcTextWidth(th.innerText, th.parentElement);
		th.style.width = (textWidth) + "px";
	},
	
	/**
	 * @param {String} text
	 * @param {HTMLTableCellElement} parentElement
	 * @returns {Number}
	 */
	_calcTextWidth : function(text, parentElement){
		let span = document.createElement("span");
		span.innerHTML = text;
		span.style.position = "absolute";
		span.style.visibility = "hidden";
		span.style.height = "auto";
		span.style.width = "auto";
		span.style.whiteSpace = "nowrap";
		parentElement.append(span);
		let textWidth = span.clientWidth;
		span.remove();
		return textWidth;
	},
	
	/**
	 * @param {HTMLTableElement} tb
	 * @param {HTMLDivElement} s
	 * @param {MouseEvent} e
	 */
	 _resizerMouseDown : function(tb, s, e){
		tb.resizer = s;
		tb.x0 = e.screenX;
	},
	
	/**
	 * @param {HTMLTableElement} tb
	 * @param {HTMLDivElement} s
	 * @param {TouchEvent} e
	 */
	 _resizerTouchStart : function(tb, s, e){
		tb.resizer = s;
		tb.x0 = e.touches[0].screenX;
	},
	
	/** 
	 * @param {HTMLTableElement} s
	 * @param {MouseEvent} e
	 */
	 _resizerMouseMove : function(s, e){
		if(s.resizer){
			let deltaX = e.screenX - s.x0;
			let col = s.resizer.parentElement;
			let colStyle = getComputedStyle(col);
			let colWidth = parseInt(colStyle.width);
			col.style.maxWidth = colWidth + deltaX + "px";
			col.style.width = colWidth + deltaX + "px";
			s.x0 = e.screenX;
		}
	},
	
	/** 
	 * @param {HTMLTableElement} s
	 * @param {TouchEvent} e
	 */
	 _resizerTouchMove : function(s, e){
		if(s.resizer){
			e.preventDefault();
			let deltaX = e.touches[0].screenX - s.x0;
			let col = s.resizer.parentElement;
			let colStyle = getComputedStyle(col);
			let colWidth = parseInt(colStyle.width);
			col.style.maxWidth = colWidth + deltaX + "px";
			col.style.width = colWidth + deltaX + "px";
			s.x0 = e.touches[0].screenX;
		}
	},
	
	/** 
	 * @param {HTMLTableElement} s
	 * @param {MouseEvent} e
	 */
 	_resizerMouseUp : function(s, e){
		s.resizer = null;
	},
	
	/** 
	 * @param {HTMLTableElement} s
	 * @param {TouchEvent} e
	 */
	_resizerTouchEnd : function (s, e) {
		s.resizer = null;
	},
	
	/** 
	 * @param {HTMLTableElement} s
	 * @param {MouseEvent} e
	 */
	_resizerMouseLeave : function (s, e){
		s.resizer = null;
	},
	
	/** 
	 * @param {HTMLTableElement} s
	 * @param {String} header
	 * @returns {[HTMLTableRowElement]}
	 */
	insertColumn : function(s, header){
		
		let thRow = s.querySelector("thead tr");
		let th = document.createElement("th");
		thRow.append(th);
		
		th.classList.add("jatatable-th");
		th.innerText = header;
		if(s.jatatableSettings.autoColumnWidth){
			th.classList.add("auto-column-width");
			let textWidth = Jatatable._calcTextWidth(header, th);
			th.style.width = (textWidth) + "px";
		}
		if(document.querySelector(".resizer-last")) {
			document.querySelector(".resizer-last").classList.remove("resizer-last");
		}
		
		let resizer = document.createElement("div");
		resizer.classList.add("resizer");
		resizer.classList.add("resizer-last");
		resizer.addEventListener("mousedown", function(e){
			Jatatable._resizerMouseDown(s, resizer, e);
		});
		resizer.addEventListener("touchstart", function(e){
			Jatatable._resizerTouchStart(s, resizer, e);
		});
		th.append(resizer);
		
		let rows = s.querySelectorAll("tbody tr");
		rows.forEach(function(row){
			let td = document.createElement("td");
			row.append(td);
		});
		return rows;
	},
	
	/** @param {HTMLTableElement} s
	 * @param {Number} colIndex 
	*/
	deleteColumn : function(s, colIndex){
		let cols = s.querySelectorAll("th");
		cols[colIndex].remove();
		let rows = s.querySelectorAll("tbody tr");
		rows.forEach(function(row){
			let cells = row.querySelectorAll("td");
			cells[colIndex].remove();
		});
	}
};