namespace Acme.Workforce.Kit.Numbers;

/// <summary>Functions of numbers.</summary>
public static class Numbers
{
    public static int Clamp(int valueValue, int low, int high)
    {
        if (valueValue < low)
        {
            return low;
        }
        if (valueValue > high)
        {
            return high;
        }
        return valueValue;
    }

    public static double ClampFloat(double valueValue, double low, double high)
    {
        return Math.Min(Math.Max(valueValue, low), high);
    }

    /// <summary>Returns part as a percentage of whole, rounded to two places, and 0 when whole is 0.</summary>
    public static double Percent(double part, double whole)
    {
        if (whole == 0.0)
        {
            return 0.0;
        }
        return Math.Round(part / whole * 100.0, 2);
    }

    public static double RoundTo(double valueValue, double step)
    {
        if (step <= 0.0)
        {
            return valueValue;
        }
        return Math.Floor(valueValue / step + 0.5) * step;
    }

    public static double Mean(List<double> values)
    {
        if (values.Count == 0)
        {
            return 0.0;
        }
        var total = 0.0;
        foreach (var valueValue in values)
        {
            total += valueValue;
        }
        return total / (double)values.Count;
    }

    /// <summary>Returns the values in ascending order, leaving the input as it was.</summary>
    public static List<double> SortedFloats(List<double> values)
    {
        var sorted = new List<double>(values);
        for (var i = 0; i < sorted.Count; i++)
        {
            for (var k = 0; k < sorted.Count - i - 1; k++)
            {
                if (sorted[k] > sorted[k + 1])
                {
                    var swap = sorted[k];
                    sorted[k] = sorted[k + 1];
                    sorted[k + 1] = swap;
                }
            }
        }
        return sorted;
    }

    /// <summary>Returns the p-th percentile by the nearest-rank method.</summary>
    public static double Percentile(List<double> values, double p)
    {
        if (values.Count == 0)
        {
            return 0.0;
        }
        var sorted = SortedFloats(values);
        var rank = (int)(p / 100.0 * (double)(sorted.Count - 1));
        return sorted[Clamp(rank, 0, sorted.Count - 1)];
    }

    public static double WeightedMean(List<double> values, List<double> weights)
    {
        if (values.Count != weights.Count)
        {
            throw new InvalidOperationException($"got {values.Count} values but {weights.Count} weights");
        }
        var sum = 0.0;
        var weight = 0.0;
        for (var i = 0; i < values.Count; i++)
        {
            sum += values[i] * weights[i];
            weight += weights[i];
        }
        if (weight == 0.0)
        {
            return 0.0;
        }
        return sum / weight;
    }

    public static double GrowthRate(double previous, double current)
    {
        if (previous == 0.0)
        {
            return 0.0;
        }
        return Math.Round((current - previous) / Math.Abs(previous), 2);
    }
}
