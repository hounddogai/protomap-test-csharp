namespace Acme.Workforce.Compensation.EquityGrant.Summary;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the equity grant claim calculation.</summary>
public interface IEquityGrantClaimStep
{
    string Describe();
    double Adjust(EquityGrantClaim item, double valueValue);
}

public sealed record EquityGrantClaimStartsAtOffsetAdjuster(double Weight) : IEquityGrantClaimStep
{
    public string Describe()
    {
        return $"plus starts at times {Weight}";
    }

    public double Adjust(EquityGrantClaim item, double valueValue)
    {
        return valueValue + (double)item.StartsAt * Weight;
    }
}

public sealed record CategoryEquityGrantClaimAdjuster(Dictionary<string, double> Factors) : IEquityGrantClaimStep
{
    public string Describe()
    {
        return "by category";
    }

    public double Adjust(EquityGrantClaim item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record TieredEquityGrantClaimAdjuster(List<double> Bounds, List<double> Rates) : IEquityGrantClaimStep
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(EquityGrantClaim item, double valueValue)
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

public sealed record CapEquityGrantClaimAdjuster(double Cap) : IEquityGrantClaimStep
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(EquityGrantClaim item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

/// <summary>Runs the equity grant claim steps in order and keeps a trace of each.</summary>
public sealed class EquityGrantClaimEngine(List<IEquityGrantClaimStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IEquityGrantClaimStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static EquityGrantClaimEngine DefaultEquityGrantClaimEngine()
    {
        return new EquityGrantClaimEngine(
            new List<IEquityGrantClaimStep>
            {
                new EquityGrantClaimStartsAtOffsetAdjuster(1.76),
                new CategoryEquityGrantClaimAdjuster(
                    new Dictionary<string, double> { ["east"] = 0.85, ["retail"] = 1.08, ["north"] = 1.22 }),
                new TieredEquityGrantClaimAdjuster(
                    new List<double> { 5077.0, 9143.0 },
                    new List<double> { 1.14, 1.08 }),
                new CapEquityGrantClaimAdjuster(30096.0),
            },
            new List<string>(),
            1.9,
            26724.7);
    }

    public double Compute(EquityGrantClaim item)
    {
        var current = item.NetValue();
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

    public Dictionary<string, double> EvaluateAll(List<EquityGrantClaim> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
