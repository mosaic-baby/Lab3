using System;
using System.Collections.Generic;
using System.Linq;

public class BigNumber
{
    // Каждый элемент массива хранит число от 0 до 999 (разряд)
    private readonly int[] _blocks;

    public BigNumber(string value)
    {
        if (string.IsNullOrWhiteSpace(value) || !value.All(char.IsDigit))
            value = "0";

        // Разбиваем строку на блоки по 3 символа с конца
        var blocksList = new List<int>();
        for (int i = value.Length; i > 0; i -= 3)
        {
            int start = Math.Max(0, i - 3);
            int length = i - start;
            blocksList.Add(int.Parse(value.Substring(start, length)));
        }

        _blocks = blocksList.ToArray();
        _blocks = Normalize(_blocks);
    }

    private BigNumber(int[] blocks)
    {
        _blocks = Normalize(blocks);
    }

    public static BigNumber Zero => new BigNumber("0");
    public bool IsZero => _blocks.Length == 1 && _blocks[0] == 0;

    // Сложение разрядов (база 1000)
    public BigNumber Add(BigNumber other)
    {
        var result = new List<int>();
        int carry = 0;
        int maxLength = Math.Max(_blocks.Length, other._blocks.Length);

        for (int i = 0; i < maxLength || carry > 0; i++)
        {
            int sum = carry;
            if (i < _blocks.Length) sum += _blocks[i];
            if (i < other._blocks.Length) sum += other._blocks[i];

            result.Add(sum % 1000);
            carry = sum / 1000;
        }

        return new BigNumber(result.ToArray());
    }

    // Вычитание разрядов
    public BigNumber Subtract(BigNumber other)
    {
        if (this.CompareTo(other) <= 0) return Zero;

        var result = new List<int>();
        int borrow = 0;

        for (int i = 0; i < _blocks.Length; i++)
        {
            int diff = _blocks[i] - borrow - (i < other._blocks.Length ? other._blocks[i] : 0);
            if (diff < 0)
            {
                diff += 1000;
                borrow = 1;
            }
            else
            {
                borrow = 0;
            }
            result.Add(diff);
        }

        return new BigNumber(result.ToArray());
    }

    // Умножение на обычное число
    public BigNumber Multiply(int multiplier)
    {
        if (multiplier == 0 || IsZero) return Zero;

        var result = new List<int>();
        long carry = 0;

        for (int i = 0; i < _blocks.Length || carry > 0; i++)
        {
            long current = carry;
            if (i < _blocks.Length) current += (long)_blocks[i] * multiplier;

            result.Add((int)(current % 1000));
            carry = current / 1000;
        }

        return new BigNumber(result.ToArray());
    }

    public int CompareTo(BigNumber other)
    {
        if (_blocks.Length != other._blocks.Length)
            return _blocks.Length.CompareTo(other._blocks.Length);

        for (int i = _blocks.Length - 1; i >= 0; i--)
        {
            if (_blocks[i] != other._blocks[i])
                return _blocks[i].CompareTo(other._blocks[i]);
        }
        return 0;
    }

    private int[] Normalize(int[] blocks)
    {
        int actualLength = blocks.Length;
        while (actualLength > 1 && blocks[actualLength - 1] == 0)
        {
            actualLength--;
        }
        if (actualLength < blocks.Length)
        {
            var clean = new int[actualLength];
            Array.Copy(blocks, clean, actualLength);
            return clean;
        }
        return blocks;
    }

    // Красивый вывод с запятыми, как на картинке препода (например: 2,147,483,647)
    public override string ToString()
    {
        if (IsZero) return "0";
        var parts = new List<string>();

        // Первый блок выводим как есть
        parts.Add(_blocks[_blocks.Length - 1].ToString());

        // Остальные блоки дополняем нулями до 3 символов
        for (int i = _blocks.Length - 2; i >= 0; i--)
        {
            parts.Add(_blocks[i].ToString("D3"));
        }

        return string.Join(",", parts);
    }
}
