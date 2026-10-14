namespace Acme.Workforce.Payrollops.Payslip.Budget;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the payslip record calculation.</summary>
public interface IPayslipRecordFactor
{
    string Describe();
    double Adjust(PayslipRecord item, double valueValue);
}

public sealed record PercentPayslipRecordAdjuster(double Percent) : IPayslipRecordFactor
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(PayslipRecord item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record PayslipRecordCreatedAtOffsetAdjuster(double Weight) : IPayslipRecordFactor
{
    public string Describe()
    {
        return $"plus created at times {Weight}";
    }

    public double Adjust(PayslipRecord item, double valueValue)
    {
        return valueValue + (double)item.CreatedAt * Weight;
    }
}

public sealed record TieredPayslipRecordAdjuster(List<double> Bounds, List<double> Rates) : IPayslipRecordFactor
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(PayslipRecord item, double valueValue)
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

public sealed record CapPayslipRecordAdjuster(double Cap) : IPayslipRecordFactor
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(PayslipRecord item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record FlaggedPayslipRecordAdjuster(Registry Flags, string Flag, double Percent) : IPayslipRecordFactor
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(PayslipRecord item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the payslip record steps in order and keeps a trace of each.</summary>
public sealed class PayslipRecordEngine(List<IPayslipRecordFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IPayslipRecordFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static PayslipRecordEngine DefaultPayslipRecordEngine(Registry flags)
    {
        return new PayslipRecordEngine(
            new List<IPayslipRecordFactor>
            {
                new PercentPayslipRecordAdjuster(-7.0),
                new PayslipRecordCreatedAtOffsetAdjuster(1.77),
                new TieredPayslipRecordAdjuster(
                    new List<double> { 3017.0, 6116.0, 9324.0 },
                    new List<double> { 1.14, 0.99, 0.91 }),
                new CapPayslipRecordAdjuster(13011.0),
                new FlaggedPayslipRecordAdjuster(flags, "payslip-record-strict", 2.1),
            },
            new List<string>(),
            1.9,
            15013.6);
    }

    public double Compute(PayslipRecord item)
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

    public Dictionary<string, double> EvaluateAll(List<PayslipRecord> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
