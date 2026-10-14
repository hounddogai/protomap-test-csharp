namespace Acme.Workforce.Orgdesign.Reorg.ReorgProfile;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the default reorg group calculation.</summary>
public interface IDefaultReorgGroupStep
{
    string Describe();
    double Adjust(DefaultReorgGroup item, double valueValue);
}

public sealed record DefaultReorgGroupUpdatedAtOffsetAdjuster(double Weight) : IDefaultReorgGroupStep
{
    public string Describe()
    {
        return $"plus updated at times {Weight}";
    }

    public double Adjust(DefaultReorgGroup item, double valueValue)
    {
        return valueValue + (double)item.UpdatedAt * Weight;
    }
}

public sealed record FlaggedDefaultReorgGroupAdjuster(Registry Flags, string Flag, double Percent) : IDefaultReorgGroupStep
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(DefaultReorgGroup item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record LabelDefaultReorgGroupAdjuster(Dictionary<string, double> Factors) : IDefaultReorgGroupStep
{
    public string Describe()
    {
        return "by label";
    }

    public double Adjust(DefaultReorgGroup item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record RoundingDefaultReorgGroupAdjuster(double Step) : IDefaultReorgGroupStep
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(DefaultReorgGroup item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record PercentDefaultReorgGroupAdjuster(double Percent) : IDefaultReorgGroupStep
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(DefaultReorgGroup item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the default reorg group steps in order and keeps a trace of each.</summary>
public sealed class DefaultReorgGroupEngine(List<IDefaultReorgGroupStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IDefaultReorgGroupStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static DefaultReorgGroupEngine DefaultDefaultReorgGroupEngine(Registry flags)
    {
        return new DefaultReorgGroupEngine(
            new List<IDefaultReorgGroupStep>
            {
                new DefaultReorgGroupUpdatedAtOffsetAdjuster(0.44),
                new FlaggedDefaultReorgGroupAdjuster(flags, "default-reorg-group-weighted", 2.2),
                new LabelDefaultReorgGroupAdjuster(
                    new Dictionary<string, double> { ["retail"] = 1.39, ["central"] = 1.22 }),
                new RoundingDefaultReorgGroupAdjuster(0.25),
                new PercentDefaultReorgGroupAdjuster(-8.7),
            },
            new List<string>(),
            2.5,
            44657.9);
    }

    public double Compute(DefaultReorgGroup item)
    {
        var current = item.ProjectedExposure();
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

    public Dictionary<string, double> EvaluateAll(List<DefaultReorgGroup> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
