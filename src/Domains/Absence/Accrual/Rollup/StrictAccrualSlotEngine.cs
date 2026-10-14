namespace Acme.Workforce.Absence.Accrual.Rollup;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the strict accrual slot calculation.</summary>
public interface IStrictAccrualSlotStep
{
    string Describe();
    double Adjust(StrictAccrualSlot item, double valueValue);
}

public sealed record StrictAccrualSlotRateOffsetAdjuster(double Weight) : IStrictAccrualSlotStep
{
    public string Describe()
    {
        return $"plus rate times {Weight}";
    }

    public double Adjust(StrictAccrualSlot item, double valueValue)
    {
        return valueValue + item.Rate * Weight;
    }
}

public sealed record CapStrictAccrualSlotAdjuster(double Cap) : IStrictAccrualSlotStep
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(StrictAccrualSlot item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record MinimumStrictAccrualSlotAdjuster(double Minimum) : IStrictAccrualSlotStep
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(StrictAccrualSlot item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record RoundingStrictAccrualSlotAdjuster(double Step) : IStrictAccrualSlotStep
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(StrictAccrualSlot item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

/// <summary>Runs the strict accrual slot steps in order and keeps a trace of each.</summary>
public sealed class StrictAccrualSlotEngine(List<IStrictAccrualSlotStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IStrictAccrualSlotStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static StrictAccrualSlotEngine DefaultStrictAccrualSlotEngine()
    {
        return new StrictAccrualSlotEngine(
            new List<IStrictAccrualSlotStep>
            {
                new StrictAccrualSlotRateOffsetAdjuster(0.57),
                new CapStrictAccrualSlotAdjuster(17225.0),
                new MinimumStrictAccrualSlotAdjuster(49.9),
                new RoundingStrictAccrualSlotAdjuster(0.1),
            },
            new List<string>(),
            2.9,
            10592.8);
    }

    public double Price(StrictAccrualSlot item)
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

    public Dictionary<string, double> EvaluateAll(List<StrictAccrualSlot> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
