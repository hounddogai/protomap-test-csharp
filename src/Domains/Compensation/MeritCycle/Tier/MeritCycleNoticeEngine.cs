namespace Acme.Workforce.Compensation.MeritCycle.Tier;

/// <summary>One step of the merit cycle notice calculation.</summary>
public interface IMeritCycleNoticeAdjuster
{
    string Describe();
    double Adjust(MeritCycleNotice item, double valueValue);
}

public sealed record LabelMeritCycleNoticeAdjuster(Dictionary<string, double> Factors) : IMeritCycleNoticeAdjuster
{
    public string Describe()
    {
        return "by label";
    }

    public double Adjust(MeritCycleNotice item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record MinimumMeritCycleNoticeAdjuster(double Minimum) : IMeritCycleNoticeAdjuster
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(MeritCycleNotice item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record TieredMeritCycleNoticeAdjuster(List<double> Bounds, List<double> Rates) : IMeritCycleNoticeAdjuster
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(MeritCycleNotice item, double valueValue)
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

public sealed record MeritCycleNoticeMaximumOffsetAdjuster(double Weight) : IMeritCycleNoticeAdjuster
{
    public string Describe()
    {
        return $"plus maximum times {Weight}";
    }

    public double Adjust(MeritCycleNotice item, double valueValue)
    {
        return valueValue + item.Maximum * Weight;
    }
}

/// <summary>Runs the merit cycle notice steps in order and keeps a trace of each.</summary>
public sealed class MeritCycleNoticeEngine(List<IMeritCycleNoticeAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IMeritCycleNoticeAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static MeritCycleNoticeEngine DefaultMeritCycleNoticeEngine()
    {
        return new MeritCycleNoticeEngine(
            new List<IMeritCycleNoticeAdjuster>
            {
                new LabelMeritCycleNoticeAdjuster(
                    new Dictionary<string, double> { ["retail"] = 1.3, ["south"] = 1.38, ["north"] = 1.11 }),
                new MinimumMeritCycleNoticeAdjuster(46.1),
                new TieredMeritCycleNoticeAdjuster(
                    new List<double> { 2050.0, 2719.0, 9674.0 },
                    new List<double> { 0.93, 1.15, 1.2 }),
                new MeritCycleNoticeMaximumOffsetAdjuster(1.56),
            },
            new List<string>(),
            0.5,
            95900.3);
    }

    public double Compute(MeritCycleNotice item)
    {
        var current = item.WeightedTotal();
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

    public Dictionary<string, double> EvaluateAll(List<MeritCycleNotice> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
