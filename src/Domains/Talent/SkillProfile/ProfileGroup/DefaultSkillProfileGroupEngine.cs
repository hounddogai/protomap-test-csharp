namespace Acme.Workforce.Talent.SkillProfile.ProfileGroup;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the default skill profile group calculation.</summary>
public interface IDefaultSkillProfileGroupFactor
{
    string Describe();
    double Adjust(DefaultSkillProfileGroup item, double valueValue);
}

public sealed record PercentDefaultSkillProfileGroupAdjuster(double Percent) : IDefaultSkillProfileGroupFactor
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(DefaultSkillProfileGroup item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record RoundingDefaultSkillProfileGroupAdjuster(double Step) : IDefaultSkillProfileGroupFactor
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(DefaultSkillProfileGroup item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record MinimumDefaultSkillProfileGroupAdjuster(double Minimum) : IDefaultSkillProfileGroupFactor
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(DefaultSkillProfileGroup item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record DefaultSkillProfileGroupBatchSizeOffsetAdjuster(double Weight) : IDefaultSkillProfileGroupFactor
{
    public string Describe()
    {
        return $"plus batch size times {Weight}";
    }

    public double Adjust(DefaultSkillProfileGroup item, double valueValue)
    {
        return valueValue + (double)item.BatchSize * Weight;
    }
}

public sealed record TieredDefaultSkillProfileGroupAdjuster(List<double> Bounds, List<double> Rates) : IDefaultSkillProfileGroupFactor
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(DefaultSkillProfileGroup item, double valueValue)
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

public sealed record FlaggedDefaultSkillProfileGroupAdjuster(Registry Flags, string Flag, double Percent) : IDefaultSkillProfileGroupFactor
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(DefaultSkillProfileGroup item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the default skill profile group steps in order and keeps a trace of each.</summary>
public sealed class DefaultSkillProfileGroupEngine(List<IDefaultSkillProfileGroupFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IDefaultSkillProfileGroupFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static DefaultSkillProfileGroupEngine DefaultDefaultSkillProfileGroupEngine(Registry flags)
    {
        return new DefaultSkillProfileGroupEngine(
            new List<IDefaultSkillProfileGroupFactor>
            {
                new PercentDefaultSkillProfileGroupAdjuster(8.2),
                new RoundingDefaultSkillProfileGroupAdjuster(1.0),
                new MinimumDefaultSkillProfileGroupAdjuster(12.3),
                new DefaultSkillProfileGroupBatchSizeOffsetAdjuster(0.91),
                new TieredDefaultSkillProfileGroupAdjuster(
                    new List<double> { 298.0, 1224.0, 6469.0 },
                    new List<double> { 0.87, 1.02, 1.05 }),
                new FlaggedDefaultSkillProfileGroupAdjuster(flags, "default-skill-profile-group-rolling", 1.8),
            },
            new List<string>(),
            4.6,
            57889.4);
    }

    public double Compute(DefaultSkillProfileGroup item)
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

    public Dictionary<string, double> EvaluateAll(List<DefaultSkillProfileGroup> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
