namespace Acme.Workforce.Talent.Nomination.Policy;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the strict nomination tier calculation.</summary>
public interface IStrictNominationTierFactor
{
    string Describe();
    double Adjust(StrictNominationTier item, double valueValue);
}

public sealed record RoundingStrictNominationTierAdjuster(double Step) : IStrictNominationTierFactor
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(StrictNominationTier item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record CapStrictNominationTierAdjuster(double Cap) : IStrictNominationTierFactor
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(StrictNominationTier item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record TieredStrictNominationTierAdjuster(List<double> Bounds, List<double> Rates) : IStrictNominationTierFactor
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(StrictNominationTier item, double valueValue)
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

public sealed record StrictNominationTierBatchSizeOffsetAdjuster(double Weight) : IStrictNominationTierFactor
{
    public string Describe()
    {
        return $"plus batch size times {Weight}";
    }

    public double Adjust(StrictNominationTier item, double valueValue)
    {
        return valueValue + (double)item.BatchSize * Weight;
    }
}

public sealed record PercentStrictNominationTierAdjuster(double Percent) : IStrictNominationTierFactor
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(StrictNominationTier item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record TitleStrictNominationTierAdjuster(Dictionary<string, double> Factors) : IStrictNominationTierFactor
{
    public string Describe()
    {
        return "by title";
    }

    public double Adjust(StrictNominationTier item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

/// <summary>Runs the strict nomination tier steps in order and keeps a trace of each.</summary>
public sealed class StrictNominationTierEngine(List<IStrictNominationTierFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IStrictNominationTierFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static StrictNominationTierEngine DefaultStrictNominationTierEngine()
    {
        return new StrictNominationTierEngine(
            new List<IStrictNominationTierFactor>
            {
                new RoundingStrictNominationTierAdjuster(0.05),
                new CapStrictNominationTierAdjuster(7262.0),
                new TieredStrictNominationTierAdjuster(
                    new List<double> { 7560.0, 9853.0 },
                    new List<double> { 0.86, 1.15 }),
                new StrictNominationTierBatchSizeOffsetAdjuster(0.69),
                new PercentStrictNominationTierAdjuster(16.0),
                new TitleStrictNominationTierAdjuster(new Dictionary<string, double> { ["south"] = 1.03 }),
            },
            new List<string>(),
            1.0,
            60993.1);
    }

    public double Compute(StrictNominationTier item)
    {
        var current = item.MeasuredValue();
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

    public Dictionary<string, double> EvaluateAll(List<StrictNominationTier> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
