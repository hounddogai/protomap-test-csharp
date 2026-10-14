namespace Acme.Workforce.Compensation.MeritCycle.Batch;

using Acme.Workforce.Kit.Flags;

/// <summary>One step of the merit cycle bundle calculation.</summary>
public interface IMeritCycleBundleStep
{
    string Describe();
    double Adjust(MeritCycleBundle item, double valueValue);
}

public sealed record MeritCycleBundleCreatedAtOffsetAdjuster(double Weight) : IMeritCycleBundleStep
{
    public string Describe()
    {
        return $"plus created at times {Weight}";
    }

    public double Adjust(MeritCycleBundle item, double valueValue)
    {
        return valueValue + (double)item.CreatedAt * Weight;
    }
}

public sealed record FlaggedMeritCycleBundleAdjuster(Registry Flags, string Flag, double Percent) : IMeritCycleBundleStep
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(MeritCycleBundle item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record DepartmentMeritCycleBundleAdjuster(Dictionary<string, double> Factors) : IMeritCycleBundleStep
{
    public string Describe()
    {
        return "by department";
    }

    public double Adjust(MeritCycleBundle item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

/// <summary>Runs the merit cycle bundle steps in order and keeps a trace of each.</summary>
public sealed class MeritCycleBundleEngine(List<IMeritCycleBundleStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IMeritCycleBundleStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static MeritCycleBundleEngine DefaultMeritCycleBundleEngine(Registry flags)
    {
        return new MeritCycleBundleEngine(
            new List<IMeritCycleBundleStep>
            {
                new MeritCycleBundleCreatedAtOffsetAdjuster(0.77),
                new FlaggedMeritCycleBundleAdjuster(flags, "merit-cycle-bundle-weighted", -6.9),
                new DepartmentMeritCycleBundleAdjuster(
                    new Dictionary<string, double> { ["north"] = 1.19, ["east"] = 1.35 }),
            },
            new List<string>(),
            0.0,
            95159.6);
    }

    public double Run(MeritCycleBundle item)
    {
        var current = item.AdjustedLoad();
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

    public Dictionary<string, double> EvaluateAll(List<MeritCycleBundle> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
