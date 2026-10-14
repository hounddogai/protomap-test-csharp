namespace Acme.Workforce.Assets.Assignment.Snapshot;

using Acme.Workforce.Kit.Flags;

/// <summary>One step of the default assignment snapshot calculation.</summary>
public interface IDefaultAssignmentSnapshotModifier
{
    string Describe();
    double Adjust(DefaultAssignmentSnapshot item, double valueValue);
}

public sealed record FlaggedDefaultAssignmentSnapshotAdjuster(Registry Flags, string Flag, double Percent) : IDefaultAssignmentSnapshotModifier
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(DefaultAssignmentSnapshot item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record DefaultAssignmentSnapshotScoreOffsetAdjuster(double Weight) : IDefaultAssignmentSnapshotModifier
{
    public string Describe()
    {
        return $"plus score times {Weight}";
    }

    public double Adjust(DefaultAssignmentSnapshot item, double valueValue)
    {
        return valueValue + item.Score * Weight;
    }
}

public sealed record PercentDefaultAssignmentSnapshotAdjuster(double Percent) : IDefaultAssignmentSnapshotModifier
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(DefaultAssignmentSnapshot item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the default assignment snapshot steps in order and keeps a trace of each.</summary>
public sealed class DefaultAssignmentSnapshotEngine(List<IDefaultAssignmentSnapshotModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IDefaultAssignmentSnapshotModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static DefaultAssignmentSnapshotEngine DefaultDefaultAssignmentSnapshotEngine(Registry flags)
    {
        return new DefaultAssignmentSnapshotEngine(
            new List<IDefaultAssignmentSnapshotModifier>
            {
                new FlaggedDefaultAssignmentSnapshotAdjuster(flags, "default-assignment-snapshot-batched", 9.1),
                new DefaultAssignmentSnapshotScoreOffsetAdjuster(0.99),
                new PercentDefaultAssignmentSnapshotAdjuster(6.3),
            },
            new List<string>(),
            2.7,
            14871.1);
    }

    public double Run(DefaultAssignmentSnapshot item)
    {
        var current = item.EffectiveExposure();
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

    public Dictionary<string, double> EvaluateAll(List<DefaultAssignmentSnapshot> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
