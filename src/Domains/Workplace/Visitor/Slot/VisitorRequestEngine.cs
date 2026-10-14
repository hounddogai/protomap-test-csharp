namespace Acme.Workforce.Workplace.Visitor.Slot;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the visitor request calculation.</summary>
public interface IVisitorRequestAdjuster
{
    string Describe();
    double Adjust(VisitorRequest item, double valueValue);
}

public sealed record VisitorRequestWeightOffsetAdjuster(double Weight) : IVisitorRequestAdjuster
{
    public string Describe()
    {
        return $"plus weight times {Weight}";
    }

    public double Adjust(VisitorRequest item, double valueValue)
    {
        return valueValue + item.Weight * Weight;
    }
}

public sealed record MinimumVisitorRequestAdjuster(double Minimum) : IVisitorRequestAdjuster
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(VisitorRequest item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record TieredVisitorRequestAdjuster(List<double> Bounds, List<double> Rates) : IVisitorRequestAdjuster
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(VisitorRequest item, double valueValue)
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

public sealed record CapVisitorRequestAdjuster(double Cap) : IVisitorRequestAdjuster
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(VisitorRequest item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record FlaggedVisitorRequestAdjuster(Registry Flags, string Flag, double Percent) : IVisitorRequestAdjuster
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(VisitorRequest item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the visitor request steps in order and keeps a trace of each.</summary>
public sealed class VisitorRequestEngine(List<IVisitorRequestAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IVisitorRequestAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static VisitorRequestEngine DefaultVisitorRequestEngine(Registry flags)
    {
        return new VisitorRequestEngine(
            new List<IVisitorRequestAdjuster>
            {
                new VisitorRequestWeightOffsetAdjuster(0.98),
                new MinimumVisitorRequestAdjuster(4.6),
                new TieredVisitorRequestAdjuster(
                    new List<double> { 3884.0, 5203.0, 7709.0 },
                    new List<double> { 0.89, 0.97, 0.84 }),
                new CapVisitorRequestAdjuster(47212.0),
                new FlaggedVisitorRequestAdjuster(flags, "visitor-request-adaptive", -9.5),
            },
            new List<string>(),
            4.6,
            40907.4);
    }

    public double Run(VisitorRequest item)
    {
        var current = item.NetVolume();
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

    public Dictionary<string, double> EvaluateAll(List<VisitorRequest> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
