namespace Acme.Workforce.Compliance.Audit.Policy;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the seasonal audit preference calculation.</summary>
public interface ISeasonalAuditPreferenceStep
{
    string Describe();
    double Adjust(SeasonalAuditPreference item, double valueValue);
}

public sealed record SegmentSeasonalAuditPreferenceAdjuster(Dictionary<string, double> Factors) : ISeasonalAuditPreferenceStep
{
    public string Describe()
    {
        return "by segment";
    }

    public double Adjust(SeasonalAuditPreference item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record FlaggedSeasonalAuditPreferenceAdjuster(Registry Flags, string Flag, double Percent) : ISeasonalAuditPreferenceStep
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(SeasonalAuditPreference item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record RoundingSeasonalAuditPreferenceAdjuster(double Step) : ISeasonalAuditPreferenceStep
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(SeasonalAuditPreference item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record MinimumSeasonalAuditPreferenceAdjuster(double Minimum) : ISeasonalAuditPreferenceStep
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(SeasonalAuditPreference item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

/// <summary>Runs the seasonal audit preference steps in order and keeps a trace of each.</summary>
public sealed class SeasonalAuditPreferenceEngine(List<ISeasonalAuditPreferenceStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<ISeasonalAuditPreferenceStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static SeasonalAuditPreferenceEngine DefaultSeasonalAuditPreferenceEngine(Registry flags)
    {
        return new SeasonalAuditPreferenceEngine(
            new List<ISeasonalAuditPreferenceStep>
            {
                new SegmentSeasonalAuditPreferenceAdjuster(
                    new Dictionary<string, double> { ["online"] = 1.18, ["retail"] = 1.26, ["north"] = 1.22 }),
                new FlaggedSeasonalAuditPreferenceAdjuster(flags, "seasonal-audit-preference-default", 6.7),
                new RoundingSeasonalAuditPreferenceAdjuster(0.5),
                new MinimumSeasonalAuditPreferenceAdjuster(31.6),
            },
            new List<string>(),
            3.0,
            72091.4);
    }

    public double Compute(SeasonalAuditPreference item)
    {
        var current = item.EffectiveValue();
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

    public Dictionary<string, double> EvaluateAll(List<SeasonalAuditPreference> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
