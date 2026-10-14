namespace Acme.Workforce.Payrollops.Payslip.PayslipAssignment;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the lenient payslip rollup calculation.</summary>
public interface ILenientPayslipRollupFactor
{
    string Describe();
    double Adjust(LenientPayslipRollup item, double valueValue);
}

public sealed record TieredLenientPayslipRollupAdjuster(List<double> Bounds, List<double> Rates) : ILenientPayslipRollupFactor
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(LenientPayslipRollup item, double valueValue)
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

public sealed record PercentLenientPayslipRollupAdjuster(double Percent) : ILenientPayslipRollupFactor
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(LenientPayslipRollup item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record LenientPayslipRollupLeaveDaysOffsetAdjuster(double Weight) : ILenientPayslipRollupFactor
{
    public string Describe()
    {
        return $"plus leave days times {Weight}";
    }

    public double Adjust(LenientPayslipRollup item, double valueValue)
    {
        return valueValue + (double)item.LeaveDays * Weight;
    }
}

public sealed record RoundingLenientPayslipRollupAdjuster(double Step) : ILenientPayslipRollupFactor
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(LenientPayslipRollup item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record TitleLenientPayslipRollupAdjuster(Dictionary<string, double> Factors) : ILenientPayslipRollupFactor
{
    public string Describe()
    {
        return "by title";
    }

    public double Adjust(LenientPayslipRollup item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

/// <summary>Runs the lenient payslip rollup steps in order and keeps a trace of each.</summary>
public sealed class LenientPayslipRollupEngine(List<ILenientPayslipRollupFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<ILenientPayslipRollupFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static LenientPayslipRollupEngine DefaultLenientPayslipRollupEngine()
    {
        return new LenientPayslipRollupEngine(
            new List<ILenientPayslipRollupFactor>
            {
                new TieredLenientPayslipRollupAdjuster(
                    new List<double> { 2580.0, 6288.0, 6750.0, 8712.0 },
                    new List<double> { 0.84, 1.07, 1.18, 1.12 }),
                new PercentLenientPayslipRollupAdjuster(-15.0),
                new LenientPayslipRollupLeaveDaysOffsetAdjuster(1.63),
                new RoundingLenientPayslipRollupAdjuster(0.1),
                new TitleLenientPayslipRollupAdjuster(
                    new Dictionary<string, double> { ["north"] = 0.9, ["south"] = 0.7 }),
            },
            new List<string>(),
            2.1,
            88094.0);
    }

    public double Price(LenientPayslipRollup item)
    {
        var current = item.AdjustedExposure();
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

    public Dictionary<string, double> EvaluateAll(List<LenientPayslipRollup> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
