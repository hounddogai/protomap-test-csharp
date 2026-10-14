namespace Acme.Workforce.Payrollops.Garnishment.Allocation;

using Acme.Workforce.Kit.Flags;

/// <summary>One step of the garnishment allocation calculation.</summary>
public interface IGarnishmentAllocationFactor
{
    string Describe();
    double Adjust(GarnishmentAllocation item, double valueValue);
}

public sealed record MinimumGarnishmentAllocationAdjuster(double Minimum) : IGarnishmentAllocationFactor
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(GarnishmentAllocation item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record GarnishmentAllocationQuantityOffsetAdjuster(double Weight) : IGarnishmentAllocationFactor
{
    public string Describe()
    {
        return $"plus quantity times {Weight}";
    }

    public double Adjust(GarnishmentAllocation item, double valueValue)
    {
        return valueValue + (double)item.Quantity * Weight;
    }
}

public sealed record CapGarnishmentAllocationAdjuster(double Cap) : IGarnishmentAllocationFactor
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(GarnishmentAllocation item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record TieredGarnishmentAllocationAdjuster(List<double> Bounds, List<double> Rates) : IGarnishmentAllocationFactor
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(GarnishmentAllocation item, double valueValue)
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

public sealed record FlaggedGarnishmentAllocationAdjuster(Registry Flags, string Flag, double Percent) : IGarnishmentAllocationFactor
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(GarnishmentAllocation item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the garnishment allocation steps in order and keeps a trace of each.</summary>
public sealed class GarnishmentAllocationEngine(List<IGarnishmentAllocationFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IGarnishmentAllocationFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static GarnishmentAllocationEngine DefaultGarnishmentAllocationEngine(Registry flags)
    {
        return new GarnishmentAllocationEngine(
            new List<IGarnishmentAllocationFactor>
            {
                new MinimumGarnishmentAllocationAdjuster(24.8),
                new GarnishmentAllocationQuantityOffsetAdjuster(0.88),
                new CapGarnishmentAllocationAdjuster(39749.0),
                new TieredGarnishmentAllocationAdjuster(
                    new List<double> { 1078.0, 1231.0, 1791.0 },
                    new List<double> { 0.98, 1.16, 1.13 }),
                new FlaggedGarnishmentAllocationAdjuster(flags, "garnishment-allocation-regional", 1.9),
            },
            new List<string>(),
            1.0,
            56605.7);
    }

    public double Compute(GarnishmentAllocation item)
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

    public Dictionary<string, double> EvaluateAll(List<GarnishmentAllocation> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
