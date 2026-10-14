namespace Acme.Workforce.Talent.SkillProfile.ProfileSummary;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the tiered skill profile summary calculation.</summary>
public interface ITieredSkillProfileSummaryStep
{
    string Describe();
    double Adjust(TieredSkillProfileSummary item, double valueValue);
}

public sealed record FlaggedTieredSkillProfileSummaryAdjuster(Registry Flags, string Flag, double Percent) : ITieredSkillProfileSummaryStep
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(TieredSkillProfileSummary item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record TieredSkillProfileSummaryGraceDaysOffsetAdjuster(double Weight) : ITieredSkillProfileSummaryStep
{
    public string Describe()
    {
        return $"plus grace days times {Weight}";
    }

    public double Adjust(TieredSkillProfileSummary item, double valueValue)
    {
        return valueValue + (double)item.GraceDays * Weight;
    }
}

public sealed record PercentTieredSkillProfileSummaryAdjuster(double Percent) : ITieredSkillProfileSummaryStep
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(TieredSkillProfileSummary item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record TieredTieredSkillProfileSummaryAdjuster(List<double> Bounds, List<double> Rates) : ITieredSkillProfileSummaryStep
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(TieredSkillProfileSummary item, double valueValue)
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

/// <summary>Runs the tiered skill profile summary steps in order and keeps a trace of each.</summary>
public sealed class TieredSkillProfileSummaryEngine(List<ITieredSkillProfileSummaryStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<ITieredSkillProfileSummaryStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static TieredSkillProfileSummaryEngine DefaultTieredSkillProfileSummaryEngine(Registry flags)
    {
        return new TieredSkillProfileSummaryEngine(
            new List<ITieredSkillProfileSummaryStep>
            {
                new FlaggedTieredSkillProfileSummaryAdjuster(flags, "tiered-skill-profile-summary-lenient", -9.1),
                new TieredSkillProfileSummaryGraceDaysOffsetAdjuster(0.12),
                new PercentTieredSkillProfileSummaryAdjuster(-7.7),
                new TieredTieredSkillProfileSummaryAdjuster(
                    new List<double> { 1859.0, 4569.0 },
                    new List<double> { 0.88, 0.94 }),
            },
            new List<string>(),
            4.6,
            97012.0);
    }

    public double Compute(TieredSkillProfileSummary item)
    {
        var current = item.WeightedTotal();
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

    public Dictionary<string, double> EvaluateAll(List<TieredSkillProfileSummary> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
