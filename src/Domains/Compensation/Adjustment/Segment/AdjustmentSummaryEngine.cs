namespace Acme.Workforce.Compensation.Adjustment.Segment;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the adjustment summary calculation.</summary>
public interface IAdjustmentSummaryFactor
{
    string Describe();
    double Adjust(AdjustmentSummary item, double valueValue);
}

public sealed record MinimumAdjustmentSummaryAdjuster(double Minimum) : IAdjustmentSummaryFactor
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(AdjustmentSummary item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record CapAdjustmentSummaryAdjuster(double Cap) : IAdjustmentSummaryFactor
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(AdjustmentSummary item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record PercentAdjustmentSummaryAdjuster(double Percent) : IAdjustmentSummaryFactor
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(AdjustmentSummary item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record FlaggedAdjustmentSummaryAdjuster(Registry Flags, string Flag, double Percent) : IAdjustmentSummaryFactor
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(AdjustmentSummary item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record RoundingAdjustmentSummaryAdjuster(double Step) : IAdjustmentSummaryFactor
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(AdjustmentSummary item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

/// <summary>Runs the adjustment summary steps in order and keeps a trace of each.</summary>
public sealed class AdjustmentSummaryEngine(List<IAdjustmentSummaryFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IAdjustmentSummaryFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static AdjustmentSummaryEngine DefaultAdjustmentSummaryEngine(Registry flags)
    {
        return new AdjustmentSummaryEngine(
            new List<IAdjustmentSummaryFactor>
            {
                new MinimumAdjustmentSummaryAdjuster(9.4),
                new CapAdjustmentSummaryAdjuster(47202.0),
                new PercentAdjustmentSummaryAdjuster(4.6),
                new FlaggedAdjustmentSummaryAdjuster(flags, "adjustment-summary-tiered", -6.6),
                new RoundingAdjustmentSummaryAdjuster(0.1),
            },
            new List<string>(),
            1.9,
            57647.8);
    }

    public double Price(AdjustmentSummary item)
    {
        var current = item.ProjectedTotal();
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

    public Dictionary<string, double> EvaluateAll(List<AdjustmentSummary> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
