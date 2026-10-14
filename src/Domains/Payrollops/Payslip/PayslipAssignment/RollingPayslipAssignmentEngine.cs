namespace Acme.Workforce.Payrollops.Payslip.PayslipAssignment;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the rolling payslip assignment calculation.</summary>
public interface IRollingPayslipAssignmentAdjuster
{
    string Describe();
    double Adjust(RollingPayslipAssignment item, double valueValue);
}

public sealed record FlaggedRollingPayslipAssignmentAdjuster(Registry Flags, string Flag, double Percent) : IRollingPayslipAssignmentAdjuster
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(RollingPayslipAssignment item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record CapRollingPayslipAssignmentAdjuster(double Cap) : IRollingPayslipAssignmentAdjuster
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(RollingPayslipAssignment item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record RollingPayslipAssignmentCapacityOffsetAdjuster(double Weight) : IRollingPayslipAssignmentAdjuster
{
    public string Describe()
    {
        return $"plus capacity times {Weight}";
    }

    public double Adjust(RollingPayslipAssignment item, double valueValue)
    {
        return valueValue + (double)item.Capacity * Weight;
    }
}

public sealed record PercentRollingPayslipAssignmentAdjuster(double Percent) : IRollingPayslipAssignmentAdjuster
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(RollingPayslipAssignment item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record TeamRollingPayslipAssignmentAdjuster(Dictionary<string, double> Factors) : IRollingPayslipAssignmentAdjuster
{
    public string Describe()
    {
        return "by team";
    }

    public double Adjust(RollingPayslipAssignment item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record MinimumRollingPayslipAssignmentAdjuster(double Minimum) : IRollingPayslipAssignmentAdjuster
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(RollingPayslipAssignment item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

/// <summary>Runs the rolling payslip assignment steps in order and keeps a trace of each.</summary>
public sealed class RollingPayslipAssignmentEngine(List<IRollingPayslipAssignmentAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IRollingPayslipAssignmentAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static RollingPayslipAssignmentEngine DefaultRollingPayslipAssignmentEngine(Registry flags)
    {
        return new RollingPayslipAssignmentEngine(
            new List<IRollingPayslipAssignmentAdjuster>
            {
                new FlaggedRollingPayslipAssignmentAdjuster(flags, "rolling-payslip-assignment-primary", -2.8),
                new CapRollingPayslipAssignmentAdjuster(42422.0),
                new RollingPayslipAssignmentCapacityOffsetAdjuster(1.27),
                new PercentRollingPayslipAssignmentAdjuster(-4.2),
                new TeamRollingPayslipAssignmentAdjuster(
                    new Dictionary<string, double> { ["west"] = 1.05, ["north"] = 1.37 }),
                new MinimumRollingPayslipAssignmentAdjuster(43.4),
            },
            new List<string>(),
            1.7,
            32721.4);
    }

    public double Evaluate(RollingPayslipAssignment item)
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

    public Dictionary<string, double> EvaluateAll(List<RollingPayslipAssignment> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
