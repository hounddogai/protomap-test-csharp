namespace Acme.Workforce.Compensation.BonusPool.Variance;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the strict bonus pool variance calculation.</summary>
public interface IStrictBonusPoolVarianceStep
{
    string Describe();
    double Adjust(StrictBonusPoolVariance item, double valueValue);
}

public sealed record RoundingStrictBonusPoolVarianceAdjuster(double Step) : IStrictBonusPoolVarianceStep
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(StrictBonusPoolVariance item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record StrictBonusPoolVarianceRatioOffsetAdjuster(double Weight) : IStrictBonusPoolVarianceStep
{
    public string Describe()
    {
        return $"plus ratio times {Weight}";
    }

    public double Adjust(StrictBonusPoolVariance item, double valueValue)
    {
        return valueValue + item.Ratio * Weight;
    }
}

public sealed record CapStrictBonusPoolVarianceAdjuster(double Cap) : IStrictBonusPoolVarianceStep
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(StrictBonusPoolVariance item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record LabelStrictBonusPoolVarianceAdjuster(Dictionary<string, double> Factors) : IStrictBonusPoolVarianceStep
{
    public string Describe()
    {
        return "by label";
    }

    public double Adjust(StrictBonusPoolVariance item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record PercentStrictBonusPoolVarianceAdjuster(double Percent) : IStrictBonusPoolVarianceStep
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(StrictBonusPoolVariance item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the strict bonus pool variance steps in order and keeps a trace of each.</summary>
public sealed class StrictBonusPoolVarianceEngine(List<IStrictBonusPoolVarianceStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IStrictBonusPoolVarianceStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static StrictBonusPoolVarianceEngine DefaultStrictBonusPoolVarianceEngine()
    {
        return new StrictBonusPoolVarianceEngine(
            new List<IStrictBonusPoolVarianceStep>
            {
                new RoundingStrictBonusPoolVarianceAdjuster(0.05),
                new StrictBonusPoolVarianceRatioOffsetAdjuster(0.1),
                new CapStrictBonusPoolVarianceAdjuster(6726.0),
                new LabelStrictBonusPoolVarianceAdjuster(
                    new Dictionary<string, double> { ["central"] = 1.33, ["west"] = 0.95 }),
                new PercentStrictBonusPoolVarianceAdjuster(10.4),
            },
            new List<string>(),
            0.8,
            31713.3);
    }

    public double Run(StrictBonusPoolVariance item)
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

    public Dictionary<string, double> EvaluateAll(List<StrictBonusPoolVariance> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
