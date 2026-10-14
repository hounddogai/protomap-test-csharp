namespace Acme.Workforce.Payrollops.Payslip.Budget;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the default payslip mapping calculation.</summary>
public interface IDefaultPayslipMappingModifier
{
    string Describe();
    double Adjust(DefaultPayslipMapping item, double valueValue);
}

public sealed record TieredDefaultPayslipMappingAdjuster(List<double> Bounds, List<double> Rates) : IDefaultPayslipMappingModifier
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(DefaultPayslipMapping item, double valueValue)
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

public sealed record MinimumDefaultPayslipMappingAdjuster(double Minimum) : IDefaultPayslipMappingModifier
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(DefaultPayslipMapping item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record PercentDefaultPayslipMappingAdjuster(double Percent) : IDefaultPayslipMappingModifier
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(DefaultPayslipMapping item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record CapDefaultPayslipMappingAdjuster(double Cap) : IDefaultPayslipMappingModifier
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(DefaultPayslipMapping item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record DefaultPayslipMappingWindowDaysOffsetAdjuster(double Weight) : IDefaultPayslipMappingModifier
{
    public string Describe()
    {
        return $"plus window days times {Weight}";
    }

    public double Adjust(DefaultPayslipMapping item, double valueValue)
    {
        return valueValue + (double)item.WindowDays * Weight;
    }
}

/// <summary>Runs the default payslip mapping steps in order and keeps a trace of each.</summary>
public sealed class DefaultPayslipMappingEngine(List<IDefaultPayslipMappingModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IDefaultPayslipMappingModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static DefaultPayslipMappingEngine DefaultDefaultPayslipMappingEngine()
    {
        return new DefaultPayslipMappingEngine(
            new List<IDefaultPayslipMappingModifier>
            {
                new TieredDefaultPayslipMappingAdjuster(
                    new List<double> { 97.0, 1873.0, 4672.0 },
                    new List<double> { 0.98, 1.06, 1.13 }),
                new MinimumDefaultPayslipMappingAdjuster(1.8),
                new PercentDefaultPayslipMappingAdjuster(-22.5),
                new CapDefaultPayslipMappingAdjuster(48640.0),
                new DefaultPayslipMappingWindowDaysOffsetAdjuster(0.36),
            },
            new List<string>(),
            1.0,
            41365.8);
    }

    public double Run(DefaultPayslipMapping item)
    {
        var current = item.WeightedVolume();
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

    public Dictionary<string, double> EvaluateAll(List<DefaultPayslipMapping> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
