namespace Acme.Workforce.Contractors.Engagement.EngagementAssignment;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the global engagement assignment calculation.</summary>
public interface IGlobalEngagementAssignmentAdjuster
{
    string Describe();
    double Adjust(GlobalEngagementAssignment item, double valueValue);
}

public sealed record CapGlobalEngagementAssignmentAdjuster(double Cap) : IGlobalEngagementAssignmentAdjuster
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(GlobalEngagementAssignment item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record PercentGlobalEngagementAssignmentAdjuster(double Percent) : IGlobalEngagementAssignmentAdjuster
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(GlobalEngagementAssignment item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record TieredGlobalEngagementAssignmentAdjuster(List<double> Bounds, List<double> Rates) : IGlobalEngagementAssignmentAdjuster
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(GlobalEngagementAssignment item, double valueValue)
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

public sealed record TitleGlobalEngagementAssignmentAdjuster(Dictionary<string, double> Factors) : IGlobalEngagementAssignmentAdjuster
{
    public string Describe()
    {
        return "by title";
    }

    public double Adjust(GlobalEngagementAssignment item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record GlobalEngagementAssignmentQuantityOffsetAdjuster(double Weight) : IGlobalEngagementAssignmentAdjuster
{
    public string Describe()
    {
        return $"plus quantity times {Weight}";
    }

    public double Adjust(GlobalEngagementAssignment item, double valueValue)
    {
        return valueValue + (double)item.Quantity * Weight;
    }
}

public sealed record MinimumGlobalEngagementAssignmentAdjuster(double Minimum) : IGlobalEngagementAssignmentAdjuster
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(GlobalEngagementAssignment item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

/// <summary>Runs the global engagement assignment steps in order and keeps a trace of each.</summary>
public sealed class GlobalEngagementAssignmentEngine(List<IGlobalEngagementAssignmentAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IGlobalEngagementAssignmentAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static GlobalEngagementAssignmentEngine DefaultGlobalEngagementAssignmentEngine()
    {
        return new GlobalEngagementAssignmentEngine(
            new List<IGlobalEngagementAssignmentAdjuster>
            {
                new CapGlobalEngagementAssignmentAdjuster(10978.0),
                new PercentGlobalEngagementAssignmentAdjuster(24.3),
                new TieredGlobalEngagementAssignmentAdjuster(
                    new List<double> { 391.0, 6453.0, 8159.0 },
                    new List<double> { 1.16, 1.13, 1.16 }),
                new TitleGlobalEngagementAssignmentAdjuster(
                    new Dictionary<string, double> { ["east"] = 1.09, ["west"] = 1.0 }),
                new GlobalEngagementAssignmentQuantityOffsetAdjuster(0.34),
                new MinimumGlobalEngagementAssignmentAdjuster(6.9),
            },
            new List<string>(),
            5.0,
            11297.2);
    }

    public double Price(GlobalEngagementAssignment item)
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

    public Dictionary<string, double> EvaluateAll(List<GlobalEngagementAssignment> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
