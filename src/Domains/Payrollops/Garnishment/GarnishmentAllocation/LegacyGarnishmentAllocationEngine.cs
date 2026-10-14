namespace Acme.Workforce.Payrollops.Garnishment.GarnishmentAllocation;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the legacy garnishment allocation calculation.</summary>
public interface ILegacyGarnishmentAllocationFactor
{
    string Describe();
    double Adjust(LegacyGarnishmentAllocation item, double valueValue);
}

public sealed record MinimumLegacyGarnishmentAllocationAdjuster(double Minimum) : ILegacyGarnishmentAllocationFactor
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(LegacyGarnishmentAllocation item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record PercentLegacyGarnishmentAllocationAdjuster(double Percent) : ILegacyGarnishmentAllocationFactor
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(LegacyGarnishmentAllocation item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record LegacyGarnishmentAllocationMarginOffsetAdjuster(double Weight) : ILegacyGarnishmentAllocationFactor
{
    public string Describe()
    {
        return $"plus margin times {Weight}";
    }

    public double Adjust(LegacyGarnishmentAllocation item, double valueValue)
    {
        return valueValue + item.Margin * Weight;
    }
}

public sealed record CapLegacyGarnishmentAllocationAdjuster(double Cap) : ILegacyGarnishmentAllocationFactor
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(LegacyGarnishmentAllocation item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

/// <summary>Runs the legacy garnishment allocation steps in order and keeps a trace of each.</summary>
public sealed class LegacyGarnishmentAllocationEngine(List<ILegacyGarnishmentAllocationFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<ILegacyGarnishmentAllocationFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static LegacyGarnishmentAllocationEngine DefaultLegacyGarnishmentAllocationEngine()
    {
        return new LegacyGarnishmentAllocationEngine(
            new List<ILegacyGarnishmentAllocationFactor>
            {
                new MinimumLegacyGarnishmentAllocationAdjuster(7.9),
                new PercentLegacyGarnishmentAllocationAdjuster(-19.7),
                new LegacyGarnishmentAllocationMarginOffsetAdjuster(0.51),
                new CapLegacyGarnishmentAllocationAdjuster(21671.0),
            },
            new List<string>(),
            3.4,
            17156.9);
    }

    public double Price(LegacyGarnishmentAllocation item)
    {
        var current = item.WeightedValue();
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

    public Dictionary<string, double> EvaluateAll(List<LegacyGarnishmentAllocation> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
