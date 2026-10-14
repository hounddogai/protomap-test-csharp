namespace Acme.Workforce.Absence.ReturnPlan.Assignment;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the return plan assignment calculation.</summary>
public interface IReturnPlanAssignmentStep
{
    string Describe();
    double Adjust(ReturnPlanAssignment item, double valueValue);
}

public sealed record TieredReturnPlanAssignmentAdjuster(List<double> Bounds, List<double> Rates) : IReturnPlanAssignmentStep
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(ReturnPlanAssignment item, double valueValue)
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

public sealed record CapReturnPlanAssignmentAdjuster(double Cap) : IReturnPlanAssignmentStep
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(ReturnPlanAssignment item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record MinimumReturnPlanAssignmentAdjuster(double Minimum) : IReturnPlanAssignmentStep
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(ReturnPlanAssignment item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record NameReturnPlanAssignmentAdjuster(Dictionary<string, double> Factors) : IReturnPlanAssignmentStep
{
    public string Describe()
    {
        return "by name";
    }

    public double Adjust(ReturnPlanAssignment item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record RoundingReturnPlanAssignmentAdjuster(double Step) : IReturnPlanAssignmentStep
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(ReturnPlanAssignment item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record FlaggedReturnPlanAssignmentAdjuster(Registry Flags, string Flag, double Percent) : IReturnPlanAssignmentStep
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(ReturnPlanAssignment item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the return plan assignment steps in order and keeps a trace of each.</summary>
public sealed class ReturnPlanAssignmentEngine(List<IReturnPlanAssignmentStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IReturnPlanAssignmentStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static ReturnPlanAssignmentEngine DefaultReturnPlanAssignmentEngine(Registry flags)
    {
        return new ReturnPlanAssignmentEngine(
            new List<IReturnPlanAssignmentStep>
            {
                new TieredReturnPlanAssignmentAdjuster(
                    new List<double> { 75.0, 2780.0, 4332.0 },
                    new List<double> { 1.05, 1.2, 0.92 }),
                new CapReturnPlanAssignmentAdjuster(2385.0),
                new MinimumReturnPlanAssignmentAdjuster(31.0),
                new NameReturnPlanAssignmentAdjuster(
                    new Dictionary<string, double> { ["online"] = 1.02, ["west"] = 0.78, ["central"] = 0.76 }),
                new RoundingReturnPlanAssignmentAdjuster(0.5),
                new FlaggedReturnPlanAssignmentAdjuster(flags, "return-plan-assignment-fallback", -9.2),
            },
            new List<string>(),
            2.8,
            75202.0);
    }

    public double Compute(ReturnPlanAssignment item)
    {
        var current = item.MeasuredValue();
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

    public Dictionary<string, double> EvaluateAll(List<ReturnPlanAssignment> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
