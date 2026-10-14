namespace Acme.Workforce.Compensation.PayBand.Exception;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the scheduled pay band snapshot calculation.</summary>
public interface IScheduledPayBandSnapshotStep
{
    string Describe();
    double Adjust(ScheduledPayBandSnapshot item, double valueValue);
}

public sealed record ScheduledPayBandSnapshotWindowDaysOffsetAdjuster(double Weight) : IScheduledPayBandSnapshotStep
{
    public string Describe()
    {
        return $"plus window days times {Weight}";
    }

    public double Adjust(ScheduledPayBandSnapshot item, double valueValue)
    {
        return valueValue + (double)item.WindowDays * Weight;
    }
}

public sealed record RoundingScheduledPayBandSnapshotAdjuster(double Step) : IScheduledPayBandSnapshotStep
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(ScheduledPayBandSnapshot item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record PercentScheduledPayBandSnapshotAdjuster(double Percent) : IScheduledPayBandSnapshotStep
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(ScheduledPayBandSnapshot item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the scheduled pay band snapshot steps in order and keeps a trace of each.</summary>
public sealed class ScheduledPayBandSnapshotEngine(List<IScheduledPayBandSnapshotStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IScheduledPayBandSnapshotStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static ScheduledPayBandSnapshotEngine DefaultScheduledPayBandSnapshotEngine()
    {
        return new ScheduledPayBandSnapshotEngine(
            new List<IScheduledPayBandSnapshotStep>
            {
                new ScheduledPayBandSnapshotWindowDaysOffsetAdjuster(0.59),
                new RoundingScheduledPayBandSnapshotAdjuster(0.5),
                new PercentScheduledPayBandSnapshotAdjuster(-3.3),
            },
            new List<string>(),
            0.4,
            96764.2);
    }

    public double Evaluate(ScheduledPayBandSnapshot item)
    {
        var current = item.NetTotal();
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

    public Dictionary<string, double> EvaluateAll(List<ScheduledPayBandSnapshot> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
