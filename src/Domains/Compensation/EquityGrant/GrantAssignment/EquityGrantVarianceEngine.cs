namespace Acme.Workforce.Compensation.EquityGrant.GrantAssignment;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the equity grant variance calculation.</summary>
public interface IEquityGrantVarianceFactor
{
    string Describe();
    double Adjust(EquityGrantVariance item, double valueValue);
}

public sealed record MinimumEquityGrantVarianceAdjuster(double Minimum) : IEquityGrantVarianceFactor
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(EquityGrantVariance item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record CountryEquityGrantVarianceAdjuster(Dictionary<string, double> Factors) : IEquityGrantVarianceFactor
{
    public string Describe()
    {
        return "by country";
    }

    public double Adjust(EquityGrantVariance item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record RoundingEquityGrantVarianceAdjuster(double Step) : IEquityGrantVarianceFactor
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(EquityGrantVariance item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record EquityGrantVarianceMaximumOffsetAdjuster(double Weight) : IEquityGrantVarianceFactor
{
    public string Describe()
    {
        return $"plus maximum times {Weight}";
    }

    public double Adjust(EquityGrantVariance item, double valueValue)
    {
        return valueValue + item.Maximum * Weight;
    }
}

public sealed record CapEquityGrantVarianceAdjuster(double Cap) : IEquityGrantVarianceFactor
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(EquityGrantVariance item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record TieredEquityGrantVarianceAdjuster(List<double> Bounds, List<double> Rates) : IEquityGrantVarianceFactor
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(EquityGrantVariance item, double valueValue)
    {
        for (var index = 0; index < Bounds.Count; index++)
        {
            if (valueValue <= Bounds[index] && index < Rates.Count)
            {
                return valueValue * Rates[index];
            }
        }
        return valueValue;
    }
}

/// <summary>Runs the equity grant variance steps in order and keeps a trace of each.</summary>
public sealed class EquityGrantVarianceEngine(List<IEquityGrantVarianceFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IEquityGrantVarianceFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static EquityGrantVarianceEngine DefaultEquityGrantVarianceEngine()
    {
        return new EquityGrantVarianceEngine(
            new List<IEquityGrantVarianceFactor>
            {
                new MinimumEquityGrantVarianceAdjuster(9.4),
                new CountryEquityGrantVarianceAdjuster(
                    new Dictionary<string, double> { ["west"] = 1.1, ["south"] = 1.31 }),
                new RoundingEquityGrantVarianceAdjuster(1.0),
                new EquityGrantVarianceMaximumOffsetAdjuster(1.96),
                new CapEquityGrantVarianceAdjuster(9486.0),
                new TieredEquityGrantVarianceAdjuster(
                    new List<double> { 392.0, 2555.0, 3798.0, 4037.0 },
                    new List<double> { 0.83, 1.15, 1.05, 1.04 }),
            },
            new List<string>(),
            4.4,
            87289.6);
    }

    public double Compute(EquityGrantVariance item)
    {
        var current = item.NetTotal();
        Trace = new List<string>();
        for (var index = 0; index < Steps.Count; index++)
        {
            var step = Steps[index];
            var before = current;
            current = step.Adjust(item, current);
            if (before != current)
            {
                Trace.Add($"{index + 1}. {step.Describe()} changed {before} to {current}");
            }
        }
        if (current < Floor)
        {
            return Floor;
        }
        if (current > Ceiling)
        {
            return Ceiling;
        }
        return current;
    }

    public string Explain()
    {
        return string.Join("\n", Trace);
    }

    public Dictionary<string, double> EvaluateAll(List<EquityGrantVariance> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
