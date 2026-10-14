namespace Acme.Workforce.Talent.Nomination.Cycle;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the adaptive nomination variance calculation.</summary>
public interface IAdaptiveNominationVarianceAdjuster
{
    string Describe();
    double Adjust(AdaptiveNominationVariance item, double valueValue);
}

public sealed record SourceAdaptiveNominationVarianceAdjuster(Dictionary<string, double> Factors) : IAdaptiveNominationVarianceAdjuster
{
    public string Describe()
    {
        return "by source";
    }

    public double Adjust(AdaptiveNominationVariance item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record CapAdaptiveNominationVarianceAdjuster(double Cap) : IAdaptiveNominationVarianceAdjuster
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(AdaptiveNominationVariance item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record FlaggedAdaptiveNominationVarianceAdjuster(Registry Flags, string Flag, double Percent) : IAdaptiveNominationVarianceAdjuster
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(AdaptiveNominationVariance item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record RoundingAdaptiveNominationVarianceAdjuster(double Step) : IAdaptiveNominationVarianceAdjuster
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(AdaptiveNominationVariance item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

/// <summary>Runs the adaptive nomination variance steps in order and keeps a trace of each.</summary>
public sealed class AdaptiveNominationVarianceEngine(List<IAdaptiveNominationVarianceAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IAdaptiveNominationVarianceAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static AdaptiveNominationVarianceEngine DefaultAdaptiveNominationVarianceEngine(Registry flags)
    {
        return new AdaptiveNominationVarianceEngine(
            new List<IAdaptiveNominationVarianceAdjuster>
            {
                new SourceAdaptiveNominationVarianceAdjuster(new Dictionary<string, double> { ["south"] = 1.0 }),
                new CapAdaptiveNominationVarianceAdjuster(14295.0),
                new FlaggedAdaptiveNominationVarianceAdjuster(flags, "adaptive-nomination-variance-manual", -5.0),
                new RoundingAdaptiveNominationVarianceAdjuster(5.0),
            },
            new List<string>(),
            1.0,
            20311.0);
    }

    public double Compute(AdaptiveNominationVariance item)
    {
        var current = item.WeightedTotal();
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

    public Dictionary<string, double> EvaluateAll(List<AdaptiveNominationVariance> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
