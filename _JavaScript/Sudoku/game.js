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
        let g = () => new Array(9).fill(0).map(x => new cellSet());
        let result = {Rows: g(), Cols: g(), Squares: g(), All:[]};

        for (let i = 0; i < 9; ++i) {
            result.All.push(result.Rows[i]);
            result.All.push(result.Cols[i]);
            result.All.push(result.Squares[i]);
        }

        for (let i = 0; i < 9; i++) {
            for (let j = 0; j < 9; ++j) {
                let rowSet = result.Rows[i];
                let colSet = result.Cols[i];
                let squareSet = result.Squares[(Math.floor(i/3) + 3 * Math.floor(j/3))];

                rowSet.add(field[i][j]);
                // field[i][j].setRowSet(rowSet);

                colSet.add(field[j][i]);
                // field[j][i].setColSet(colSet);

                squareSet.add(field[i][j]);
                // field[i][j].setSquareSet(squareSet);
            }
        }

        return result;
    }

    Freeze() {
        if (!this.FreezeAllowed())
            return;

        this.FreezeAllowed(false);

        for (let x = 0; x < 9; ++x) {
            for (let y = 0; y < 9; ++y) {
                let cell = this.Field[x][y];
                if (cell.getNumericValue() > 0)
                    cell.IsEditable(false);
            }
        }
    }

    SaveLoad() {
        let serializedValue = '';
        for (let x = 0; x < 9; ++x) {
            for (let y = 0; y < 9; ++y)
                serializedValue += this.Field[x][y].getNumericValue();
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
        let isGood = this._cellSets.All.map(x => x.check()).every(x => x === true);
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

    Resolve1() {
        let allSets = this._cellSets.All;
        for (let i = 0; i < allSets.length; ++i) {
            let cellSet = allSets[i];
            let sudokuValues = cellSet.getSudokuValues();
            if (sudokuValues.length !== 8)
                continue;

            let emptyCells = cellSet._cells.filter(x => x.getNumericValue() === 0);
            if (emptyCells.length !== 1)
                continue;

            emptyCells[0].Value(45 - sudokuValues.reduce((a, b) => a + b, 0));
        }
    }
}

class cellSet {
    constructor() {
        this._cells = [];
    }
    
    add(cell) {
        if (this._cells.length === 9)
            throw "cell set is overloaded.";
        this._cells.push(cell);
    }

    getNumericValues() {
        return this._cells.map(x => x.getNumericValue());
    }

    getSudokuValues() {
        return Array.from(new Set(this.getNumericValues().filter(x => x > 0)));
    }

    check() {
        let hs = {};
        for (let i = 0; i < this._cells.length; ++i) {
            let cell = this._cells[i];
            let value = cell.getNumericValue();

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
    
    getNumericValue() {
        return Sudoku.fromSymbolToValue(this.Value());
    }
}

window.addEventListener("load", start, false);

function start(e) {
    let sudoku = new Sudoku();

    // document.addEventListener("keyup", e => sudoku.Keyup(e), false);

    ko.applyBindings(sudoku);
}