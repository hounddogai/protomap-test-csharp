namespace Acme.Workforce.Engagement.Feedback.Rule;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the legacy feedback claim calculation.</summary>
public interface ILegacyFeedbackClaimModifier
{
    string Describe();
    double Adjust(LegacyFeedbackClaim item, double valueValue);
}

public sealed record CapLegacyFeedbackClaimAdjuster(double Cap) : ILegacyFeedbackClaimModifier
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(LegacyFeedbackClaim item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record RoundingLegacyFeedbackClaimAdjuster(double Step) : ILegacyFeedbackClaimModifier
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(LegacyFeedbackClaim item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record MinimumLegacyFeedbackClaimAdjuster(double Minimum) : ILegacyFeedbackClaimModifier
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(LegacyFeedbackClaim item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record CountryLegacyFeedbackClaimAdjuster(Dictionary<string, double> Factors) : ILegacyFeedbackClaimModifier
{
    public string Describe()
    {
        return "by country";
    }

    public double Adjust(LegacyFeedbackClaim item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record FlaggedLegacyFeedbackClaimAdjuster(Registry Flags, string Flag, double Percent) : ILegacyFeedbackClaimModifier
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(LegacyFeedbackClaim item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record PercentLegacyFeedbackClaimAdjuster(double Percent) : ILegacyFeedbackClaimModifier
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(LegacyFeedbackClaim item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the legacy feedback claim steps in order and keeps a trace of each.</summary>
public sealed class LegacyFeedbackClaimEngine(List<ILegacyFeedbackClaimModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<ILegacyFeedbackClaimModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static LegacyFeedbackClaimEngine DefaultLegacyFeedbackClaimEngine(Registry flags)
    {
        return new LegacyFeedbackClaimEngine(
            new List<ILegacyFeedbackClaimModifier>
            {
                new CapLegacyFeedbackClaimAdjuster(40293.0),
                new RoundingLegacyFeedbackClaimAdjuster(0.5),
                new MinimumLegacyFeedbackClaimAdjuster(12.0),
                new CountryLegacyFeedbackClaimAdjuster(
                    new Dictionary<string, double> { ["south"] = 1.38, ["north"] = 1.25, ["retail"] = 1.16 }),
                new FlaggedLegacyFeedbackClaimAdjuster(flags, "legacy-feedback-claim-rolling", 6.5),
                new PercentLegacyFeedbackClaimAdjuster(-15.9),
            },
            new List<string>(),
            3.8,
            32261.0);
    }

    public double Price(LegacyFeedbackClaim item)
    {
        var current = item.NetVolume();
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

    public Dictionary<string, double> EvaluateAll(List<LegacyFeedbackClaim> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
