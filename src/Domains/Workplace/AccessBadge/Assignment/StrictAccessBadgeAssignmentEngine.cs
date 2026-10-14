namespace Acme.Workforce.Workplace.AccessBadge.Assignment;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the strict access badge assignment calculation.</summary>
public interface IStrictAccessBadgeAssignmentStep
{
    string Describe();
    double Adjust(StrictAccessBadgeAssignment item, double valueValue);
}

public sealed record RoundingStrictAccessBadgeAssignmentAdjuster(double Step) : IStrictAccessBadgeAssignmentStep
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(StrictAccessBadgeAssignment item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record PercentStrictAccessBadgeAssignmentAdjuster(double Percent) : IStrictAccessBadgeAssignmentStep
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(StrictAccessBadgeAssignment item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record TieredStrictAccessBadgeAssignmentAdjuster(List<double> Bounds, List<double> Rates) : IStrictAccessBadgeAssignmentStep
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(StrictAccessBadgeAssignment item, double valueValue)
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

public sealed record TeamStrictAccessBadgeAssignmentAdjuster(Dictionary<string, double> Factors) : IStrictAccessBadgeAssignmentStep
{
    public string Describe()
    {
        return "by team";
    }

    public double Adjust(StrictAccessBadgeAssignment item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record CapStrictAccessBadgeAssignmentAdjuster(double Cap) : IStrictAccessBadgeAssignmentStep
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(StrictAccessBadgeAssignment item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record StrictAccessBadgeAssignmentBudgetOffsetAdjuster(double Weight) : IStrictAccessBadgeAssignmentStep
{
    public string Describe()
    {
        return $"plus budget times {Weight}";
    }

    public double Adjust(StrictAccessBadgeAssignment item, double valueValue)
    {
        return valueValue + item.Budget * Weight;
    }
}

/// <summary>Runs the strict access badge assignment steps in order and keeps a trace of each.</summary>
public sealed class StrictAccessBadgeAssignmentEngine(List<IStrictAccessBadgeAssignmentStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IStrictAccessBadgeAssignmentStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static StrictAccessBadgeAssignmentEngine DefaultStrictAccessBadgeAssignmentEngine()
    {
        return new StrictAccessBadgeAssignmentEngine(
            new List<IStrictAccessBadgeAssignmentStep>
            {
                new RoundingStrictAccessBadgeAssignmentAdjuster(0.5),
                new PercentStrictAccessBadgeAssignmentAdjuster(-0.6),
                new TieredStrictAccessBadgeAssignmentAdjuster(
                    new List<double> { 2652.0, 3509.0, 5594.0 },
                    new List<double> { 0.99, 0.91, 0.96 }),
                new TeamStrictAccessBadgeAssignmentAdjuster(
                    new Dictionary<string, double> { ["central"] = 1.0, ["south"] = 1.01 }),
                new CapStrictAccessBadgeAssignmentAdjuster(1777.0),
                new StrictAccessBadgeAssignmentBudgetOffsetAdjuster(0.27),
            },
            new List<string>(),
            3.6,
            99536.3);
    }

    public double Compute(StrictAccessBadgeAssignment item)
    {
        var current = item.WeightedValue();
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

    public Dictionary<string, double> EvaluateAll(List<StrictAccessBadgeAssignment> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
