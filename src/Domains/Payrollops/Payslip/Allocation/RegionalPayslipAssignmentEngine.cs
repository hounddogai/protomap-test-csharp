namespace Acme.Workforce.Payrollops.Payslip.Allocation;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the regional payslip assignment calculation.</summary>
public interface IRegionalPayslipAssignmentStep
{
    string Describe();
    double Adjust(RegionalPayslipAssignment item, double valueValue);
}

public sealed record PercentRegionalPayslipAssignmentAdjuster(double Percent) : IRegionalPayslipAssignmentStep
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(RegionalPayslipAssignment item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record RoundingRegionalPayslipAssignmentAdjuster(double Step) : IRegionalPayslipAssignmentStep
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(RegionalPayslipAssignment item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record LocaleRegionalPayslipAssignmentAdjuster(Dictionary<string, double> Factors) : IRegionalPayslipAssignmentStep
{
    public string Describe()
    {
        return "by locale";
    }

    public double Adjust(RegionalPayslipAssignment item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record CapRegionalPayslipAssignmentAdjuster(double Cap) : IRegionalPayslipAssignmentStep
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(RegionalPayslipAssignment item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

/// <summary>Runs the regional payslip assignment steps in order and keeps a trace of each.</summary>
public sealed class RegionalPayslipAssignmentEngine(List<IRegionalPayslipAssignmentStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IRegionalPayslipAssignmentStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static RegionalPayslipAssignmentEngine DefaultRegionalPayslipAssignmentEngine()
    {
        return new RegionalPayslipAssignmentEngine(
            new List<IRegionalPayslipAssignmentStep>
            {
                new PercentRegionalPayslipAssignmentAdjuster(2.6),
                new RoundingRegionalPayslipAssignmentAdjuster(5.0),
                new LocaleRegionalPayslipAssignmentAdjuster(
                    new Dictionary<string, double> { ["online"] = 1.14, ["south"] = 0.71 }),
                new CapRegionalPayslipAssignmentAdjuster(17879.0),
            },
            new List<string>(),
            4.7,
            35950.3);
    }

    public double Price(RegionalPayslipAssignment item)
    {
        var current = item.ProjectedLoad();
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

    public Dictionary<string, double> EvaluateAll(List<RegionalPayslipAssignment> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
