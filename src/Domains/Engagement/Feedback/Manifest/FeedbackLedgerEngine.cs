namespace Acme.Workforce.Engagement.Feedback.Manifest;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the feedback ledger calculation.</summary>
public interface IFeedbackLedgerFactor
{
    string Describe();
    double Adjust(FeedbackLedger item, double valueValue);
}

public sealed record RoundingFeedbackLedgerAdjuster(double Step) : IFeedbackLedgerFactor
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(FeedbackLedger item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record FlaggedFeedbackLedgerAdjuster(Registry Flags, string Flag, double Percent) : IFeedbackLedgerFactor
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(FeedbackLedger item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record PercentFeedbackLedgerAdjuster(double Percent) : IFeedbackLedgerFactor
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(FeedbackLedger item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record LabelFeedbackLedgerAdjuster(Dictionary<string, double> Factors) : IFeedbackLedgerFactor
{
    public string Describe()
    {
        return "by label";
    }

    public double Adjust(FeedbackLedger item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record CapFeedbackLedgerAdjuster(double Cap) : IFeedbackLedgerFactor
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(FeedbackLedger item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record FeedbackLedgerRatingOffsetAdjuster(double Weight) : IFeedbackLedgerFactor
{
    public string Describe()
    {
        return $"plus rating times {Weight}";
    }

    public double Adjust(FeedbackLedger item, double valueValue)
    {
        return valueValue + item.Rating * Weight;
    }
}

/// <summary>Runs the feedback ledger steps in order and keeps a trace of each.</summary>
public sealed class FeedbackLedgerEngine(List<IFeedbackLedgerFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IFeedbackLedgerFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static FeedbackLedgerEngine DefaultFeedbackLedgerEngine(Registry flags)
    {
        return new FeedbackLedgerEngine(
            new List<IFeedbackLedgerFactor>
            {
                new RoundingFeedbackLedgerAdjuster(0.1),
                new FlaggedFeedbackLedgerAdjuster(flags, "feedback-ledger-pending", 5.9),
                new PercentFeedbackLedgerAdjuster(16.7),
                new LabelFeedbackLedgerAdjuster(new Dictionary<string, double> { ["west"] = 1.18, ["south"] = 1.01 }),
                new CapFeedbackLedgerAdjuster(33225.0),
                new FeedbackLedgerRatingOffsetAdjuster(1.55),
            },
            new List<string>(),
            2.3,
            55870.0);
    }

    public double Price(FeedbackLedger item)
    {
        var current = item.AdjustedTotal();
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

    public Dictionary<string, double> EvaluateAll(List<FeedbackLedger> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
