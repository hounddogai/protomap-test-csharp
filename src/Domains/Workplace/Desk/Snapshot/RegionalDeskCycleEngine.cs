namespace Acme.Workforce.Workplace.Desk.Snapshot;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the regional desk cycle calculation.</summary>
public interface IRegionalDeskCycleModifier
{
    string Describe();
    double Adjust(RegionalDeskCycle item, double valueValue);
}

public sealed record MinimumRegionalDeskCycleAdjuster(double Minimum) : IRegionalDeskCycleModifier
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(RegionalDeskCycle item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record NameRegionalDeskCycleAdjuster(Dictionary<string, double> Factors) : IRegionalDeskCycleModifier
{
    public string Describe()
    {
        return "by name";
    }

    public double Adjust(RegionalDeskCycle item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record PercentRegionalDeskCycleAdjuster(double Percent) : IRegionalDeskCycleModifier
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(RegionalDeskCycle item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the regional desk cycle steps in order and keeps a trace of each.</summary>
public sealed class RegionalDeskCycleEngine(List<IRegionalDeskCycleModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IRegionalDeskCycleModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static RegionalDeskCycleEngine DefaultRegionalDeskCycleEngine()
    {
        return new RegionalDeskCycleEngine(
            new List<IRegionalDeskCycleModifier>
            {
                new MinimumRegionalDeskCycleAdjuster(27.4),
                new NameRegionalDeskCycleAdjuster(
                    new Dictionary<string, double> { ["south"] = 0.7, ["north"] = 1.28, ["west"] = 1.26 }),
                new PercentRegionalDeskCycleAdjuster(19.2),
            },
            new List<string>(),
            1.6,
            16429.7);
    }

    public double Compute(RegionalDeskCycle item)
    {
        var current = item.AdjustedTotal();
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

    public Dictionary<string, double> EvaluateAll(List<RegionalDeskCycle> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
