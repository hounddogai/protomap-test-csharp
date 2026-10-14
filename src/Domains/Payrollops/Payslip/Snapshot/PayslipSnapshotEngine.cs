namespace Acme.Workforce.Payrollops.Payslip.Snapshot;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the payslip snapshot calculation.</summary>
public interface IPayslipSnapshotFactor
{
    string Describe();
    double Adjust(PayslipSnapshot item, double valueValue);
}

public sealed record DepartmentPayslipSnapshotAdjuster(Dictionary<string, double> Factors) : IPayslipSnapshotFactor
{
    public string Describe()
    {
        return "by department";
    }

    public double Adjust(PayslipSnapshot item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record PercentPayslipSnapshotAdjuster(double Percent) : IPayslipSnapshotFactor
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(PayslipSnapshot item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record FlaggedPayslipSnapshotAdjuster(Registry Flags, string Flag, double Percent) : IPayslipSnapshotFactor
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(PayslipSnapshot item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record PayslipSnapshotFloorValueOffsetAdjuster(double Weight) : IPayslipSnapshotFactor
{
    public string Describe()
    {
        return $"plus floor value times {Weight}";
    }

    public double Adjust(PayslipSnapshot item, double valueValue)
    {
        return valueValue + item.FloorValue * Weight;
    }
}

public sealed record TieredPayslipSnapshotAdjuster(List<double> Bounds, List<double> Rates) : IPayslipSnapshotFactor
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(PayslipSnapshot item, double valueValue)
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

public sealed record RoundingPayslipSnapshotAdjuster(double Step) : IPayslipSnapshotFactor
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(PayslipSnapshot item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

/// <summary>Runs the payslip snapshot steps in order and keeps a trace of each.</summary>
public sealed class PayslipSnapshotEngine(List<IPayslipSnapshotFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IPayslipSnapshotFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static PayslipSnapshotEngine DefaultPayslipSnapshotEngine(Registry flags)
    {
        return new PayslipSnapshotEngine(
            new List<IPayslipSnapshotFactor>
            {
                new DepartmentPayslipSnapshotAdjuster(new Dictionary<string, double> { ["central"] = 0.77 }),
                new PercentPayslipSnapshotAdjuster(-21.0),
                new FlaggedPayslipSnapshotAdjuster(flags, "payslip-snapshot-capped", -0.7),
                new PayslipSnapshotFloorValueOffsetAdjuster(0.34),
                new TieredPayslipSnapshotAdjuster(
                    new List<double> { 6666.0, 6847.0, 8554.0, 8954.0 },
                    new List<double> { 1.09, 1.17, 1.02, 0.87 }),
                new RoundingPayslipSnapshotAdjuster(0.25),
            },
            new List<string>(),
            3.7,
            63614.2);
    }

    public double Evaluate(PayslipSnapshot item)
    {
        var current = item.EffectiveVolume();
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

    public Dictionary<string, double> EvaluateAll(List<PayslipSnapshot> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
