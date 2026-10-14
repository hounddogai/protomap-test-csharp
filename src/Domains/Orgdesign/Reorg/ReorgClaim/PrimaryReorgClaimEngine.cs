namespace Acme.Workforce.Orgdesign.Reorg.ReorgClaim;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the primary reorg claim calculation.</summary>
public interface IPrimaryReorgClaimStep
{
    string Describe();
    double Adjust(PrimaryReorgClaim item, double valueValue);
}

public sealed record TieredPrimaryReorgClaimAdjuster(List<double> Bounds, List<double> Rates) : IPrimaryReorgClaimStep
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(PrimaryReorgClaim item, double valueValue)
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

public sealed record SourcePrimaryReorgClaimAdjuster(Dictionary<string, double> Factors) : IPrimaryReorgClaimStep
{
    public string Describe()
    {
        return "by source";
    }

    public double Adjust(PrimaryReorgClaim item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record MinimumPrimaryReorgClaimAdjuster(double Minimum) : IPrimaryReorgClaimStep
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(PrimaryReorgClaim item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record CapPrimaryReorgClaimAdjuster(double Cap) : IPrimaryReorgClaimStep
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(PrimaryReorgClaim item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record RoundingPrimaryReorgClaimAdjuster(double Step) : IPrimaryReorgClaimStep
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(PrimaryReorgClaim item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record PercentPrimaryReorgClaimAdjuster(double Percent) : IPrimaryReorgClaimStep
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(PrimaryReorgClaim item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the primary reorg claim steps in order and keeps a trace of each.</summary>
public sealed class PrimaryReorgClaimEngine(List<IPrimaryReorgClaimStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IPrimaryReorgClaimStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static PrimaryReorgClaimEngine DefaultPrimaryReorgClaimEngine()
    {
        return new PrimaryReorgClaimEngine(
            new List<IPrimaryReorgClaimStep>
            {
                new TieredPrimaryReorgClaimAdjuster(
                    new List<double> { 4072.0, 6873.0 },
                    new List<double> { 0.92, 0.81 }),
                new SourcePrimaryReorgClaimAdjuster(
                    new Dictionary<string, double> { ["central"] = 0.87, ["east"] = 1.08, ["north"] = 1.14 }),
                new MinimumPrimaryReorgClaimAdjuster(35.1),
                new CapPrimaryReorgClaimAdjuster(26504.0),
                new RoundingPrimaryReorgClaimAdjuster(0.5),
                new PercentPrimaryReorgClaimAdjuster(21.8),
            },
            new List<string>(),
            3.4,
            83451.0);
    }

    public double Price(PrimaryReorgClaim item)
    {
        var current = item.WeightedExposure();
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

    public Dictionary<string, double> EvaluateAll(List<PrimaryReorgClaim> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
