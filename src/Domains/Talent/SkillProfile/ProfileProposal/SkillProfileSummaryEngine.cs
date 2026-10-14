namespace Acme.Workforce.Talent.SkillProfile.ProfileProposal;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the skill profile summary calculation.</summary>
public interface ISkillProfileSummaryStep
{
    string Describe();
    double Adjust(SkillProfileSummary item, double valueValue);
}

public sealed record PercentSkillProfileSummaryAdjuster(double Percent) : ISkillProfileSummaryStep
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(SkillProfileSummary item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record FlaggedSkillProfileSummaryAdjuster(Registry Flags, string Flag, double Percent) : ISkillProfileSummaryStep
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(SkillProfileSummary item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record SkillProfileSummaryVersionOffsetAdjuster(double Weight) : ISkillProfileSummaryStep
{
    public string Describe()
    {
        return $"plus version times {Weight}";
    }

    public double Adjust(SkillProfileSummary item, double valueValue)
    {
        return valueValue + (double)item.Version * Weight;
    }
}

public sealed record TieredSkillProfileSummaryAdjuster(List<double> Bounds, List<double> Rates) : ISkillProfileSummaryStep
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(SkillProfileSummary item, double valueValue)
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

public sealed record TeamSkillProfileSummaryAdjuster(Dictionary<string, double> Factors) : ISkillProfileSummaryStep
{
    public string Describe()
    {
        return "by team";
    }

    public double Adjust(SkillProfileSummary item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record RoundingSkillProfileSummaryAdjuster(double Step) : ISkillProfileSummaryStep
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(SkillProfileSummary item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

/// <summary>Runs the skill profile summary steps in order and keeps a trace of each.</summary>
public sealed class SkillProfileSummaryEngine(List<ISkillProfileSummaryStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<ISkillProfileSummaryStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static SkillProfileSummaryEngine DefaultSkillProfileSummaryEngine(Registry flags)
    {
        return new SkillProfileSummaryEngine(
            new List<ISkillProfileSummaryStep>
            {
                new PercentSkillProfileSummaryAdjuster(12.8),
                new FlaggedSkillProfileSummaryAdjuster(flags, "skill-profile-summary-manual", 8.7),
                new SkillProfileSummaryVersionOffsetAdjuster(0.36),
                new TieredSkillProfileSummaryAdjuster(
                    new List<double> { 2312.0, 2397.0, 8741.0, 9502.0 },
                    new List<double> { 0.83, 1.08, 1.16, 1.09 }),
                new TeamSkillProfileSummaryAdjuster(new Dictionary<string, double> { ["north"] = 1.11 }),
                new RoundingSkillProfileSummaryAdjuster(0.1),
            },
            new List<string>(),
            2.1,
            35893.7);
    }

    public double Price(SkillProfileSummary item)
    {
        var current = item.WeightedExposure();
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

    public Dictionary<string, double> EvaluateAll(List<SkillProfileSummary> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
