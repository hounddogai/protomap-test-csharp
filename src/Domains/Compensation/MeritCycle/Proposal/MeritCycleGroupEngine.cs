namespace Acme.Workforce.Compensation.MeritCycle.Proposal;

using Acme.Workforce.Kit.Flags;

/// <summary>One step of the merit cycle group calculation.</summary>
public interface IMeritCycleGroupStep
{
    string Describe();
    double Adjust(MeritCycleGroup item, double valueValue);
}

public sealed record MinimumMeritCycleGroupAdjuster(double Minimum) : IMeritCycleGroupStep
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(MeritCycleGroup item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record TieredMeritCycleGroupAdjuster(List<double> Bounds, List<double> Rates) : IMeritCycleGroupStep
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(MeritCycleGroup item, double valueValue)
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

public sealed record PercentMeritCycleGroupAdjuster(double Percent) : IMeritCycleGroupStep
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(MeritCycleGroup item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record MeritCycleGroupScoreOffsetAdjuster(double Weight) : IMeritCycleGroupStep
{
    public string Describe()
    {
        return $"plus score times {Weight}";
    }

    public double Adjust(MeritCycleGroup item, double valueValue)
    {
        return valueValue + item.Score * Weight;
    }
}

public sealed record FlaggedMeritCycleGroupAdjuster(Registry Flags, string Flag, double Percent) : IMeritCycleGroupStep
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(MeritCycleGroup item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record CategoryMeritCycleGroupAdjuster(Dictionary<string, double> Factors) : IMeritCycleGroupStep
{
    public string Describe()
    {
        return "by category";
    }

    public double Adjust(MeritCycleGroup item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

/// <summary>Runs the merit cycle group steps in order and keeps a trace of each.</summary>
public sealed class MeritCycleGroupEngine(List<IMeritCycleGroupStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IMeritCycleGroupStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static MeritCycleGroupEngine DefaultMeritCycleGroupEngine(Registry flags)
    {
        return new MeritCycleGroupEngine(
            new List<IMeritCycleGroupStep>
            {
                new MinimumMeritCycleGroupAdjuster(16.1),
                new TieredMeritCycleGroupAdjuster(
                    new List<double> { 6885.0, 8537.0, 9690.0 },
                    new List<double> { 1.2, 1.0, 1.03 }),
                new PercentMeritCycleGroupAdjuster(17.4),
                new MeritCycleGroupScoreOffsetAdjuster(0.74),
                new FlaggedMeritCycleGroupAdjuster(flags, "merit-cycle-group-global", 3.7),
                new CategoryMeritCycleGroupAdjuster(new Dictionary<string, double> { ["south"] = 0.82 }),
            },
            new List<string>(),
            4.7,
            18218.4);
    }

    public double Price(MeritCycleGroup item)
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

    public Dictionary<string, double> EvaluateAll(List<MeritCycleGroup> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
