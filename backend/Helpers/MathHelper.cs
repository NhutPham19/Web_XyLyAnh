namespace WebXuLyAnh.Api.Helpers;

public static class MathHelper
{
    public static int Gcd(int a, int b)
    {
        a = Math.Abs(a);
        b = Math.Abs(b);
        while (b != 0)
        {
            int temp = b;
            b = a % b;
            a = temp;
        }
        return a == 0 ? 1 : a;
    }

    public static (int numerator, int denominator, string display) SimplifyFraction(int numerator, int denominator)
    {
        if (denominator == 0)
            throw new DivideByZeroException("Mẫu số không thể bằng 0.");

        if (numerator == 0)
            return (0, 1, "0");

        bool isNegative = (numerator < 0) ^ (denominator < 0);
        int gcd = Gcd(numerator, denominator);
        int simNum = Math.Abs(numerator) / gcd;
        int simDen = Math.Abs(denominator) / gcd;

        if (isNegative)
            simNum = -simNum;

        string display = simDen == 1 ? simNum.ToString() : $"{simNum}/{simDen}";
        return (simNum, simDen, display);
    }
}

public static class RoundingHelper
{
    /// <summary>
    /// Làm tròn 1 chữ số thập phân (Half-up, âm thì away from zero).
    /// Nếu sau khi làm tròn là .0 thì trả về chuỗi số nguyên. Tránh trả về -0 hoặc -0.0.
    /// </summary>
    public static string FormatRounded1(int numerator, int denominator)
    {
        if (numerator == 0) return "0";
        bool isNegative = (numerator < 0) ^ (denominator < 0);
        long absNum = Math.Abs((long)numerator);
        long absDen = Math.Abs((long)denominator);

        // x * 10 + 0.5 = (absNum * 10 * 2 + absDen) / (absDen * 2)
        long scaledPlusHalf = (absNum * 20 + absDen) / (absDen * 2);

        long integerPart = scaledPlusHalf / 10;
        long decimalPart = scaledPlusHalf % 10;

        string prefix = isNegative && (integerPart > 0 || decimalPart > 0) ? "-" : "";

        if (decimalPart == 0)
        {
            return $"{prefix}{integerPart}";
        }
        return $"{prefix}{integerPart}.{decimalPart}";
    }

    /// <summary>
    /// Làm tròn số nguyên (Half-up, âm thì away from zero).
    /// </summary>
    public static int FormatRoundedInt(int numerator, int denominator)
    {
        if (numerator == 0) return 0;
        bool isNegative = (numerator < 0) ^ (denominator < 0);
        long absNum = Math.Abs((long)numerator);
        long absDen = Math.Abs((long)denominator);

        long roundedAbs = (absNum * 2 + absDen) / (absDen * 2);
        return isNegative ? -(int)roundedAbs : (int)roundedAbs;
    }
}
