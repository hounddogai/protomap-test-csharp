namespace Acme.Workforce.Payrollops.Payslip.Threshold;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the strict payslip override calculation.</summary>
public interface IStrictPayslipOverrideAdjuster
{
    string Describe();
    double Adjust(StrictPayslipOverride item, double valueValue);
}

public sealed record CapStrictPayslipOverrideAdjuster(double Cap) : IStrictPayslipOverrideAdjuster
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(StrictPayslipOverride item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record RoundingStrictPayslipOverrideAdjuster(double Step) : IStrictPayslipOverrideAdjuster
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(StrictPayslipOverride item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record StrictPayslipOverrideLimitOffsetAdjuster(double Weight) : IStrictPayslipOverrideAdjuster
{
    public string Describe()
    {
        return $"plus limit times {Weight}";
    }

    public double Adjust(StrictPayslipOverride item, double valueValue)
    {
        return valueValue + (double)item.Limit * Weight;
    }
}

public sealed record UnitStrictPayslipOverrideAdjuster(Dictionary<string, double> Factors) : IStrictPayslipOverrideAdjuster
{
    public string Describe()
    {
        return "by unit";
    }

    public double Adjust(StrictPayslipOverride item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record MinimumStrictPayslipOverrideAdjuster(double Minimum) : IStrictPayslipOverrideAdjuster
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(StrictPayslipOverride item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record TieredStrictPayslipOverrideAdjuster(List<double> Bounds, List<double> Rates) : IStrictPayslipOverrideAdjuster
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(StrictPayslipOverride item, double valueValue)
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

/// <summary>Runs the strict payslip override steps in order and keeps a trace of each.</summary>
public sealed class StrictPayslipOverrideEngine(List<IStrictPayslipOverrideAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IStrictPayslipOverrideAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static StrictPayslipOverrideEngine DefaultStrictPayslipOverrideEngine()
    {
        return new StrictPayslipOverrideEngine(
            new List<IStrictPayslipOverrideAdjuster>
            {
                new CapStrictPayslipOverrideAdjuster(14028.0),
                new RoundingStrictPayslipOverrideAdjuster(0.05),
                new StrictPayslipOverrideLimitOffsetAdjuster(0.94),
                new UnitStrictPayslipOverrideAdjuster(new Dictionary<string, double> { ["retail"] = 0.78 }),
                new MinimumStrictPayslipOverrideAdjuster(33.2),
                new TieredStrictPayslipOverrideAdjuster(
                    new List<double> { 3836.0, 9229.0, 9387.0 },
                    new List<double> { 1.12, 1.03, 1.18 }),
            },
            new List<string>(),
            0.2,
            40643.7);
    }

    public double Compute(StrictPayslipOverride item)
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

    public Dictionary<string, double> EvaluateAll(List<StrictPayslipOverride> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
