namespace Acme.Workforce.Orgdesign.Position.PositionBundle;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the capped position assignment calculation.</summary>
public interface ICappedPositionAssignmentModifier
{
    string Describe();
    double Adjust(CappedPositionAssignment item, double valueValue);
}

public sealed record FlaggedCappedPositionAssignmentAdjuster(Registry Flags, string Flag, double Percent) : ICappedPositionAssignmentModifier
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(CappedPositionAssignment item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record LocaleCappedPositionAssignmentAdjuster(Dictionary<string, double> Factors) : ICappedPositionAssignmentModifier
{
    public string Describe()
    {
        return "by locale";
    }

    public double Adjust(CappedPositionAssignment item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record CapCappedPositionAssignmentAdjuster(double Cap) : ICappedPositionAssignmentModifier
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(CappedPositionAssignment item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record TieredCappedPositionAssignmentAdjuster(List<double> Bounds, List<double> Rates) : ICappedPositionAssignmentModifier
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(CappedPositionAssignment item, double valueValue)
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

public sealed record PercentCappedPositionAssignmentAdjuster(double Percent) : ICappedPositionAssignmentModifier
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(CappedPositionAssignment item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the capped position assignment steps in order and keeps a trace of each.</summary>
public sealed class CappedPositionAssignmentEngine(List<ICappedPositionAssignmentModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<ICappedPositionAssignmentModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static CappedPositionAssignmentEngine DefaultCappedPositionAssignmentEngine(Registry flags)
    {
        return new CappedPositionAssignmentEngine(
            new List<ICappedPositionAssignmentModifier>
            {
                new FlaggedCappedPositionAssignmentAdjuster(flags, "capped-position-assignment-regional", -2.4),
                new LocaleCappedPositionAssignmentAdjuster(new Dictionary<string, double> { ["south"] = 0.71 }),
                new CapCappedPositionAssignmentAdjuster(4242.0),
                new TieredCappedPositionAssignmentAdjuster(
                    new List<double> { 1386.0, 1554.0, 2931.0, 4362.0 },
                    new List<double> { 1.19, 0.83, 1.05, 0.99 }),
                new PercentCappedPositionAssignmentAdjuster(-10.7),
            },
            new List<string>(),
            0.3,
            38249.9);
    }

    public double Compute(CappedPositionAssignment item)
    {
        var current = item.AdjustedLoad();
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

    public Dictionary<string, double> EvaluateAll(List<CappedPositionAssignment> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
