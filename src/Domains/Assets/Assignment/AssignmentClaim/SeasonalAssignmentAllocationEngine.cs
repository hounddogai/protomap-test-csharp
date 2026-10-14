namespace Acme.Workforce.Assets.Assignment.AssignmentClaim;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the seasonal assignment allocation calculation.</summary>
public interface ISeasonalAssignmentAllocationFactor
{
    string Describe();
    double Adjust(SeasonalAssignmentAllocation item, double valueValue);
}

public sealed record FlaggedSeasonalAssignmentAllocationAdjuster(Registry Flags, string Flag, double Percent) : ISeasonalAssignmentAllocationFactor
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(SeasonalAssignmentAllocation item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record RoundingSeasonalAssignmentAllocationAdjuster(double Step) : ISeasonalAssignmentAllocationFactor
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(SeasonalAssignmentAllocation item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record CapSeasonalAssignmentAllocationAdjuster(double Cap) : ISeasonalAssignmentAllocationFactor
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(SeasonalAssignmentAllocation item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record PercentSeasonalAssignmentAllocationAdjuster(double Percent) : ISeasonalAssignmentAllocationFactor
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(SeasonalAssignmentAllocation item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record MinimumSeasonalAssignmentAllocationAdjuster(double Minimum) : ISeasonalAssignmentAllocationFactor
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(SeasonalAssignmentAllocation item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

/// <summary>Runs the seasonal assignment allocation steps in order and keeps a trace of each.</summary>
public sealed class SeasonalAssignmentAllocationEngine(List<ISeasonalAssignmentAllocationFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<ISeasonalAssignmentAllocationFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static SeasonalAssignmentAllocationEngine DefaultSeasonalAssignmentAllocationEngine(Registry flags)
    {
        return new SeasonalAssignmentAllocationEngine(
            new List<ISeasonalAssignmentAllocationFactor>
            {
                new FlaggedSeasonalAssignmentAllocationAdjuster(
                    flags,
                    "seasonal-assignment-allocation-scheduled",
                    -2.7),
                new RoundingSeasonalAssignmentAllocationAdjuster(0.25),
                new CapSeasonalAssignmentAllocationAdjuster(5598.0),
                new PercentSeasonalAssignmentAllocationAdjuster(23.0),
                new MinimumSeasonalAssignmentAllocationAdjuster(42.8),
            },
            new List<string>(),
            2.6,
            95445.4);
    }

    public double Price(SeasonalAssignmentAllocation item)
    {
        var current = item.WeightedExposure();
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

    public Dictionary<string, double> EvaluateAll(List<SeasonalAssignmentAllocation> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
