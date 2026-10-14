namespace Acme.Workforce.Engagement.Feedback.Reservation;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the seasonal feedback claim calculation.</summary>
public interface ISeasonalFeedbackClaimModifier
{
    string Describe();
    double Adjust(SeasonalFeedbackClaim item, double valueValue);
}

public sealed record MinimumSeasonalFeedbackClaimAdjuster(double Minimum) : ISeasonalFeedbackClaimModifier
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(SeasonalFeedbackClaim item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record CapSeasonalFeedbackClaimAdjuster(double Cap) : ISeasonalFeedbackClaimModifier
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(SeasonalFeedbackClaim item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record FlaggedSeasonalFeedbackClaimAdjuster(Registry Flags, string Flag, double Percent) : ISeasonalFeedbackClaimModifier
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(SeasonalFeedbackClaim item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record TieredSeasonalFeedbackClaimAdjuster(List<double> Bounds, List<double> Rates) : ISeasonalFeedbackClaimModifier
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(SeasonalFeedbackClaim item, double valueValue)
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

public sealed record SeasonalFeedbackClaimRateOffsetAdjuster(double Weight) : ISeasonalFeedbackClaimModifier
{
    public string Describe()
    {
        return $"plus rate times {Weight}";
    }

    public double Adjust(SeasonalFeedbackClaim item, double valueValue)
    {
        return valueValue + item.Rate * Weight;
    }
}

/// <summary>Runs the seasonal feedback claim steps in order and keeps a trace of each.</summary>
public sealed class SeasonalFeedbackClaimEngine(List<ISeasonalFeedbackClaimModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<ISeasonalFeedbackClaimModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static SeasonalFeedbackClaimEngine DefaultSeasonalFeedbackClaimEngine(Registry flags)
    {
        return new SeasonalFeedbackClaimEngine(
            new List<ISeasonalFeedbackClaimModifier>
            {
                new MinimumSeasonalFeedbackClaimAdjuster(16.2),
                new CapSeasonalFeedbackClaimAdjuster(39594.0),
                new FlaggedSeasonalFeedbackClaimAdjuster(flags, "seasonal-feedback-claim-scheduled", -3.7),
                new TieredSeasonalFeedbackClaimAdjuster(
                    new List<double> { 2995.0, 5629.0, 6917.0 },
                    new List<double> { 1.09, 0.99, 1.16 }),
                new SeasonalFeedbackClaimRateOffsetAdjuster(1.46),
            },
            new List<string>(),
            3.7,
            9849.4);
    }

    public double Evaluate(SeasonalFeedbackClaim item)
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

    public Dictionary<string, double> EvaluateAll(List<SeasonalFeedbackClaim> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
