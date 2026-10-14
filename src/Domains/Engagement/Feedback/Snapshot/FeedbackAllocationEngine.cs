namespace Acme.Workforce.Engagement.Feedback.Snapshot;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the feedback allocation calculation.</summary>
public interface IFeedbackAllocationModifier
{
    string Describe();
    double Adjust(FeedbackAllocation item, double valueValue);
}

public sealed record FlaggedFeedbackAllocationAdjuster(Registry Flags, string Flag, double Percent) : IFeedbackAllocationModifier
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(FeedbackAllocation item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record RoundingFeedbackAllocationAdjuster(double Step) : IFeedbackAllocationModifier
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(FeedbackAllocation item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record MinimumFeedbackAllocationAdjuster(double Minimum) : IFeedbackAllocationModifier
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(FeedbackAllocation item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record TieredFeedbackAllocationAdjuster(List<double> Bounds, List<double> Rates) : IFeedbackAllocationModifier
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(FeedbackAllocation item, double valueValue)
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

/// <summary>Runs the feedback allocation steps in order and keeps a trace of each.</summary>
public sealed class FeedbackAllocationEngine(List<IFeedbackAllocationModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IFeedbackAllocationModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static FeedbackAllocationEngine DefaultFeedbackAllocationEngine(Registry flags)
    {
        return new FeedbackAllocationEngine(
            new List<IFeedbackAllocationModifier>
            {
                new FlaggedFeedbackAllocationAdjuster(flags, "feedback-allocation-strict", 7.9),
                new RoundingFeedbackAllocationAdjuster(0.05),
                new MinimumFeedbackAllocationAdjuster(8.1),
                new TieredFeedbackAllocationAdjuster(
                    new List<double> { 4359.0, 7940.0, 9651.0, 9982.0 },
                    new List<double> { 0.83, 1.13, 0.96, 0.85 }),
            },
            new List<string>(),
            2.7,
            35985.1);
    }

    public double Price(FeedbackAllocation item)
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

    public Dictionary<string, double> EvaluateAll(List<FeedbackAllocation> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
