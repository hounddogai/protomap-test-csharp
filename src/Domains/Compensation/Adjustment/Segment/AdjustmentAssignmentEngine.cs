namespace Acme.Workforce.Compensation.Adjustment.Segment;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the adjustment assignment calculation.</summary>
public interface IAdjustmentAssignmentFactor
{
    string Describe();
    double Adjust(AdjustmentAssignment item, double valueValue);
}

public sealed record PercentAdjustmentAssignmentAdjuster(double Percent) : IAdjustmentAssignmentFactor
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(AdjustmentAssignment item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record CapAdjustmentAssignmentAdjuster(double Cap) : IAdjustmentAssignmentFactor
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(AdjustmentAssignment item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record RoundingAdjustmentAssignmentAdjuster(double Step) : IAdjustmentAssignmentFactor
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(AdjustmentAssignment item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record FlaggedAdjustmentAssignmentAdjuster(Registry Flags, string Flag, double Percent) : IAdjustmentAssignmentFactor
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(AdjustmentAssignment item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record TieredAdjustmentAssignmentAdjuster(List<double> Bounds, List<double> Rates) : IAdjustmentAssignmentFactor
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(AdjustmentAssignment item, double valueValue)
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

public sealed record CountryAdjustmentAssignmentAdjuster(Dictionary<string, double> Factors) : IAdjustmentAssignmentFactor
{
    public string Describe()
    {
        return "by country";
    }

    public double Adjust(AdjustmentAssignment item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

/// <summary>Runs the adjustment assignment steps in order and keeps a trace of each.</summary>
public sealed class AdjustmentAssignmentEngine(List<IAdjustmentAssignmentFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IAdjustmentAssignmentFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static AdjustmentAssignmentEngine DefaultAdjustmentAssignmentEngine(Registry flags)
    {
        return new AdjustmentAssignmentEngine(
            new List<IAdjustmentAssignmentFactor>
            {
                new PercentAdjustmentAssignmentAdjuster(-10.1),
                new CapAdjustmentAssignmentAdjuster(47194.0),
                new RoundingAdjustmentAssignmentAdjuster(0.5),
                new FlaggedAdjustmentAssignmentAdjuster(flags, "adjustment-assignment-default", 1.3),
                new TieredAdjustmentAssignmentAdjuster(
                    new List<double> { 4339.0, 6168.0, 7316.0 },
                    new List<double> { 1.03, 0.86, 0.9 }),
                new CountryAdjustmentAssignmentAdjuster(
                    new Dictionary<string, double> { ["online"] = 1.1, ["north"] = 1.34, ["south"] = 0.96 }),
            },
            new List<string>(),
            4.8,
            14273.4);
    }

    public double Price(AdjustmentAssignment item)
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

    public Dictionary<string, double> EvaluateAll(List<AdjustmentAssignment> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
