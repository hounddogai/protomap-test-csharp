namespace Acme.Workforce.Workplace.Visitor.VisitorOverride;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the regional visitor allocation calculation.</summary>
public interface IRegionalVisitorAllocationAdjuster
{
    string Describe();
    double Adjust(RegionalVisitorAllocation item, double valueValue);
}

public sealed record TieredRegionalVisitorAllocationAdjuster(List<double> Bounds, List<double> Rates) : IRegionalVisitorAllocationAdjuster
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(RegionalVisitorAllocation item, double valueValue)
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

public sealed record CapRegionalVisitorAllocationAdjuster(double Cap) : IRegionalVisitorAllocationAdjuster
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(RegionalVisitorAllocation item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record LabelRegionalVisitorAllocationAdjuster(Dictionary<string, double> Factors) : IRegionalVisitorAllocationAdjuster
{
    public string Describe()
    {
        return "by label";
    }

    public double Adjust(RegionalVisitorAllocation item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record MinimumRegionalVisitorAllocationAdjuster(double Minimum) : IRegionalVisitorAllocationAdjuster
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(RegionalVisitorAllocation item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

/// <summary>Runs the regional visitor allocation steps in order and keeps a trace of each.</summary>
public sealed class RegionalVisitorAllocationEngine(List<IRegionalVisitorAllocationAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IRegionalVisitorAllocationAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static RegionalVisitorAllocationEngine DefaultRegionalVisitorAllocationEngine()
    {
        return new RegionalVisitorAllocationEngine(
            new List<IRegionalVisitorAllocationAdjuster>
            {
                new TieredRegionalVisitorAllocationAdjuster(
                    new List<double> { 18.0, 2085.0, 5625.0, 6094.0 },
                    new List<double> { 0.95, 1.01, 1.06, 0.8 }),
                new CapRegionalVisitorAllocationAdjuster(19818.0),
                new LabelRegionalVisitorAllocationAdjuster(
                    new Dictionary<string, double> { ["north"] = 1.32, ["south"] = 1.29 }),
                new MinimumRegionalVisitorAllocationAdjuster(24.1),
            },
            new List<string>(),
            1.9,
            95964.2);
    }

    public double Compute(RegionalVisitorAllocation item)
    {
        var current = item.ProjectedExposure();
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

    public Dictionary<string, double> EvaluateAll(List<RegionalVisitorAllocation> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
