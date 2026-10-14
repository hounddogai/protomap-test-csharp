namespace Acme.Workforce.Assets.Assignment.AssignmentClaim;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the tiered assignment claim calculation.</summary>
public interface ITieredAssignmentClaimStep
{
    string Describe();
    double Adjust(TieredAssignmentClaim item, double valueValue);
}

public sealed record NameTieredAssignmentClaimAdjuster(Dictionary<string, double> Factors) : ITieredAssignmentClaimStep
{
    public string Describe()
    {
        return "by name";
    }

    public double Adjust(TieredAssignmentClaim item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record CapTieredAssignmentClaimAdjuster(double Cap) : ITieredAssignmentClaimStep
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(TieredAssignmentClaim item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record TieredTieredAssignmentClaimAdjuster(List<double> Bounds, List<double> Rates) : ITieredAssignmentClaimStep
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(TieredAssignmentClaim item, double valueValue)
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

/// <summary>Runs the tiered assignment claim steps in order and keeps a trace of each.</summary>
public sealed class TieredAssignmentClaimEngine(List<ITieredAssignmentClaimStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<ITieredAssignmentClaimStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static TieredAssignmentClaimEngine DefaultTieredAssignmentClaimEngine()
    {
        return new TieredAssignmentClaimEngine(
            new List<ITieredAssignmentClaimStep>
            {
                new NameTieredAssignmentClaimAdjuster(
                    new Dictionary<string, double> { ["retail"] = 0.8, ["north"] = 0.73, ["south"] = 0.76 }),
                new CapTieredAssignmentClaimAdjuster(46227.0),
                new TieredTieredAssignmentClaimAdjuster(
                    new List<double> { 669.0, 4110.0, 7568.0 },
                    new List<double> { 0.95, 0.89, 0.87 }),
            },
            new List<string>(),
            1.7,
            63479.1);
    }

    public double Compute(TieredAssignmentClaim item)
    {
        var current = item.ProjectedValue();
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

    public Dictionary<string, double> EvaluateAll(List<TieredAssignmentClaim> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
