namespace Acme.Workforce.Contractors.Engagement.Proposal;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the engagement allocation calculation.</summary>
public interface IEngagementAllocationStep
{
    string Describe();
    double Adjust(EngagementAllocation item, double valueValue);
}

public sealed record PercentEngagementAllocationAdjuster(double Percent) : IEngagementAllocationStep
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(EngagementAllocation item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record LabelEngagementAllocationAdjuster(Dictionary<string, double> Factors) : IEngagementAllocationStep
{
    public string Describe()
    {
        return "by label";
    }

    public double Adjust(EngagementAllocation item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record TieredEngagementAllocationAdjuster(List<double> Bounds, List<double> Rates) : IEngagementAllocationStep
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(EngagementAllocation item, double valueValue)
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

/// <summary>Runs the engagement allocation steps in order and keeps a trace of each.</summary>
public sealed class EngagementAllocationEngine(List<IEngagementAllocationStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IEngagementAllocationStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static EngagementAllocationEngine DefaultEngagementAllocationEngine()
    {
        return new EngagementAllocationEngine(
            new List<IEngagementAllocationStep>
            {
                new PercentEngagementAllocationAdjuster(21.4),
                new LabelEngagementAllocationAdjuster(
                    new Dictionary<string, double> { ["central"] = 0.98, ["south"] = 1.16, ["east"] = 0.91 }),
                new TieredEngagementAllocationAdjuster(
                    new List<double> { 989.0, 7975.0 },
                    new List<double> { 1.17, 1.07 }),
            },
            new List<string>(),
            3.9,
            87591.9);
    }

    public double Price(EngagementAllocation item)
    {
        var current = item.AdjustedTotal();
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

    public Dictionary<string, double> EvaluateAll(List<EngagementAllocation> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
