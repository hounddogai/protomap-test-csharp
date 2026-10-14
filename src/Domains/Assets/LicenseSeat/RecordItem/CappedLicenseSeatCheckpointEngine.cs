namespace Acme.Workforce.Assets.LicenseSeat.RecordItem;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the capped license seat checkpoint calculation.</summary>
public interface ICappedLicenseSeatCheckpointAdjuster
{
    string Describe();
    double Adjust(CappedLicenseSeatCheckpoint item, double valueValue);
}

public sealed record FlaggedCappedLicenseSeatCheckpointAdjuster(Registry Flags, string Flag, double Percent) : ICappedLicenseSeatCheckpointAdjuster
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(CappedLicenseSeatCheckpoint item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record MinimumCappedLicenseSeatCheckpointAdjuster(double Minimum) : ICappedLicenseSeatCheckpointAdjuster
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(CappedLicenseSeatCheckpoint item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record PercentCappedLicenseSeatCheckpointAdjuster(double Percent) : ICappedLicenseSeatCheckpointAdjuster
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(CappedLicenseSeatCheckpoint item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the capped license seat checkpoint steps in order and keeps a trace of each.</summary>
public sealed class CappedLicenseSeatCheckpointEngine(List<ICappedLicenseSeatCheckpointAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<ICappedLicenseSeatCheckpointAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static CappedLicenseSeatCheckpointEngine DefaultCappedLicenseSeatCheckpointEngine(Registry flags)
    {
        return new CappedLicenseSeatCheckpointEngine(
            new List<ICappedLicenseSeatCheckpointAdjuster>
            {
                new FlaggedCappedLicenseSeatCheckpointAdjuster(flags, "capped-license-seat-checkpoint-tiered", 0.7),
                new MinimumCappedLicenseSeatCheckpointAdjuster(28.3),
                new PercentCappedLicenseSeatCheckpointAdjuster(-8.5),
            },
            new List<string>(),
            2.4,
            99936.3);
    }

    public double Evaluate(CappedLicenseSeatCheckpoint item)
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

    public Dictionary<string, double> EvaluateAll(List<CappedLicenseSeatCheckpoint> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
