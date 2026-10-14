namespace Acme.Workforce.Compliance.Exception.Rule;

using Acme.Workforce.Kit.Flags;

/// <summary>One step of the fallback exception assignment calculation.</summary>
public interface IFallbackExceptionAssignmentStep
{
    string Describe();
    double Adjust(FallbackExceptionAssignment item, double valueValue);
}

public sealed record FlaggedFallbackExceptionAssignmentAdjuster(Registry Flags, string Flag, double Percent) : IFallbackExceptionAssignmentStep
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(FallbackExceptionAssignment item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record PercentFallbackExceptionAssignmentAdjuster(double Percent) : IFallbackExceptionAssignmentStep
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(FallbackExceptionAssignment item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record CapFallbackExceptionAssignmentAdjuster(double Cap) : IFallbackExceptionAssignmentStep
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(FallbackExceptionAssignment item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record TieredFallbackExceptionAssignmentAdjuster(List<double> Bounds, List<double> Rates) : IFallbackExceptionAssignmentStep
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(FallbackExceptionAssignment item, double valueValue)
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

public sealed record NameFallbackExceptionAssignmentAdjuster(Dictionary<string, double> Factors) : IFallbackExceptionAssignmentStep
{
    public string Describe()
    {
        return "by name";
    }

    public double Adjust(FallbackExceptionAssignment item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

/// <summary>Runs the fallback exception assignment steps in order and keeps a trace of each.</summary>
public sealed class FallbackExceptionAssignmentEngine(List<IFallbackExceptionAssignmentStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IFallbackExceptionAssignmentStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static FallbackExceptionAssignmentEngine DefaultFallbackExceptionAssignmentEngine(Registry flags)
    {
        return new FallbackExceptionAssignmentEngine(
            new List<IFallbackExceptionAssignmentStep>
            {
                new FlaggedFallbackExceptionAssignmentAdjuster(flags, "fallback-exception-assignment-seasonal", -5.5),
                new PercentFallbackExceptionAssignmentAdjuster(1.7),
                new CapFallbackExceptionAssignmentAdjuster(12081.0),
                new TieredFallbackExceptionAssignmentAdjuster(
                    new List<double> { 163.0, 1318.0, 2850.0, 9637.0 },
                    new List<double> { 0.85, 1.05, 0.9, 0.98 }),
                new NameFallbackExceptionAssignmentAdjuster(
                    new Dictionary<string, double> { ["central"] = 0.88, ["south"] = 1.36 }),
            },
            new List<string>(),
            0.5,
            35923.2);
    }

    public double Run(FallbackExceptionAssignment item)
    {
        var current = item.AdjustedVolume();
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

    public Dictionary<string, double> EvaluateAll(List<FallbackExceptionAssignment> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
