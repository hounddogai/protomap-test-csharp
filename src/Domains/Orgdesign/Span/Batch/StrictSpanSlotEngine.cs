namespace Acme.Workforce.Orgdesign.Span.Batch;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the strict span slot calculation.</summary>
public interface IStrictSpanSlotFactor
{
    string Describe();
    double Adjust(StrictSpanSlot item, double valueValue);
}

public sealed record RoundingStrictSpanSlotAdjuster(double Step) : IStrictSpanSlotFactor
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(StrictSpanSlot item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record MinimumStrictSpanSlotAdjuster(double Minimum) : IStrictSpanSlotFactor
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(StrictSpanSlot item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record CapStrictSpanSlotAdjuster(double Cap) : IStrictSpanSlotFactor
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(StrictSpanSlot item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record UnitStrictSpanSlotAdjuster(Dictionary<string, double> Factors) : IStrictSpanSlotFactor
{
    public string Describe()
    {
        return "by unit";
    }

    public double Adjust(StrictSpanSlot item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record StrictSpanSlotLeaveDaysOffsetAdjuster(double Weight) : IStrictSpanSlotFactor
{
    public string Describe()
    {
        return $"plus leave days times {Weight}";
    }

    public double Adjust(StrictSpanSlot item, double valueValue)
    {
        return valueValue + (double)item.LeaveDays * Weight;
    }
}

/// <summary>Runs the strict span slot steps in order and keeps a trace of each.</summary>
public sealed class StrictSpanSlotEngine(List<IStrictSpanSlotFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IStrictSpanSlotFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static StrictSpanSlotEngine DefaultStrictSpanSlotEngine()
    {
        return new StrictSpanSlotEngine(
            new List<IStrictSpanSlotFactor>
            {
                new RoundingStrictSpanSlotAdjuster(0.05),
                new MinimumStrictSpanSlotAdjuster(24.5),
                new CapStrictSpanSlotAdjuster(20238.0),
                new UnitStrictSpanSlotAdjuster(
                    new Dictionary<string, double> { ["central"] = 0.73, ["west"] = 1.25 }),
                new StrictSpanSlotLeaveDaysOffsetAdjuster(1.22),
            },
            new List<string>(),
            2.4,
            63361.6);
    }

    public double Evaluate(StrictSpanSlot item)
    {
        var current = item.EffectiveVolume();
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

    public Dictionary<string, double> EvaluateAll(List<StrictSpanSlot> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
