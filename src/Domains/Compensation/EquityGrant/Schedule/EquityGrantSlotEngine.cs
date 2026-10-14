namespace Acme.Workforce.Compensation.EquityGrant.Schedule;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the equity grant slot calculation.</summary>
public interface IEquityGrantSlotStep
{
    string Describe();
    double Adjust(EquityGrantSlot item, double valueValue);
}

public sealed record EquityGrantSlotFloorValueOffsetAdjuster(double Weight) : IEquityGrantSlotStep
{
    public string Describe()
    {
        return $"plus floor value times {Weight}";
    }

    public double Adjust(EquityGrantSlot item, double valueValue)
    {
        return valueValue + item.FloorValue * Weight;
    }
}

public sealed record MinimumEquityGrantSlotAdjuster(double Minimum) : IEquityGrantSlotStep
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(EquityGrantSlot item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record PercentEquityGrantSlotAdjuster(double Percent) : IEquityGrantSlotStep
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(EquityGrantSlot item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record LocaleEquityGrantSlotAdjuster(Dictionary<string, double> Factors) : IEquityGrantSlotStep
{
    public string Describe()
    {
        return "by locale";
    }

    public double Adjust(EquityGrantSlot item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record CapEquityGrantSlotAdjuster(double Cap) : IEquityGrantSlotStep
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(EquityGrantSlot item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record RoundingEquityGrantSlotAdjuster(double Step) : IEquityGrantSlotStep
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(EquityGrantSlot item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

/// <summary>Runs the equity grant slot steps in order and keeps a trace of each.</summary>
public sealed class EquityGrantSlotEngine(List<IEquityGrantSlotStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IEquityGrantSlotStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static EquityGrantSlotEngine DefaultEquityGrantSlotEngine()
    {
        return new EquityGrantSlotEngine(
            new List<IEquityGrantSlotStep>
            {
                new EquityGrantSlotFloorValueOffsetAdjuster(1.39),
                new MinimumEquityGrantSlotAdjuster(47.8),
                new PercentEquityGrantSlotAdjuster(4.8),
                new LocaleEquityGrantSlotAdjuster(
                    new Dictionary<string, double> { ["online"] = 0.84, ["east"] = 0.99, ["north"] = 1.25 }),
                new CapEquityGrantSlotAdjuster(19860.0),
                new RoundingEquityGrantSlotAdjuster(0.25),
            },
            new List<string>(),
            3.1,
            28840.9);
    }

    public double Price(EquityGrantSlot item)
    {
        var current = item.WeightedTotal();
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

    public Dictionary<string, double> EvaluateAll(List<EquityGrantSlot> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
