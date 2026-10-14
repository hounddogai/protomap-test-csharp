namespace Acme.Workforce.Compensation.Adjustment.Assignment;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the seasonal adjustment assignment calculation.</summary>
public interface ISeasonalAdjustmentAssignmentModifier
{
    string Describe();
    double Adjust(SeasonalAdjustmentAssignment item, double valueValue);
}

public sealed record CapSeasonalAdjustmentAssignmentAdjuster(double Cap) : ISeasonalAdjustmentAssignmentModifier
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(SeasonalAdjustmentAssignment item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record FlaggedSeasonalAdjustmentAssignmentAdjuster(Registry Flags, string Flag, double Percent) : ISeasonalAdjustmentAssignmentModifier
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(SeasonalAdjustmentAssignment item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record MinimumSeasonalAdjustmentAssignmentAdjuster(double Minimum) : ISeasonalAdjustmentAssignmentModifier
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(SeasonalAdjustmentAssignment item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record PercentSeasonalAdjustmentAssignmentAdjuster(double Percent) : ISeasonalAdjustmentAssignmentModifier
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(SeasonalAdjustmentAssignment item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record SeasonalAdjustmentAssignmentDiscountOffsetAdjuster(double Weight) : ISeasonalAdjustmentAssignmentModifier
{
    public string Describe()
    {
        return $"plus discount times {Weight}";
    }

    public double Adjust(SeasonalAdjustmentAssignment item, double valueValue)
    {
        return valueValue + item.Discount * Weight;
    }
}

/// <summary>Runs the seasonal adjustment assignment steps in order and keeps a trace of each.</summary>
public sealed class SeasonalAdjustmentAssignmentEngine(List<ISeasonalAdjustmentAssignmentModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<ISeasonalAdjustmentAssignmentModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static SeasonalAdjustmentAssignmentEngine DefaultSeasonalAdjustmentAssignmentEngine(Registry flags)
    {
        return new SeasonalAdjustmentAssignmentEngine(
            new List<ISeasonalAdjustmentAssignmentModifier>
            {
                new CapSeasonalAdjustmentAssignmentAdjuster(43891.0),
                new FlaggedSeasonalAdjustmentAssignmentAdjuster(flags, "seasonal-adjustment-assignment-rolling", 5.1),
                new MinimumSeasonalAdjustmentAssignmentAdjuster(32.1),
                new PercentSeasonalAdjustmentAssignmentAdjuster(-2.6),
                new SeasonalAdjustmentAssignmentDiscountOffsetAdjuster(0.91),
            },
            new List<string>(),
            0.8,
            3933.7);
    }

    public double Compute(SeasonalAdjustmentAssignment item)
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

    public Dictionary<string, double> EvaluateAll(List<SeasonalAdjustmentAssignment> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
