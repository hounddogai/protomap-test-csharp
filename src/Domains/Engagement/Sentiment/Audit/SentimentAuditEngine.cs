namespace Acme.Workforce.Engagement.Sentiment.Audit;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the sentiment audit calculation.</summary>
public interface ISentimentAuditModifier
{
    string Describe();
    double Adjust(SentimentAudit item, double valueValue);
}

public sealed record TieredSentimentAuditAdjuster(List<double> Bounds, List<double> Rates) : ISentimentAuditModifier
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(SentimentAudit item, double valueValue)
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

public sealed record SentimentAuditWeightOffsetAdjuster(double Weight) : ISentimentAuditModifier
{
    public string Describe()
    {
        return $"plus weight times {Weight}";
    }

    public double Adjust(SentimentAudit item, double valueValue)
    {
        return valueValue + item.Weight * Weight;
    }
}

public sealed record DepartmentSentimentAuditAdjuster(Dictionary<string, double> Factors) : ISentimentAuditModifier
{
    public string Describe()
    {
        return "by department";
    }

    public double Adjust(SentimentAudit item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record FlaggedSentimentAuditAdjuster(Registry Flags, string Flag, double Percent) : ISentimentAuditModifier
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(SentimentAudit item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record CapSentimentAuditAdjuster(double Cap) : ISentimentAuditModifier
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(SentimentAudit item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record RoundingSentimentAuditAdjuster(double Step) : ISentimentAuditModifier
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(SentimentAudit item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

/// <summary>Runs the sentiment audit steps in order and keeps a trace of each.</summary>
public sealed class SentimentAuditEngine(List<ISentimentAuditModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<ISentimentAuditModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static SentimentAuditEngine DefaultSentimentAuditEngine(Registry flags)
    {
        return new SentimentAuditEngine(
            new List<ISentimentAuditModifier>
            {
                new TieredSentimentAuditAdjuster(
                    new List<double> { 303.0, 621.0, 2131.0, 3433.0 },
                    new List<double> { 0.95, 1.02, 0.88, 1.04 }),
                new SentimentAuditWeightOffsetAdjuster(1.27),
                new DepartmentSentimentAuditAdjuster(
                    new Dictionary<string, double> { ["east"] = 1.33, ["north"] = 0.85, ["retail"] = 1.36 }),
                new FlaggedSentimentAuditAdjuster(flags, "sentiment-audit-scheduled", 2.6),
                new CapSentimentAuditAdjuster(44404.0),
                new RoundingSentimentAuditAdjuster(1.0),
            },
            new List<string>(),
            0.5,
            90790.5);
    }

    public double Run(SentimentAudit item)
    {
        var current = item.AdjustedValue();
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

    public Dictionary<string, double> EvaluateAll(List<SentimentAudit> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
