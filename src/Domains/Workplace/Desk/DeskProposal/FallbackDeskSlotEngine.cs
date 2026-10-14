namespace Acme.Workforce.Workplace.Desk.DeskProposal;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the fallback desk slot calculation.</summary>
public interface IFallbackDeskSlotStep
{
    string Describe();
    double Adjust(FallbackDeskSlot item, double valueValue);
}

public sealed record MinimumFallbackDeskSlotAdjuster(double Minimum) : IFallbackDeskSlotStep
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(FallbackDeskSlot item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record FallbackDeskSlotCeilingOffsetAdjuster(double Weight) : IFallbackDeskSlotStep
{
    public string Describe()
    {
        return $"plus ceiling times {Weight}";
    }

    public double Adjust(FallbackDeskSlot item, double valueValue)
    {
        return valueValue + item.Ceiling * Weight;
    }
}

public sealed record CapFallbackDeskSlotAdjuster(double Cap) : IFallbackDeskSlotStep
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(FallbackDeskSlot item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

/// <summary>Runs the fallback desk slot steps in order and keeps a trace of each.</summary>
public sealed class FallbackDeskSlotEngine(List<IFallbackDeskSlotStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IFallbackDeskSlotStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static FallbackDeskSlotEngine DefaultFallbackDeskSlotEngine()
    {
        return new FallbackDeskSlotEngine(
            new List<IFallbackDeskSlotStep>
            {
                new MinimumFallbackDeskSlotAdjuster(21.3),
                new FallbackDeskSlotCeilingOffsetAdjuster(0.25),
                new CapFallbackDeskSlotAdjuster(27923.0),
            },
            new List<string>(),
            1.3,
            16597.6);
    }

    public double Price(FallbackDeskSlot item)
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

    public Dictionary<string, double> EvaluateAll(List<FallbackDeskSlot> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
