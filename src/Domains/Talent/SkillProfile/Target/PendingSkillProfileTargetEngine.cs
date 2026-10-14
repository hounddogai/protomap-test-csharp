namespace Acme.Workforce.Talent.SkillProfile.Target;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the pending skill profile target calculation.</summary>
public interface IPendingSkillProfileTargetAdjuster
{
    string Describe();
    double Adjust(PendingSkillProfileTarget item, double valueValue);
}

public sealed record FlaggedPendingSkillProfileTargetAdjuster(Registry Flags, string Flag, double Percent) : IPendingSkillProfileTargetAdjuster
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(PendingSkillProfileTarget item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record MinimumPendingSkillProfileTargetAdjuster(double Minimum) : IPendingSkillProfileTargetAdjuster
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(PendingSkillProfileTarget item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record TieredPendingSkillProfileTargetAdjuster(List<double> Bounds, List<double> Rates) : IPendingSkillProfileTargetAdjuster
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(PendingSkillProfileTarget item, double valueValue)
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

public sealed record PercentPendingSkillProfileTargetAdjuster(double Percent) : IPendingSkillProfileTargetAdjuster
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(PendingSkillProfileTarget item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record RoundingPendingSkillProfileTargetAdjuster(double Step) : IPendingSkillProfileTargetAdjuster
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(PendingSkillProfileTarget item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

/// <summary>Runs the pending skill profile target steps in order and keeps a trace of each.</summary>
public sealed class PendingSkillProfileTargetEngine(List<IPendingSkillProfileTargetAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IPendingSkillProfileTargetAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static PendingSkillProfileTargetEngine DefaultPendingSkillProfileTargetEngine(Registry flags)
    {
        return new PendingSkillProfileTargetEngine(
            new List<IPendingSkillProfileTargetAdjuster>
            {
                new FlaggedPendingSkillProfileTargetAdjuster(flags, "pending-skill-profile-target-fallback", 9.6),
                new MinimumPendingSkillProfileTargetAdjuster(31.8),
                new TieredPendingSkillProfileTargetAdjuster(
                    new List<double> { 1169.0, 2446.0, 4489.0, 6676.0 },
                    new List<double> { 1.11, 0.92, 1.04, 0.83 }),
                new PercentPendingSkillProfileTargetAdjuster(15.9),
                new RoundingPendingSkillProfileTargetAdjuster(0.1),
            },
            new List<string>(),
            4.3,
            53674.4);
    }

    public double Price(PendingSkillProfileTarget item)
    {
        var current = item.ProjectedLoad();
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

    public Dictionary<string, double> EvaluateAll(List<PendingSkillProfileTarget> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
