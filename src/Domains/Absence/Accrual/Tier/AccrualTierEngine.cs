namespace Acme.Workforce.Absence.Accrual.Tier;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the accrual tier calculation.</summary>
public interface IAccrualTierFactor
{
    string Describe();
    double Adjust(AccrualTier item, double valueValue);
}

public sealed record RoundingAccrualTierAdjuster(double Step) : IAccrualTierFactor
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(AccrualTier item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record UnitAccrualTierAdjuster(Dictionary<string, double> Factors) : IAccrualTierFactor
{
    public string Describe()
    {
        return "by unit";
    }

    public double Adjust(AccrualTier item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record AccrualTierRetriesOffsetAdjuster(double Weight) : IAccrualTierFactor
{
    public string Describe()
    {
        return $"plus retries times {Weight}";
    }

    public double Adjust(AccrualTier item, double valueValue)
    {
        return valueValue + (double)item.Retries * Weight;
    }
}

public sealed record PercentAccrualTierAdjuster(double Percent) : IAccrualTierFactor
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(AccrualTier item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record TieredAccrualTierAdjuster(List<double> Bounds, List<double> Rates) : IAccrualTierFactor
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(AccrualTier item, double valueValue)
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

public sealed record MinimumAccrualTierAdjuster(double Minimum) : IAccrualTierFactor
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(AccrualTier item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

/// <summary>Runs the accrual tier steps in order and keeps a trace of each.</summary>
public sealed class AccrualTierEngine(List<IAccrualTierFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IAccrualTierFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static AccrualTierEngine DefaultAccrualTierEngine()
    {
        return new AccrualTierEngine(
            new List<IAccrualTierFactor>
            {
                new RoundingAccrualTierAdjuster(1.0),
                new UnitAccrualTierAdjuster(new Dictionary<string, double> { ["retail"] = 0.8, ["north"] = 0.7 }),
                new AccrualTierRetriesOffsetAdjuster(0.34),
                new PercentAccrualTierAdjuster(8.8),
                new TieredAccrualTierAdjuster(
                    new List<double> { 443.0, 1265.0, 2744.0 },
                    new List<double> { 0.99, 0.86, 0.86 }),
                new MinimumAccrualTierAdjuster(34.3),
            },
            new List<string>(),
            0.6,
            60409.1);
    }

    public double Compute(AccrualTier item)
    {
        var current = item.WeightedLoad();
        Trace = new List<string>();
        foreach (var step in Steps)
        {
            var next = step.Adjust(item, current);
            Trace.Add($"{step.Describe()}: {Math.Round(current, 2)} -> {Math.Round(next, 2)}");
            current = next;
        }
        return Numbers.ClampFloat(current, Floor, Ceiling);
    }

    public string Explain()
    {
        return string.Join("\n", Trace);
    }

    public Dictionary<string, double> EvaluateAll(List<AccrualTier> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
