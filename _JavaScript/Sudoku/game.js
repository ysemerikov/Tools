class Sudoku {
    constructor() {
        this.FreezeAllowed = ko.observable(true);

        this.Field = Sudoku._createField();
    }

    static _createField() {
        let field = new Array(9);

        for (let x = 0; x < field.length; ++x) {
            field[x] = new Array(9);
            for (let y = 0; y < 9; ++y) {
                field[x][y] = new cell(x, y);
            }
        }

        return field;
    }

    static _fromSymbolToValue(s) {
        if ('0' < s && s <= '9')
            s -= '0';

        if (0 < s && s <= 9)
            return s;

        return undefined;
    }

    Freeze() {
        if (!this.FreezeAllowed())
            return;

        this.FreezeAllowed(false);
    }
}

class cell {
    constructor(position_x, position_y) {
        this.position_x = position_x;
        this.position_y = position_y;

        this.Value = ko.observable();
        this.IsEditable = ko.observable(true);
        this.IsError = ko.observable(false);
    }
}

window.addEventListener("load", start, false);

function start(e) {
    let sudoku = new Sudoku();

    // document.addEventListener("keyup", e => sudoku.Keyup(e), false);

    ko.applyBindings(sudoku);
}