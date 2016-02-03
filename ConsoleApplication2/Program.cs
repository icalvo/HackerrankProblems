using System;
using System.IO;

public class Solution
{
    internal static void Main()
    {
        Problem.Solve(Console.In, Console.Out);
    }
}

public static class Problem
{
    public static void Solve(TextReader reader, TextWriter writer)
    {
        var rotationInput = MatrixRotationInput.FromReader(reader);
        var matrix = rotationInput.Matrix;
        int rotations = rotationInput.Rotations;
        var rotatedMatrix = matrix.Rotate(rotations);
        
        rotatedMatrix.Write(writer);
    }

}

public static class MatrixExtensions
{

    public static void Write<T>(this Matrix<T> matrix, TextWriter output)
    {
        for (int r = 0; r < matrix.RowCount; r++)
        {
            T[] row = new T[matrix.ColumnCount];
            for (int c = 0; c < matrix.ColumnCount; c++)
            {
                row[c] = matrix[r, c];
            }

            output.WriteLine(string.Join(" ", row));
        }
    }

}
public class MatrixRotationInput
{
    private MatrixRotationInput(int rotations, Matrix<string> matrix)
    {
        if (rotations < 0)
        {
            throw new ArgumentOutOfRangeException("rotations", "Cannot have negative rotations");
        }

        if (matrix == null)
        {
            throw new ArgumentNullException("matrix");
        }

        Rotations = rotations;
        Matrix = matrix;
    }

    public static MatrixRotationInput FromReader(TextReader input)
    {
        var allInput = input.ReadToEnd();
        var lines = allInput.Split(new[] {"\r", "\n"}, StringSplitOptions.RemoveEmptyEntries);



        string[] tokens = lines[0].Split(new [] { " " }, StringSplitOptions.RemoveEmptyEntries);
        var rowCount = Convert.ToInt32(tokens[0]);
        var columnCount = Convert.ToInt32(tokens[1]);
        var rotations = Convert.ToInt32(tokens[2]);
        var values = new Matrix<string>(rowCount, columnCount);
        for (int row = 0; row < rowCount; row++)
        {
            var line = lines[row + 1].Split(new[] { " " }, StringSplitOptions.RemoveEmptyEntries);
            for (int column = 0; column < columnCount; column++)
            {
                values[row, column] = line[column];
            }
        }

        return new MatrixRotationInput(rotations, values);
    }

    public int Rotations { get; private set; }

    public Matrix<string> Matrix { get; private set; }
}

public class Matrix<T>
{
    private readonly T[,] _values;

    public Matrix(int rowCount, int columnCount)
    {
        if (rowCount < 0)
        {
            throw new ArgumentOutOfRangeException("rowCount", "Cannot have negative row count");
        }

        if (columnCount < 0)
        {
            throw new ArgumentOutOfRangeException("columnCount", "Cannot have negative column count");
        }

        RowCount = rowCount;
        ColumnCount = columnCount;
        _values = new T[RowCount, ColumnCount];
    }

    public int RowCount { get; private set; }

    public int ColumnCount { get; private set; }

    public T this[int rowIndex, int columnIndex]
    {
        get { return _values[rowIndex, columnIndex]; }
        set
        {
            _values[rowIndex, columnIndex] = value;
        }
    }

    public Matrix<T> Rotate(int numberOfRotations)
    {
        if (Math.Min(RowCount, ColumnCount) % 2 != 0)
        {
            throw new InvalidOperationException("Cannot rotate if Min(RowCount, ColumnCount) % 2 != 0");
        }

        var result = new Matrix<T>(RowCount, ColumnCount);

        if (numberOfRotations < 0)
        {
            throw new ArgumentOutOfRangeException("numberOfRotations", "Cannot have negative rotations");
        }

        for (int row = 0; row < RowCount; row++)
        {
            for (int column = 0; column < ColumnCount; column++)
            {
                result[row, column] = ValueAtRotatedPosition(row, column, numberOfRotations);
            }
        }

        return result;
    }

    private T ValueAtRotatedPosition(int row, int column, int numberOfRotations)
    {
        var position = new PositionOnMatrix(row, column, RowCount, ColumnCount);
        var rotatedPosition = position.Rotate(numberOfRotations);
        return _values[rotatedPosition.Row, rotatedPosition.Column];
    }

    public Matrix<T> Transform(Func<int, int, T> func)
    {
        var result = new Matrix<T>(RowCount, ColumnCount);

        for (int row = 0; row < RowCount; row++)
        {
            for (int column = 0; column < ColumnCount; column++)
            {
                result[row, column] = func(row, column);
            }
        }

        return result;
    }
}

public class PositionOnMatrix
{
    public PositionOnMatrix(int row, int column, int rowCount, int columnCount)
    {
        Row = row;
        Column = column;
        RowCount = rowCount;
        ColumnCount = columnCount;
    }

    public int Row { get; private set; }

    public int Column { get; private set; }

    private int RowCount { get; set; }

    private int ColumnCount { get; set; }

    public PositionOnMatrix Rotate(int numberOfRotations)
    {
        PositionOnMatrix rotatedPosition = this;

        numberOfRotations = OptimizeNumberOfRotations(numberOfRotations);
        ActionExtensions.Repeat(() =>
        {
            rotatedPosition = rotatedPosition.RotateOnce();
        },
        numberOfRotations);

        return rotatedPosition;
    }

    private int OptimizeNumberOfRotations(int originalNumberOfRotations)
    {
        return originalNumberOfRotations % CycleLength();
    }

    internal int CycleLength()
    {
        return 2*RowCount + 2*ColumnCount - 8*Depth() - 4;
    }

    internal int Depth()
    {
        int dist1 = Row;
        int dist2 = Column;
        int dist3 = RowCount - Row - 1;
        int dist4 = ColumnCount - Column - 1;

        return Math.Min(Math.Min(dist1, dist2), Math.Min(dist3, dist4));
    }

    private PositionOnMatrix RotateOnce()
    {
        if (MustGoDownWhenRotated())
        {
            return new PositionOnMatrix(Row + 1, Column, RowCount, ColumnCount);
        }

        if (MustGoUpWhenRotated())
        {
            return new PositionOnMatrix(Row - 1, Column, RowCount, ColumnCount);
        }

        if (MustGoRightWhenRotated())
        {
            return new PositionOnMatrix(Row, Column + 1, RowCount, ColumnCount);
        }

        if (MustGoLeftWhenRotated())
        {
            return new PositionOnMatrix(Row, Column - 1, RowCount, ColumnCount);
        }

        throw new InvalidOperationException("Should not get here");
    }

    private bool MustGoDownWhenRotated()
    {
        return
            Row >= ColumnCount - Column - 1 &&
            Column >= ColumnCount / 2 &&
            Row < RowCount - ColumnCount + Column;
    }

    private bool MustGoUpWhenRotated()
    {
        return
            Row >= Column + 1 &&
            Column < ColumnCount / 2 &&
            Row < RowCount - Column;
    }

    private bool MustGoRightWhenRotated()
    {
        return
            Column >= Row &&
            Row < RowCount / 2 &&
            Column < ColumnCount - Row - 1;
    }

    private bool MustGoLeftWhenRotated()
    {
        return
            Column >= RowCount - Row &&
            Row >= RowCount / 2 &&
            Column < ColumnCount - RowCount + Row + 1;
    }
}

internal static class ActionExtensions
{
    internal static void Repeat(this Action action, int times)
    {
        for (int rotation = 1; rotation <= times; rotation++)
        {
            action();
        }
    }
}