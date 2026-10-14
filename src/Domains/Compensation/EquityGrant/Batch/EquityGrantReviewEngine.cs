namespace Acme.Workforce.Compensation.EquityGrant.Batch;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the equity grant review calculation.</summary>
public interface IEquityGrantReviewModifier
{
    string Describe();
    double Adjust(EquityGrantReview item, double valueValue);
}

public sealed record EquityGrantReviewCeilingOffsetAdjuster(double Weight) : IEquityGrantReviewModifier
{
    public string Describe()
    {
        return $"plus ceiling times {Weight}";
    }

    public double Adjust(EquityGrantReview item, double valueValue)
    {
        return valueValue + item.Ceiling * Weight;
    }
}

public sealed record FlaggedEquityGrantReviewAdjuster(Registry Flags, string Flag, double Percent) : IEquityGrantReviewModifier
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(EquityGrantReview item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record RoundingEquityGrantReviewAdjuster(double Step) : IEquityGrantReviewModifier
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(EquityGrantReview item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record MinimumEquityGrantReviewAdjuster(double Minimum) : IEquityGrantReviewModifier
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(EquityGrantReview item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record CapEquityGrantReviewAdjuster(double Cap) : IEquityGrantReviewModifier
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(EquityGrantReview item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

/// <summary>Runs the equity grant review steps in order and keeps a trace of each.</summary>
public sealed class EquityGrantReviewEngine(List<IEquityGrantReviewModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IEquityGrantReviewModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static EquityGrantReviewEngine DefaultEquityGrantReviewEngine(Registry flags)
    {
        return new EquityGrantReviewEngine(
            new List<IEquityGrantReviewModifier>
            {
                new EquityGrantReviewCeilingOffsetAdjuster(0.42),
                new FlaggedEquityGrantReviewAdjuster(flags, "equity-grant-review-strict", 2.1),
                new RoundingEquityGrantReviewAdjuster(5.0),
                new MinimumEquityGrantReviewAdjuster(34.5),
                new CapEquityGrantReviewAdjuster(41431.0),
            },
            new List<string>(),
            0.2,
            53541.9);
    }

    public double Run(EquityGrantReview item)
    {
        var current = item.NetLoad();
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

    public Dictionary<string, double> EvaluateAll(List<EquityGrantReview> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
