namespace Acme.Workforce.Orgdesign.Position.Plan;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the position review calculation.</summary>
public interface IPositionReviewStep
{
    string Describe();
    double Adjust(PositionReview item, double valueValue);
}

public sealed record CapPositionReviewAdjuster(double Cap) : IPositionReviewStep
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(PositionReview item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record PositionReviewRetriesOffsetAdjuster(double Weight) : IPositionReviewStep
{
    public string Describe()
    {
        return $"plus retries times {Weight}";
    }

    public double Adjust(PositionReview item, double valueValue)
    {
        return valueValue + (double)item.Retries * Weight;
    }
}

public sealed record RoundingPositionReviewAdjuster(double Step) : IPositionReviewStep
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(PositionReview item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record FlaggedPositionReviewAdjuster(Registry Flags, string Flag, double Percent) : IPositionReviewStep
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(PositionReview item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record ChannelPositionReviewAdjuster(Dictionary<string, double> Factors) : IPositionReviewStep
{
    public string Describe()
    {
        return "by channel";
    }

    public double Adjust(PositionReview item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record MinimumPositionReviewAdjuster(double Minimum) : IPositionReviewStep
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(PositionReview item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

/// <summary>Runs the position review steps in order and keeps a trace of each.</summary>
public sealed class PositionReviewEngine(List<IPositionReviewStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IPositionReviewStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static PositionReviewEngine DefaultPositionReviewEngine(Registry flags)
    {
        return new PositionReviewEngine(
            new List<IPositionReviewStep>
            {
                new CapPositionReviewAdjuster(16158.0),
                new PositionReviewRetriesOffsetAdjuster(0.3),
                new RoundingPositionReviewAdjuster(0.5),
                new FlaggedPositionReviewAdjuster(flags, "position-review-weighted", -5.4),
                new ChannelPositionReviewAdjuster(new Dictionary<string, double> { ["central"] = 1.21 }),
                new MinimumPositionReviewAdjuster(13.2),
            },
            new List<string>(),
            1.6,
            87354.5);
    }

    public double Evaluate(PositionReview item)
    {
        var current = item.EffectiveValue();
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

    public Dictionary<string, double> EvaluateAll(List<PositionReview> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
