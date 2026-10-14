namespace Acme.Workforce.Talent.SkillProfile.Group;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the regional skill profile group calculation.</summary>
public interface IRegionalSkillProfileGroupAdjuster
{
    string Describe();
    double Adjust(RegionalSkillProfileGroup item, double valueValue);
}

public sealed record TieredRegionalSkillProfileGroupAdjuster(List<double> Bounds, List<double> Rates) : IRegionalSkillProfileGroupAdjuster
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(RegionalSkillProfileGroup item, double valueValue)
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

public sealed record MinimumRegionalSkillProfileGroupAdjuster(double Minimum) : IRegionalSkillProfileGroupAdjuster
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(RegionalSkillProfileGroup item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record FlaggedRegionalSkillProfileGroupAdjuster(Registry Flags, string Flag, double Percent) : IRegionalSkillProfileGroupAdjuster
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(RegionalSkillProfileGroup item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record CapRegionalSkillProfileGroupAdjuster(double Cap) : IRegionalSkillProfileGroupAdjuster
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(RegionalSkillProfileGroup item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record ChannelRegionalSkillProfileGroupAdjuster(Dictionary<string, double> Factors) : IRegionalSkillProfileGroupAdjuster
{
    public string Describe()
    {
        return "by channel";
    }

    public double Adjust(RegionalSkillProfileGroup item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record RoundingRegionalSkillProfileGroupAdjuster(double Step) : IRegionalSkillProfileGroupAdjuster
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(RegionalSkillProfileGroup item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

/// <summary>Runs the regional skill profile group steps in order and keeps a trace of each.</summary>
public sealed class RegionalSkillProfileGroupEngine(List<IRegionalSkillProfileGroupAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IRegionalSkillProfileGroupAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static RegionalSkillProfileGroupEngine DefaultRegionalSkillProfileGroupEngine(Registry flags)
    {
        return new RegionalSkillProfileGroupEngine(
            new List<IRegionalSkillProfileGroupAdjuster>
            {
                new TieredRegionalSkillProfileGroupAdjuster(
                    new List<double> { 7377.0, 8322.0, 8537.0 },
                    new List<double> { 1.17, 1.16, 0.98 }),
                new MinimumRegionalSkillProfileGroupAdjuster(48.9),
                new FlaggedRegionalSkillProfileGroupAdjuster(flags, "regional-skill-profile-group-seasonal", 1.8),
                new CapRegionalSkillProfileGroupAdjuster(9630.0),
                new ChannelRegionalSkillProfileGroupAdjuster(new Dictionary<string, double> { ["south"] = 1.3 }),
                new RoundingRegionalSkillProfileGroupAdjuster(0.25),
            },
            new List<string>(),
            2.5,
            13927.9);
    }

    public double Compute(RegionalSkillProfileGroup item)
    {
        var current = item.WeightedLoad();
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

    public Dictionary<string, double> EvaluateAll(List<RegionalSkillProfileGroup> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
