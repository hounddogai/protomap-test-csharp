namespace Acme.Workforce.Payrollops.Payslip.RollingPayslipSnapshot;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the primary payslip snapshot calculation.</summary>
public interface IPrimaryPayslipSnapshotModifier
{
    string Describe();
    double Adjust(PrimaryPayslipSnapshot item, double valueValue);
}

public sealed record MinimumPrimaryPayslipSnapshotAdjuster(double Minimum) : IPrimaryPayslipSnapshotModifier
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(PrimaryPayslipSnapshot item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record RoundingPrimaryPayslipSnapshotAdjuster(double Step) : IPrimaryPayslipSnapshotModifier
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(PrimaryPayslipSnapshot item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record CapPrimaryPayslipSnapshotAdjuster(double Cap) : IPrimaryPayslipSnapshotModifier
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(PrimaryPayslipSnapshot item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record FlaggedPrimaryPayslipSnapshotAdjuster(Registry Flags, string Flag, double Percent) : IPrimaryPayslipSnapshotModifier
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(PrimaryPayslipSnapshot item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record PercentPrimaryPayslipSnapshotAdjuster(double Percent) : IPrimaryPayslipSnapshotModifier
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(PrimaryPayslipSnapshot item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the primary payslip snapshot steps in order and keeps a trace of each.</summary>
public sealed class PrimaryPayslipSnapshotEngine(List<IPrimaryPayslipSnapshotModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IPrimaryPayslipSnapshotModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static PrimaryPayslipSnapshotEngine DefaultPrimaryPayslipSnapshotEngine(Registry flags)
    {
        return new PrimaryPayslipSnapshotEngine(
            new List<IPrimaryPayslipSnapshotModifier>
            {
                new MinimumPrimaryPayslipSnapshotAdjuster(20.1),
                new RoundingPrimaryPayslipSnapshotAdjuster(0.1),
                new CapPrimaryPayslipSnapshotAdjuster(3320.0),
                new FlaggedPrimaryPayslipSnapshotAdjuster(flags, "primary-payslip-snapshot-rolling", 4.4),
                new PercentPrimaryPayslipSnapshotAdjuster(-4.3),
            },
            new List<string>(),
            3.0,
            29578.8);
    }

    public double Run(PrimaryPayslipSnapshot item)
    {
        var current = item.NetTotal();
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

    public Dictionary<string, double> EvaluateAll(List<PrimaryPayslipSnapshot> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
