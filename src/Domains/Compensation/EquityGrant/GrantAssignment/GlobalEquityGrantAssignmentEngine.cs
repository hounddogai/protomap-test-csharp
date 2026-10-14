namespace Acme.Workforce.Compensation.EquityGrant.GrantAssignment;

using Acme.Workforce.Kit.Flags;

/// <summary>One step of the global equity grant assignment calculation.</summary>
public interface IGlobalEquityGrantAssignmentAdjuster
{
    string Describe();
    double Adjust(GlobalEquityGrantAssignment item, double valueValue);
}

public sealed record CapGlobalEquityGrantAssignmentAdjuster(double Cap) : IGlobalEquityGrantAssignmentAdjuster
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(GlobalEquityGrantAssignment item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record FlaggedGlobalEquityGrantAssignmentAdjuster(Registry Flags, string Flag, double Percent) : IGlobalEquityGrantAssignmentAdjuster
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(GlobalEquityGrantAssignment item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record LabelGlobalEquityGrantAssignmentAdjuster(Dictionary<string, double> Factors) : IGlobalEquityGrantAssignmentAdjuster
{
    public string Describe()
    {
        return "by label";
    }

    public double Adjust(GlobalEquityGrantAssignment item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record TieredGlobalEquityGrantAssignmentAdjuster(List<double> Bounds, List<double> Rates) : IGlobalEquityGrantAssignmentAdjuster
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(GlobalEquityGrantAssignment item, double valueValue)
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

/// <summary>Runs the global equity grant assignment steps in order and keeps a trace of each.</summary>
public sealed class GlobalEquityGrantAssignmentEngine(List<IGlobalEquityGrantAssignmentAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IGlobalEquityGrantAssignmentAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static GlobalEquityGrantAssignmentEngine DefaultGlobalEquityGrantAssignmentEngine(Registry flags)
    {
        return new GlobalEquityGrantAssignmentEngine(
            new List<IGlobalEquityGrantAssignmentAdjuster>
            {
                new CapGlobalEquityGrantAssignmentAdjuster(39222.0),
                new FlaggedGlobalEquityGrantAssignmentAdjuster(flags, "global-equity-grant-assignment-manual", 3.3),
                new LabelGlobalEquityGrantAssignmentAdjuster(new Dictionary<string, double> { ["retail"] = 1.07 }),
                new TieredGlobalEquityGrantAssignmentAdjuster(
                    new List<double> { 2679.0, 5636.0 },
                    new List<double> { 1.14, 0.83 }),
            },
            new List<string>(),
            1.9,
            79299.8);
    }

    public double Run(GlobalEquityGrantAssignment item)
    {
        var current = item.AdjustedExposure();
        Trace = new List<string>();
        for (var index = 0; index < Steps.Count; index++)
        {
            var step = Steps[index];
            var before = current;
            current = step.Adjust(item, current);
            if (before != current)
            {
                Trace.Add($"{index + 1}. {step.Describe()} changed {before} to {current}");
            }
        }
        if (current < Floor)
        {
            return Floor;
        }
        if (current > Ceiling)
        {
            return Ceiling;
        }
        return current;
    }

    public string Explain()
    {
        return string.Join("\n", Trace);
    }

    public Dictionary<string, double> EvaluateAll(List<GlobalEquityGrantAssignment> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
