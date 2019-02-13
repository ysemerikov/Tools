class Sudoku {
    constructor() {
        this.FreezeAllowed = ko.observable(true);
        this.SaveLoadInProcess = ko.observable(false);
        this.SerializedValue = ko.observable('');

        this.Field = Sudoku._createField();
    }

    static _createField() {
        let field = new Array(9);

        for (let x = 0; x < 9; ++x) {
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

        return 0;
    }

    Freeze() {
        if (!this.FreezeAllowed())
            return;

        this.FreezeAllowed(false);

        for (let x = 0; x < 9; ++x) {
            for (let y = 0; y < 9; ++y) {
                let cell = this.Field[x][y];
                if (Sudoku._fromSymbolToValue(cell.Value()) > 0)
                    cell.IsEditable(false);
            }
        }
    }

    SaveLoad() {
        let serializedValue = '';
        for (let x = 0; x < 9; ++x) {
            for (let y = 0; y < 9; ++y) {
                let value = Sudoku._fromSymbolToValue(this.Field[x][y].Value());
                serializedValue += value;
            }
        }

        this.SerializedValue(serializedValue);
        this.SaveLoadInProcess(!this.SaveLoadInProcess());
    }

    Load() {
        if (!this.SaveLoadInProcess())
            return;

        let serializedValue = this.SerializedValue().replace(/\D+/g, '');
        let i = 0;
        for (let x = 0; x < 9; ++x) {
            for (let y = 0; y < 9; ++y) {
                let value = Sudoku._fromSymbolToValue(serializedValue[i++]);
                this.Field[x][y].Value(value === 0 ? '' : value);
            }
        }

        this.SaveLoadInProcess(false);
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