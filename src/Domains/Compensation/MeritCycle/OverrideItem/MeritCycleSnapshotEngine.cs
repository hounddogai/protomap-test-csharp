namespace Acme.Workforce.Compensation.MeritCycle.OverrideItem;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the merit cycle snapshot calculation.</summary>
public interface IMeritCycleSnapshotStep
{
    string Describe();
    double Adjust(MeritCycleSnapshot item, double valueValue);
}

public sealed record PercentMeritCycleSnapshotAdjuster(double Percent) : IMeritCycleSnapshotStep
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(MeritCycleSnapshot item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record MeritCycleSnapshotVersionOffsetAdjuster(double Weight) : IMeritCycleSnapshotStep
{
    public string Describe()
    {
        return $"plus version times {Weight}";
    }

    public double Adjust(MeritCycleSnapshot item, double valueValue)
    {
        return valueValue + (double)item.Version * Weight;
    }
}

public sealed record RoundingMeritCycleSnapshotAdjuster(double Step) : IMeritCycleSnapshotStep
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(MeritCycleSnapshot item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

/// <summary>Runs the merit cycle snapshot steps in order and keeps a trace of each.</summary>
public sealed class MeritCycleSnapshotEngine(List<IMeritCycleSnapshotStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IMeritCycleSnapshotStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static MeritCycleSnapshotEngine DefaultMeritCycleSnapshotEngine()
    {
        return new MeritCycleSnapshotEngine(
            new List<IMeritCycleSnapshotStep>
            {
                new PercentMeritCycleSnapshotAdjuster(-23.0),
                new MeritCycleSnapshotVersionOffsetAdjuster(1.08),
                new RoundingMeritCycleSnapshotAdjuster(0.5),
            },
            new List<string>(),
            1.7,
            44210.8);
    }

    public double Evaluate(MeritCycleSnapshot item)
    {
        var current = item.ProjectedValue();
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

    public Dictionary<string, double> EvaluateAll(List<MeritCycleSnapshot> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
