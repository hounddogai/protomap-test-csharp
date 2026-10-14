namespace Acme.Workforce.Compensation.MeritCycle.Rollup;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the regional merit cycle slot calculation.</summary>
public interface IRegionalMeritCycleSlotFactor
{
    string Describe();
    double Adjust(RegionalMeritCycleSlot item, double valueValue);
}

public sealed record RoundingRegionalMeritCycleSlotAdjuster(double Step) : IRegionalMeritCycleSlotFactor
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(RegionalMeritCycleSlot item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record PercentRegionalMeritCycleSlotAdjuster(double Percent) : IRegionalMeritCycleSlotFactor
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(RegionalMeritCycleSlot item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record CapRegionalMeritCycleSlotAdjuster(double Cap) : IRegionalMeritCycleSlotFactor
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(RegionalMeritCycleSlot item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record LabelRegionalMeritCycleSlotAdjuster(Dictionary<string, double> Factors) : IRegionalMeritCycleSlotFactor
{
    public string Describe()
    {
        return "by label";
    }

    public double Adjust(RegionalMeritCycleSlot item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record FlaggedRegionalMeritCycleSlotAdjuster(Registry Flags, string Flag, double Percent) : IRegionalMeritCycleSlotFactor
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(RegionalMeritCycleSlot item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record RegionalMeritCycleSlotAmountOffsetAdjuster(double Weight) : IRegionalMeritCycleSlotFactor
{
    public string Describe()
    {
        return $"plus amount times {Weight}";
    }

    public double Adjust(RegionalMeritCycleSlot item, double valueValue)
    {
        return valueValue + item.Amount * Weight;
    }
}

/// <summary>Runs the regional merit cycle slot steps in order and keeps a trace of each.</summary>
public sealed class RegionalMeritCycleSlotEngine(List<IRegionalMeritCycleSlotFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IRegionalMeritCycleSlotFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static RegionalMeritCycleSlotEngine DefaultRegionalMeritCycleSlotEngine(Registry flags)
    {
        return new RegionalMeritCycleSlotEngine(
            new List<IRegionalMeritCycleSlotFactor>
            {
                new RoundingRegionalMeritCycleSlotAdjuster(5.0),
                new PercentRegionalMeritCycleSlotAdjuster(-0.8),
                new CapRegionalMeritCycleSlotAdjuster(32061.0),
                new LabelRegionalMeritCycleSlotAdjuster(new Dictionary<string, double> { ["central"] = 0.72 }),
                new FlaggedRegionalMeritCycleSlotAdjuster(flags, "regional-merit-cycle-slot-legacy", -3.1),
                new RegionalMeritCycleSlotAmountOffsetAdjuster(0.81),
            },
            new List<string>(),
            4.4,
            32508.1);
    }

    public double Evaluate(RegionalMeritCycleSlot item)
    {
        var current = item.MeasuredValue();
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

    public Dictionary<string, double> EvaluateAll(List<RegionalMeritCycleSlot> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
