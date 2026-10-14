namespace Acme.Workforce.Orgdesign.Scenario.Snapshot;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the legacy scenario snapshot calculation.</summary>
public interface ILegacyScenarioSnapshotAdjuster
{
    string Describe();
    double Adjust(LegacyScenarioSnapshot item, double valueValue);
}

public sealed record PercentLegacyScenarioSnapshotAdjuster(double Percent) : ILegacyScenarioSnapshotAdjuster
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(LegacyScenarioSnapshot item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record ChannelLegacyScenarioSnapshotAdjuster(Dictionary<string, double> Factors) : ILegacyScenarioSnapshotAdjuster
{
    public string Describe()
    {
        return "by channel";
    }

    public double Adjust(LegacyScenarioSnapshot item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record MinimumLegacyScenarioSnapshotAdjuster(double Minimum) : ILegacyScenarioSnapshotAdjuster
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(LegacyScenarioSnapshot item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record FlaggedLegacyScenarioSnapshotAdjuster(Registry Flags, string Flag, double Percent) : ILegacyScenarioSnapshotAdjuster
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(LegacyScenarioSnapshot item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record LegacyScenarioSnapshotGraceDaysOffsetAdjuster(double Weight) : ILegacyScenarioSnapshotAdjuster
{
    public string Describe()
    {
        return $"plus grace days times {Weight}";
    }

    public double Adjust(LegacyScenarioSnapshot item, double valueValue)
    {
        return valueValue + (double)item.GraceDays * Weight;
    }
}

/// <summary>Runs the legacy scenario snapshot steps in order and keeps a trace of each.</summary>
public sealed class LegacyScenarioSnapshotEngine(List<ILegacyScenarioSnapshotAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<ILegacyScenarioSnapshotAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static LegacyScenarioSnapshotEngine DefaultLegacyScenarioSnapshotEngine(Registry flags)
    {
        return new LegacyScenarioSnapshotEngine(
            new List<ILegacyScenarioSnapshotAdjuster>
            {
                new PercentLegacyScenarioSnapshotAdjuster(7.3),
                new ChannelLegacyScenarioSnapshotAdjuster(new Dictionary<string, double> { ["central"] = 1.16 }),
                new MinimumLegacyScenarioSnapshotAdjuster(25.5),
                new FlaggedLegacyScenarioSnapshotAdjuster(flags, "legacy-scenario-snapshot-default", 2.1),
                new LegacyScenarioSnapshotGraceDaysOffsetAdjuster(0.05),
            },
            new List<string>(),
            4.1,
            85396.4);
    }

    public double Compute(LegacyScenarioSnapshot item)
    {
        var current = item.ProjectedVolume();
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

    public Dictionary<string, double> EvaluateAll(List<LegacyScenarioSnapshot> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
