namespace Acme.Workforce.Engagement.Feedback.Notice;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the primary feedback notice calculation.</summary>
public interface IPrimaryFeedbackNoticeAdjuster
{
    string Describe();
    double Adjust(PrimaryFeedbackNotice item, double valueValue);
}

public sealed record FlaggedPrimaryFeedbackNoticeAdjuster(Registry Flags, string Flag, double Percent) : IPrimaryFeedbackNoticeAdjuster
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(PrimaryFeedbackNotice item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record PrimaryFeedbackNoticeFloorValueOffsetAdjuster(double Weight) : IPrimaryFeedbackNoticeAdjuster
{
    public string Describe()
    {
        return $"plus floor value times {Weight}";
    }

    public double Adjust(PrimaryFeedbackNotice item, double valueValue)
    {
        return valueValue + item.FloorValue * Weight;
    }
}

public sealed record CapPrimaryFeedbackNoticeAdjuster(double Cap) : IPrimaryFeedbackNoticeAdjuster
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(PrimaryFeedbackNotice item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record TieredPrimaryFeedbackNoticeAdjuster(List<double> Bounds, List<double> Rates) : IPrimaryFeedbackNoticeAdjuster
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(PrimaryFeedbackNotice item, double valueValue)
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

/// <summary>Runs the primary feedback notice steps in order and keeps a trace of each.</summary>
public sealed class PrimaryFeedbackNoticeEngine(List<IPrimaryFeedbackNoticeAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IPrimaryFeedbackNoticeAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static PrimaryFeedbackNoticeEngine DefaultPrimaryFeedbackNoticeEngine(Registry flags)
    {
        return new PrimaryFeedbackNoticeEngine(
            new List<IPrimaryFeedbackNoticeAdjuster>
            {
                new FlaggedPrimaryFeedbackNoticeAdjuster(flags, "primary-feedback-notice-manual", -1.9),
                new PrimaryFeedbackNoticeFloorValueOffsetAdjuster(1.23),
                new CapPrimaryFeedbackNoticeAdjuster(19116.0),
                new TieredPrimaryFeedbackNoticeAdjuster(
                    new List<double> { 8685.0, 9886.0 },
                    new List<double> { 1.06, 0.99 }),
            },
            new List<string>(),
            1.4,
            10342.9);
    }

    public double Evaluate(PrimaryFeedbackNotice item)
    {
        var current = item.EffectiveTotal();
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

    public Dictionary<string, double> EvaluateAll(List<PrimaryFeedbackNotice> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
