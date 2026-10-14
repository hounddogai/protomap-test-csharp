namespace Acme.Workforce.Talent.Nomination.Request;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the manual nomination score calculation.</summary>
public interface IManualNominationScoreFactor
{
    string Describe();
    double Adjust(ManualNominationScore item, double valueValue);
}

public sealed record PercentManualNominationScoreAdjuster(double Percent) : IManualNominationScoreFactor
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(ManualNominationScore item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record MinimumManualNominationScoreAdjuster(double Minimum) : IManualNominationScoreFactor
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(ManualNominationScore item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record CapManualNominationScoreAdjuster(double Cap) : IManualNominationScoreFactor
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(ManualNominationScore item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record CountryManualNominationScoreAdjuster(Dictionary<string, double> Factors) : IManualNominationScoreFactor
{
    public string Describe()
    {
        return "by country";
    }

    public double Adjust(ManualNominationScore item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record FlaggedManualNominationScoreAdjuster(Registry Flags, string Flag, double Percent) : IManualNominationScoreFactor
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(ManualNominationScore item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record RoundingManualNominationScoreAdjuster(double Step) : IManualNominationScoreFactor
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(ManualNominationScore item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

/// <summary>Runs the manual nomination score steps in order and keeps a trace of each.</summary>
public sealed class ManualNominationScoreEngine(List<IManualNominationScoreFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IManualNominationScoreFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static ManualNominationScoreEngine DefaultManualNominationScoreEngine(Registry flags)
    {
        return new ManualNominationScoreEngine(
            new List<IManualNominationScoreFactor>
            {
                new PercentManualNominationScoreAdjuster(-8.8),
                new MinimumManualNominationScoreAdjuster(31.5),
                new CapManualNominationScoreAdjuster(32839.0),
                new CountryManualNominationScoreAdjuster(new Dictionary<string, double> { ["retail"] = 0.92 }),
                new FlaggedManualNominationScoreAdjuster(flags, "manual-nomination-score-default", 8.9),
                new RoundingManualNominationScoreAdjuster(5.0),
            },
            new List<string>(),
            2.4,
            60475.3);
    }

    public double Compute(ManualNominationScore item)
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

    public Dictionary<string, double> EvaluateAll(List<ManualNominationScore> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
