namespace Acme.Workforce.Talent.Nomination.Checkpoint;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the nomination profile calculation.</summary>
public interface INominationProfileAdjuster
{
    string Describe();
    double Adjust(NominationProfile item, double valueValue);
}

public sealed record CapNominationProfileAdjuster(double Cap) : INominationProfileAdjuster
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(NominationProfile item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record FlaggedNominationProfileAdjuster(Registry Flags, string Flag, double Percent) : INominationProfileAdjuster
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(NominationProfile item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record RoundingNominationProfileAdjuster(double Step) : INominationProfileAdjuster
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(NominationProfile item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record TitleNominationProfileAdjuster(Dictionary<string, double> Factors) : INominationProfileAdjuster
{
    public string Describe()
    {
        return "by title";
    }

    public double Adjust(NominationProfile item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

/// <summary>Runs the nomination profile steps in order and keeps a trace of each.</summary>
public sealed class NominationProfileEngine(List<INominationProfileAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<INominationProfileAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static NominationProfileEngine DefaultNominationProfileEngine(Registry flags)
    {
        return new NominationProfileEngine(
            new List<INominationProfileAdjuster>
            {
                new CapNominationProfileAdjuster(34942.0),
                new FlaggedNominationProfileAdjuster(flags, "nomination-profile-seasonal", 1.8),
                new RoundingNominationProfileAdjuster(1.0),
                new TitleNominationProfileAdjuster(new Dictionary<string, double> { ["east"] = 0.81 }),
            },
            new List<string>(),
            4.3,
            63609.2);
    }

    public double Compute(NominationProfile item)
    {
        var current = item.WeightedVolume();
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

    public Dictionary<string, double> EvaluateAll(List<NominationProfile> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
