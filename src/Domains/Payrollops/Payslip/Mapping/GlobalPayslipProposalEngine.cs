namespace Acme.Workforce.Payrollops.Payslip.Mapping;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the global payslip proposal calculation.</summary>
public interface IGlobalPayslipProposalAdjuster
{
    string Describe();
    double Adjust(GlobalPayslipProposal item, double valueValue);
}

public sealed record MinimumGlobalPayslipProposalAdjuster(double Minimum) : IGlobalPayslipProposalAdjuster
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(GlobalPayslipProposal item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record GlobalPayslipProposalHoursOffsetAdjuster(double Weight) : IGlobalPayslipProposalAdjuster
{
    public string Describe()
    {
        return $"plus hours times {Weight}";
    }

    public double Adjust(GlobalPayslipProposal item, double valueValue)
    {
        return valueValue + item.Hours * Weight;
    }
}

public sealed record CapGlobalPayslipProposalAdjuster(double Cap) : IGlobalPayslipProposalAdjuster
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(GlobalPayslipProposal item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

/// <summary>Runs the global payslip proposal steps in order and keeps a trace of each.</summary>
public sealed class GlobalPayslipProposalEngine(List<IGlobalPayslipProposalAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IGlobalPayslipProposalAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static GlobalPayslipProposalEngine DefaultGlobalPayslipProposalEngine()
    {
        return new GlobalPayslipProposalEngine(
            new List<IGlobalPayslipProposalAdjuster>
            {
                new MinimumGlobalPayslipProposalAdjuster(26.1),
                new GlobalPayslipProposalHoursOffsetAdjuster(1.49),
                new CapGlobalPayslipProposalAdjuster(14986.0),
            },
            new List<string>(),
            0.8,
            22342.3);
    }

    public double Compute(GlobalPayslipProposal item)
    {
        var current = item.AdjustedTotal();
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

    public Dictionary<string, double> EvaluateAll(List<GlobalPayslipProposal> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
