namespace Acme.Workforce.Talent.Nomination.Adjustment;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the global nomination snapshot calculation.</summary>
public interface IGlobalNominationSnapshotAdjuster
{
    string Describe();
    double Adjust(GlobalNominationSnapshot item, double valueValue);
}

public sealed record TitleGlobalNominationSnapshotAdjuster(Dictionary<string, double> Factors) : IGlobalNominationSnapshotAdjuster
{
    public string Describe()
    {
        return "by title";
    }

    public double Adjust(GlobalNominationSnapshot item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record PercentGlobalNominationSnapshotAdjuster(double Percent) : IGlobalNominationSnapshotAdjuster
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(GlobalNominationSnapshot item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record RoundingGlobalNominationSnapshotAdjuster(double Step) : IGlobalNominationSnapshotAdjuster
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(GlobalNominationSnapshot item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

/// <summary>Runs the global nomination snapshot steps in order and keeps a trace of each.</summary>
public sealed class GlobalNominationSnapshotEngine(List<IGlobalNominationSnapshotAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IGlobalNominationSnapshotAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static GlobalNominationSnapshotEngine DefaultGlobalNominationSnapshotEngine()
    {
        return new GlobalNominationSnapshotEngine(
            new List<IGlobalNominationSnapshotAdjuster>
            {
                new TitleGlobalNominationSnapshotAdjuster(
                    new Dictionary<string, double> { ["south"] = 0.95, ["online"] = 0.88 }),
                new PercentGlobalNominationSnapshotAdjuster(-5.8),
                new RoundingGlobalNominationSnapshotAdjuster(1.0),
            },
            new List<string>(),
            2.8,
            42955.0);
    }

    public double Compute(GlobalNominationSnapshot item)
    {
        var current = item.MeasuredValue();
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

    public Dictionary<string, double> EvaluateAll(List<GlobalNominationSnapshot> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
