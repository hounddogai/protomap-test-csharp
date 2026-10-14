namespace Acme.Workforce.Workplace.AccessBadge.Audit;

using Acme.Workforce.Kit.Flags;

/// <summary>One step of the rolling access badge batch calculation.</summary>
public interface IRollingAccessBadgeBatchModifier
{
    string Describe();
    double Adjust(RollingAccessBadgeBatch item, double valueValue);
}

public sealed record MinimumRollingAccessBadgeBatchAdjuster(double Minimum) : IRollingAccessBadgeBatchModifier
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(RollingAccessBadgeBatch item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record DepartmentRollingAccessBadgeBatchAdjuster(Dictionary<string, double> Factors) : IRollingAccessBadgeBatchModifier
{
    public string Describe()
    {
        return "by department";
    }

    public double Adjust(RollingAccessBadgeBatch item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record PercentRollingAccessBadgeBatchAdjuster(double Percent) : IRollingAccessBadgeBatchModifier
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(RollingAccessBadgeBatch item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record FlaggedRollingAccessBadgeBatchAdjuster(Registry Flags, string Flag, double Percent) : IRollingAccessBadgeBatchModifier
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(RollingAccessBadgeBatch item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the rolling access badge batch steps in order and keeps a trace of each.</summary>
public sealed class RollingAccessBadgeBatchEngine(List<IRollingAccessBadgeBatchModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IRollingAccessBadgeBatchModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static RollingAccessBadgeBatchEngine DefaultRollingAccessBadgeBatchEngine(Registry flags)
    {
        return new RollingAccessBadgeBatchEngine(
            new List<IRollingAccessBadgeBatchModifier>
            {
                new MinimumRollingAccessBadgeBatchAdjuster(39.6),
                new DepartmentRollingAccessBadgeBatchAdjuster(
                    new Dictionary<string, double> { ["north"] = 1.32, ["online"] = 1.23 }),
                new PercentRollingAccessBadgeBatchAdjuster(11.1),
                new FlaggedRollingAccessBadgeBatchAdjuster(flags, "rolling-access-badge-batch-rolling", 1.6),
            },
            new List<string>(),
            4.4,
            29512.1);
    }

    public double Run(RollingAccessBadgeBatch item)
    {
        var current = item.ProjectedValue();
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

    public Dictionary<string, double> EvaluateAll(List<RollingAccessBadgeBatch> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
