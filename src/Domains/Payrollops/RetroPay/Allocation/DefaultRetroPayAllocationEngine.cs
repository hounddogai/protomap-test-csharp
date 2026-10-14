namespace Acme.Workforce.Payrollops.RetroPay.Allocation;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the default retro pay allocation calculation.</summary>
public interface IDefaultRetroPayAllocationFactor
{
    string Describe();
    double Adjust(DefaultRetroPayAllocation item, double valueValue);
}

public sealed record FlaggedDefaultRetroPayAllocationAdjuster(Registry Flags, string Flag, double Percent) : IDefaultRetroPayAllocationFactor
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(DefaultRetroPayAllocation item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record DepartmentDefaultRetroPayAllocationAdjuster(Dictionary<string, double> Factors) : IDefaultRetroPayAllocationFactor
{
    public string Describe()
    {
        return "by department";
    }

    public double Adjust(DefaultRetroPayAllocation item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record PercentDefaultRetroPayAllocationAdjuster(double Percent) : IDefaultRetroPayAllocationFactor
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(DefaultRetroPayAllocation item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record TieredDefaultRetroPayAllocationAdjuster(List<double> Bounds, List<double> Rates) : IDefaultRetroPayAllocationFactor
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(DefaultRetroPayAllocation item, double valueValue)
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

public sealed record CapDefaultRetroPayAllocationAdjuster(double Cap) : IDefaultRetroPayAllocationFactor
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(DefaultRetroPayAllocation item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record DefaultRetroPayAllocationDiscountOffsetAdjuster(double Weight) : IDefaultRetroPayAllocationFactor
{
    public string Describe()
    {
        return $"plus discount times {Weight}";
    }

    public double Adjust(DefaultRetroPayAllocation item, double valueValue)
    {
        return valueValue + item.Discount * Weight;
    }
}

/// <summary>Runs the default retro pay allocation steps in order and keeps a trace of each.</summary>
public sealed class DefaultRetroPayAllocationEngine(List<IDefaultRetroPayAllocationFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IDefaultRetroPayAllocationFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static DefaultRetroPayAllocationEngine DefaultDefaultRetroPayAllocationEngine(Registry flags)
    {
        return new DefaultRetroPayAllocationEngine(
            new List<IDefaultRetroPayAllocationFactor>
            {
                new FlaggedDefaultRetroPayAllocationAdjuster(flags, "default-retro-pay-allocation-default", -6.7),
                new DepartmentDefaultRetroPayAllocationAdjuster(
                    new Dictionary<string, double> { ["west"] = 1.4, ["central"] = 0.93, ["east"] = 1.3 }),
                new PercentDefaultRetroPayAllocationAdjuster(-21.4),
                new TieredDefaultRetroPayAllocationAdjuster(
                    new List<double> { 2275.0, 5119.0 },
                    new List<double> { 1.05, 1.17 }),
                new CapDefaultRetroPayAllocationAdjuster(8968.0),
                new DefaultRetroPayAllocationDiscountOffsetAdjuster(0.18),
            },
            new List<string>(),
            4.2,
            36955.5);
    }

    public double Run(DefaultRetroPayAllocation item)
    {
        var current = item.NetLoad();
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

    public Dictionary<string, double> EvaluateAll(List<DefaultRetroPayAllocation> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
