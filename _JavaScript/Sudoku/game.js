class Sudoku {
    constructor() {
        this.FreezeAllowed = ko.observable(true);
        this.SaveLoadInProcess = ko.observable(false);
        this.SerializedValue = ko.observable('');
        this.HasErrors = ko.observable(false);

        this.Field = Sudoku._createField();
        this._cellSets = Sudoku._createCellSets(this.Field);
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

    static fromSymbolToValue(s) {
        if ('0' < s && s <= '9')
            s -= '0';

        if (0 < s && s <= 9)
            return s;

        return 0;
    }

    static _createCellSets(field) {
        let result = {Rows: [], Cols: [], Squares: undefined};
        let squares = new Array(9).fill(0).map(x => []);

        for (let i = 0; i < 9; i++) {
            let row = [];
            let col = [];
            for (let j = 0; j < 9; ++j) {
                row.push(field[i][j]);
                col.push(field[j][i]);
                
                let squareNumber = Math.floor(i/3) + 3 * Math.floor(j/3);
                squares[squareNumber].push(field[i][j]);
            }
            result.Rows.push(new cellSet(row));
            result.Cols.push(new cellSet(col));
        }

        result.Squares = squares.map(x => new cellSet(x));
        return result;
    }

    Freeze() {
        if (!this.FreezeAllowed())
            return;

        this.FreezeAllowed(false);

        for (let x = 0; x < 9; ++x) {
            for (let y = 0; y < 9; ++y) {
                let cell = this.Field[x][y];
                if (Sudoku.fromSymbolToValue(cell.Value()) > 0)
                    cell.IsEditable(false);
            }
        }
    }

    SaveLoad() {
        let serializedValue = '';
        for (let x = 0; x < 9; ++x) {
            for (let y = 0; y < 9; ++y) {
                let value = Sudoku.fromSymbolToValue(this.Field[x][y].Value());
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
                let value = Sudoku.fromSymbolToValue(serializedValue[i++]);
                this.Field[x][y].Value(value === 0 ? '' : value);
            }
        }

        this.SaveLoadInProcess(false);
    }

    Check() {
        let isRowsGood = this._cellSets.Rows.map(x => x.check()).every(x => x === true);
        let isColsGood = this._cellSets.Cols.map(x => x.check()).every(x => x === true);
        let isSquaresGood = this._cellSets.Squares.map(x => x.check()).every(x => x === true);
        let isGood = isRowsGood && isColsGood && isSquaresGood;
        this.HasErrors(!isGood);
        if (isGood)
            alert('Great!');
    }
    
    HideErrors() {
        if (!this.HasErrors())
            return;

        for (let x = 0; x < 9; ++x)
            for (let y = 0; y < 9; ++y)
                this.Field[x][y].IsError(false);
            
        this.HasErrors(false);
    }
}

class cellSet {
    constructor(cells) {
        this._cells = cells;
    }

    check() {
        let hs = {};
        for (let i = 0; i < this._cells.length; ++i) {
            let cell = this._cells[i];
            let value = Sudoku.fromSymbolToValue(cell.Value());

            if (value === 0)
            {
                cell.IsError(true);
            } else if (hs[value] === undefined) {
                hs[value] = cell;
            } else {
                hs[value].IsError(true);
                cell.IsError(true);
            }
        }
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