using System;
using System.Numerics;

namespace GraphicalLab.Matrix;

public class Matrix<T> where T : IMultiplyOperators<T, T, T>, IAdditionOperators<T, T, T>, IAdditiveIdentity<T, T>
{
    public int Height { get; }
    public int Width { get; }
    private T[,] Values { get; }
    
    public Matrix(int height, int width)
    {
        Height = height;
        Width = width;
        Values = new T[height, width];
    }

    public void SetValue(int row, int column, T value)
    {
        Values[row, column] = value;
    }
    
    public T GetValue(int row, int column)
    {
        return Values[row, column];
    }
    
    public static Matrix<T> operator *(Matrix<T> a, Matrix<T> b)
    {
        if (a.Width != b.Height) 
            throw new InvalidOperationException("Matrix dimensions incompatible for multiplication");
        
        var result = new Matrix<T>(a.Height, b.Width);

        for (int i = 0; i < a.Height; i++)
        {
            for (int j = 0; j < b.Width; j++)
            {
                result.Values[i, j] = MultiplyRowColumn(a, b, i, j);
            }
        }
        
        return result;
    }

    private static T MultiplyRowColumn(Matrix<T> a, Matrix<T> b, int row, int col)
    {
        T result = T.AdditiveIdentity;
        
        for (int k = 0; k < a.Width; k++)
        {
            result += a.Values[row, k] * b.Values[k, col];
        }
        
        return result;
    }
}