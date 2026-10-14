namespace Acme.Workforce.Orgdesign.Headcount.HeadcountRecord;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the headcount snapshot calculation.</summary>
public interface IHeadcountSnapshotFactor
{
    string Describe();
    double Adjust(HeadcountSnapshot item, double valueValue);
}

public sealed record RoundingHeadcountSnapshotAdjuster(double Step) : IHeadcountSnapshotFactor
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(HeadcountSnapshot item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record MinimumHeadcountSnapshotAdjuster(double Minimum) : IHeadcountSnapshotFactor
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(HeadcountSnapshot item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record UnitHeadcountSnapshotAdjuster(Dictionary<string, double> Factors) : IHeadcountSnapshotFactor
{
    public string Describe()
    {
        return "by unit";
    }

    public double Adjust(HeadcountSnapshot item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record PercentHeadcountSnapshotAdjuster(double Percent) : IHeadcountSnapshotFactor
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(HeadcountSnapshot item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record TieredHeadcountSnapshotAdjuster(List<double> Bounds, List<double> Rates) : IHeadcountSnapshotFactor
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(HeadcountSnapshot item, double valueValue)
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

public sealed record HeadcountSnapshotVersionOffsetAdjuster(double Weight) : IHeadcountSnapshotFactor
{
    public string Describe()
    {
        return $"plus version times {Weight}";
    }

    public double Adjust(HeadcountSnapshot item, double valueValue)
    {
        return valueValue + (double)item.Version * Weight;
    }
}

/// <summary>Runs the headcount snapshot steps in order and keeps a trace of each.</summary>
public sealed class HeadcountSnapshotEngine(List<IHeadcountSnapshotFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IHeadcountSnapshotFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static HeadcountSnapshotEngine DefaultHeadcountSnapshotEngine()
    {
        return new HeadcountSnapshotEngine(
            new List<IHeadcountSnapshotFactor>
            {
                new RoundingHeadcountSnapshotAdjuster(5.0),
                new MinimumHeadcountSnapshotAdjuster(8.3),
                new UnitHeadcountSnapshotAdjuster(
                    new Dictionary<string, double> { ["south"] = 1.12, ["central"] = 1.11, ["north"] = 0.93 }),
                new PercentHeadcountSnapshotAdjuster(-18.3),
                new TieredHeadcountSnapshotAdjuster(
                    new List<double> { 2737.0, 8547.0 },
                    new List<double> { 1.09, 0.86 }),
                new HeadcountSnapshotVersionOffsetAdjuster(1.86),
            },
            new List<string>(),
            1.5,
            3225.2);
    }

    public double Run(HeadcountSnapshot item)
    {
        var current = item.ProjectedTotal();
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

    public Dictionary<string, double> EvaluateAll(List<HeadcountSnapshot> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
