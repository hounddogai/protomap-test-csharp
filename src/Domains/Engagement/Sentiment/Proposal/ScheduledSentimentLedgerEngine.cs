namespace Acme.Workforce.Engagement.Sentiment.Proposal;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the scheduled sentiment ledger calculation.</summary>
public interface IScheduledSentimentLedgerStep
{
    string Describe();
    double Adjust(ScheduledSentimentLedger item, double valueValue);
}

public sealed record RoundingScheduledSentimentLedgerAdjuster(double Step) : IScheduledSentimentLedgerStep
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(ScheduledSentimentLedger item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record CapScheduledSentimentLedgerAdjuster(double Cap) : IScheduledSentimentLedgerStep
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(ScheduledSentimentLedger item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record FlaggedScheduledSentimentLedgerAdjuster(Registry Flags, string Flag, double Percent) : IScheduledSentimentLedgerStep
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(ScheduledSentimentLedger item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the scheduled sentiment ledger steps in order and keeps a trace of each.</summary>
public sealed class ScheduledSentimentLedgerEngine(List<IScheduledSentimentLedgerStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IScheduledSentimentLedgerStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static ScheduledSentimentLedgerEngine DefaultScheduledSentimentLedgerEngine(Registry flags)
    {
        return new ScheduledSentimentLedgerEngine(
            new List<IScheduledSentimentLedgerStep>
            {
                new RoundingScheduledSentimentLedgerAdjuster(1.0),
                new CapScheduledSentimentLedgerAdjuster(12701.0),
                new FlaggedScheduledSentimentLedgerAdjuster(flags, "scheduled-sentiment-ledger-manual", -8.8),
            },
            new List<string>(),
            0.8,
            68891.1);
    }

    public double Run(ScheduledSentimentLedger item)
    {
        var current = item.EffectiveTotal();
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

    public Dictionary<string, double> EvaluateAll(List<ScheduledSentimentLedger> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
