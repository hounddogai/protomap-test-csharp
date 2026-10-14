namespace Acme.Workforce.Talent.SuccessionPlan.Allocation;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the regional succession plan allocation calculation.</summary>
public interface IRegionalSuccessionPlanAllocationStep
{
    string Describe();
    double Adjust(RegionalSuccessionPlanAllocation item, double valueValue);
}

public sealed record FlaggedRegionalSuccessionPlanAllocationAdjuster(Registry Flags, string Flag, double Percent) : IRegionalSuccessionPlanAllocationStep
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(RegionalSuccessionPlanAllocation item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record PercentRegionalSuccessionPlanAllocationAdjuster(double Percent) : IRegionalSuccessionPlanAllocationStep
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(RegionalSuccessionPlanAllocation item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record RegionalSuccessionPlanAllocationExpiresAtOffsetAdjuster(double Weight) : IRegionalSuccessionPlanAllocationStep
{
    public string Describe()
    {
        return $"plus expires at times {Weight}";
    }

    public double Adjust(RegionalSuccessionPlanAllocation item, double valueValue)
    {
        return valueValue + (double)item.ExpiresAt * Weight;
    }
}

/// <summary>Runs the regional succession plan allocation steps in order and keeps a trace of each.</summary>
public sealed class RegionalSuccessionPlanAllocationEngine(List<IRegionalSuccessionPlanAllocationStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IRegionalSuccessionPlanAllocationStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static RegionalSuccessionPlanAllocationEngine DefaultRegionalSuccessionPlanAllocationEngine(Registry flags)
    {
        return new RegionalSuccessionPlanAllocationEngine(
            new List<IRegionalSuccessionPlanAllocationStep>
            {
                new FlaggedRegionalSuccessionPlanAllocationAdjuster(
                    flags,
                    "regional-succession-plan-allocation-pending",
                    -5.8),
                new PercentRegionalSuccessionPlanAllocationAdjuster(-23.7),
                new RegionalSuccessionPlanAllocationExpiresAtOffsetAdjuster(0.65),
            },
            new List<string>(),
            3.2,
            95295.3);
    }

    public double Run(RegionalSuccessionPlanAllocation item)
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

    public Dictionary<string, double> EvaluateAll(List<RegionalSuccessionPlanAllocation> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
