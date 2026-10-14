namespace Acme.Workforce.Assets.Warranty.Rollup;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the primary warranty rollup calculation.</summary>
public interface IPrimaryWarrantyRollupFactor
{
    string Describe();
    double Adjust(PrimaryWarrantyRollup item, double valueValue);
}

public sealed record MinimumPrimaryWarrantyRollupAdjuster(double Minimum) : IPrimaryWarrantyRollupFactor
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(PrimaryWarrantyRollup item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record PrimaryWarrantyRollupPriorityOffsetAdjuster(double Weight) : IPrimaryWarrantyRollupFactor
{
    public string Describe()
    {
        return $"plus priority times {Weight}";
    }

    public double Adjust(PrimaryWarrantyRollup item, double valueValue)
    {
        return valueValue + (double)item.Priority * Weight;
    }
}

public sealed record CapPrimaryWarrantyRollupAdjuster(double Cap) : IPrimaryWarrantyRollupFactor
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(PrimaryWarrantyRollup item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record RoundingPrimaryWarrantyRollupAdjuster(double Step) : IPrimaryWarrantyRollupFactor
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(PrimaryWarrantyRollup item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

/// <summary>Runs the primary warranty rollup steps in order and keeps a trace of each.</summary>
public sealed class PrimaryWarrantyRollupEngine(List<IPrimaryWarrantyRollupFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IPrimaryWarrantyRollupFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static PrimaryWarrantyRollupEngine DefaultPrimaryWarrantyRollupEngine()
    {
        return new PrimaryWarrantyRollupEngine(
            new List<IPrimaryWarrantyRollupFactor>
            {
                new MinimumPrimaryWarrantyRollupAdjuster(3.3),
                new PrimaryWarrantyRollupPriorityOffsetAdjuster(1.14),
                new CapPrimaryWarrantyRollupAdjuster(9321.0),
                new RoundingPrimaryWarrantyRollupAdjuster(0.5),
            },
            new List<string>(),
            4.5,
            59279.2);
    }

    public double Run(PrimaryWarrantyRollup item)
    {
        var current = item.EffectiveValue();
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

    public Dictionary<string, double> EvaluateAll(List<PrimaryWarrantyRollup> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
