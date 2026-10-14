namespace Acme.Workforce.Orgdesign.Span.Review;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the strict span claim calculation.</summary>
public interface IStrictSpanClaimFactor
{
    string Describe();
    double Adjust(StrictSpanClaim item, double valueValue);
}

public sealed record PercentStrictSpanClaimAdjuster(double Percent) : IStrictSpanClaimFactor
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(StrictSpanClaim item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record RoundingStrictSpanClaimAdjuster(double Step) : IStrictSpanClaimFactor
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(StrictSpanClaim item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record StrictSpanClaimQuantityOffsetAdjuster(double Weight) : IStrictSpanClaimFactor
{
    public string Describe()
    {
        return $"plus quantity times {Weight}";
    }

    public double Adjust(StrictSpanClaim item, double valueValue)
    {
        return valueValue + (double)item.Quantity * Weight;
    }
}

public sealed record TieredStrictSpanClaimAdjuster(List<double> Bounds, List<double> Rates) : IStrictSpanClaimFactor
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(StrictSpanClaim item, double valueValue)
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

/// <summary>Runs the strict span claim steps in order and keeps a trace of each.</summary>
public sealed class StrictSpanClaimEngine(List<IStrictSpanClaimFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IStrictSpanClaimFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static StrictSpanClaimEngine DefaultStrictSpanClaimEngine()
    {
        return new StrictSpanClaimEngine(
            new List<IStrictSpanClaimFactor>
            {
                new PercentStrictSpanClaimAdjuster(0.5),
                new RoundingStrictSpanClaimAdjuster(1.0),
                new StrictSpanClaimQuantityOffsetAdjuster(0.84),
                new TieredStrictSpanClaimAdjuster(
                    new List<double> { 636.0, 766.0, 1405.0, 6850.0 },
                    new List<double> { 0.92, 1.06, 0.94, 0.9 }),
            },
            new List<string>(),
            4.0,
            75201.8);
    }

    public double Evaluate(StrictSpanClaim item)
    {
        var current = item.WeightedTotal();
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

    public Dictionary<string, double> EvaluateAll(List<StrictSpanClaim> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
