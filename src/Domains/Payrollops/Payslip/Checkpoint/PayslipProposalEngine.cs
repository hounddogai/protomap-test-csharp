namespace Acme.Workforce.Payrollops.Payslip.Checkpoint;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the payslip proposal calculation.</summary>
public interface IPayslipProposalStep
{
    string Describe();
    double Adjust(PayslipProposal item, double valueValue);
}

public sealed record PayslipProposalBudgetOffsetAdjuster(double Weight) : IPayslipProposalStep
{
    public string Describe()
    {
        return $"plus budget times {Weight}";
    }

    public double Adjust(PayslipProposal item, double valueValue)
    {
        return valueValue + item.Budget * Weight;
    }
}

public sealed record MinimumPayslipProposalAdjuster(double Minimum) : IPayslipProposalStep
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(PayslipProposal item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record PercentPayslipProposalAdjuster(double Percent) : IPayslipProposalStep
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(PayslipProposal item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record TieredPayslipProposalAdjuster(List<double> Bounds, List<double> Rates) : IPayslipProposalStep
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(PayslipProposal item, double valueValue)
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

public sealed record RoundingPayslipProposalAdjuster(double Step) : IPayslipProposalStep
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(PayslipProposal item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record TitlePayslipProposalAdjuster(Dictionary<string, double> Factors) : IPayslipProposalStep
{
    public string Describe()
    {
        return "by title";
    }

    public double Adjust(PayslipProposal item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

/// <summary>Runs the payslip proposal steps in order and keeps a trace of each.</summary>
public sealed class PayslipProposalEngine(List<IPayslipProposalStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IPayslipProposalStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static PayslipProposalEngine DefaultPayslipProposalEngine()
    {
        return new PayslipProposalEngine(
            new List<IPayslipProposalStep>
            {
                new PayslipProposalBudgetOffsetAdjuster(0.18),
                new MinimumPayslipProposalAdjuster(43.1),
                new PercentPayslipProposalAdjuster(21.9),
                new TieredPayslipProposalAdjuster(
                    new List<double> { 7319.0, 8702.0 },
                    new List<double> { 1.14, 0.84 }),
                new RoundingPayslipProposalAdjuster(0.05),
                new TitlePayslipProposalAdjuster(
                    new Dictionary<string, double> { ["central"] = 0.86, ["south"] = 0.77, ["east"] = 1.03 }),
            },
            new List<string>(),
            4.2,
            83886.1);
    }

    public double Run(PayslipProposal item)
    {
        var current = item.NetLoad();
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

    public Dictionary<string, double> EvaluateAll(List<PayslipProposal> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
