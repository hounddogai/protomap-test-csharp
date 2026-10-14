namespace Acme.Workforce.Talent.SkillProfile.Summary;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the regional skill profile summary calculation.</summary>
public interface IRegionalSkillProfileSummaryModifier
{
    string Describe();
    double Adjust(RegionalSkillProfileSummary item, double valueValue);
}

public sealed record PercentRegionalSkillProfileSummaryAdjuster(double Percent) : IRegionalSkillProfileSummaryModifier
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(RegionalSkillProfileSummary item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record CapRegionalSkillProfileSummaryAdjuster(double Cap) : IRegionalSkillProfileSummaryModifier
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(RegionalSkillProfileSummary item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record FlaggedRegionalSkillProfileSummaryAdjuster(Registry Flags, string Flag, double Percent) : IRegionalSkillProfileSummaryModifier
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(RegionalSkillProfileSummary item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record NameRegionalSkillProfileSummaryAdjuster(Dictionary<string, double> Factors) : IRegionalSkillProfileSummaryModifier
{
    public string Describe()
    {
        return "by name";
    }

    public double Adjust(RegionalSkillProfileSummary item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record MinimumRegionalSkillProfileSummaryAdjuster(double Minimum) : IRegionalSkillProfileSummaryModifier
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(RegionalSkillProfileSummary item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record RoundingRegionalSkillProfileSummaryAdjuster(double Step) : IRegionalSkillProfileSummaryModifier
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(RegionalSkillProfileSummary item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

/// <summary>Runs the regional skill profile summary steps in order and keeps a trace of each.</summary>
public sealed class RegionalSkillProfileSummaryEngine(List<IRegionalSkillProfileSummaryModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IRegionalSkillProfileSummaryModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static RegionalSkillProfileSummaryEngine DefaultRegionalSkillProfileSummaryEngine(Registry flags)
    {
        return new RegionalSkillProfileSummaryEngine(
            new List<IRegionalSkillProfileSummaryModifier>
            {
                new PercentRegionalSkillProfileSummaryAdjuster(2.6),
                new CapRegionalSkillProfileSummaryAdjuster(20307.0),
                new FlaggedRegionalSkillProfileSummaryAdjuster(
                    flags,
                    "regional-skill-profile-summary-scheduled",
                    6.7),
                new NameRegionalSkillProfileSummaryAdjuster(new Dictionary<string, double> { ["west"] = 0.9 }),
                new MinimumRegionalSkillProfileSummaryAdjuster(39.6),
                new RoundingRegionalSkillProfileSummaryAdjuster(0.1),
            },
            new List<string>(),
            2.3,
            37832.9);
    }

    public double Run(RegionalSkillProfileSummary item)
    {
        var current = item.MeasuredValue();
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

    public Dictionary<string, double> EvaluateAll(List<RegionalSkillProfileSummary> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
