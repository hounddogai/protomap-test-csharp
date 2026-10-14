namespace Acme.Workforce.Absence.Accrual.Assignment;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the accrual allocation calculation.</summary>
public interface IAccrualAllocationAdjuster
{
    string Describe();
    double Adjust(AccrualAllocation item, double valueValue);
}

public sealed record CapAccrualAllocationAdjuster(double Cap) : IAccrualAllocationAdjuster
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(AccrualAllocation item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record AccrualAllocationAmountOffsetAdjuster(double Weight) : IAccrualAllocationAdjuster
{
    public string Describe()
    {
        return $"plus amount times {Weight}";
    }

    public double Adjust(AccrualAllocation item, double valueValue)
    {
        return valueValue + item.Amount * Weight;
    }
}

public sealed record UnitAccrualAllocationAdjuster(Dictionary<string, double> Factors) : IAccrualAllocationAdjuster
{
    public string Describe()
    {
        return "by unit";
    }

    public double Adjust(AccrualAllocation item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record MinimumAccrualAllocationAdjuster(double Minimum) : IAccrualAllocationAdjuster
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(AccrualAllocation item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record RoundingAccrualAllocationAdjuster(double Step) : IAccrualAllocationAdjuster
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(AccrualAllocation item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record TieredAccrualAllocationAdjuster(List<double> Bounds, List<double> Rates) : IAccrualAllocationAdjuster
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(AccrualAllocation item, double valueValue)
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

/// <summary>Runs the accrual allocation steps in order and keeps a trace of each.</summary>
public sealed class AccrualAllocationEngine(List<IAccrualAllocationAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IAccrualAllocationAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static AccrualAllocationEngine DefaultAccrualAllocationEngine()
    {
        return new AccrualAllocationEngine(
            new List<IAccrualAllocationAdjuster>
            {
                new CapAccrualAllocationAdjuster(3785.0),
                new AccrualAllocationAmountOffsetAdjuster(0.04),
                new UnitAccrualAllocationAdjuster(
                    new Dictionary<string, double> { ["online"] = 1.18, ["retail"] = 1.06, ["west"] = 1.13 }),
                new MinimumAccrualAllocationAdjuster(25.9),
                new RoundingAccrualAllocationAdjuster(0.5),
                new TieredAccrualAllocationAdjuster(
                    new List<double> { 1849.0, 4988.0, 5867.0, 6831.0 },
                    new List<double> { 1.01, 1.16, 0.91, 1.0 }),
            },
            new List<string>(),
            3.4,
            81563.3);
    }

    public double Run(AccrualAllocation item)
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

    public Dictionary<string, double> EvaluateAll(List<AccrualAllocation> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
