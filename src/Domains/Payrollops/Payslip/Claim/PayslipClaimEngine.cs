namespace Acme.Workforce.Payrollops.Payslip.Claim;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the payslip claim calculation.</summary>
public interface IPayslipClaimStep
{
    string Describe();
    double Adjust(PayslipClaim item, double valueValue);
}

public sealed record TieredPayslipClaimAdjuster(List<double> Bounds, List<double> Rates) : IPayslipClaimStep
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(PayslipClaim item, double valueValue)
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

public sealed record RoundingPayslipClaimAdjuster(double Step) : IPayslipClaimStep
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(PayslipClaim item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record PayslipClaimLimitOffsetAdjuster(double Weight) : IPayslipClaimStep
{
    public string Describe()
    {
        return $"plus limit times {Weight}";
    }

    public double Adjust(PayslipClaim item, double valueValue)
    {
        return valueValue + (double)item.Limit * Weight;
    }
}

/// <summary>Runs the payslip claim steps in order and keeps a trace of each.</summary>
public sealed class PayslipClaimEngine(List<IPayslipClaimStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IPayslipClaimStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static PayslipClaimEngine DefaultPayslipClaimEngine()
    {
        return new PayslipClaimEngine(
            new List<IPayslipClaimStep>
            {
                new TieredPayslipClaimAdjuster(
                    new List<double> { 1967.0, 3417.0, 8389.0, 9371.0 },
                    new List<double> { 0.88, 1.15, 1.11, 1.0 }),
                new RoundingPayslipClaimAdjuster(5.0),
                new PayslipClaimLimitOffsetAdjuster(1.49),
            },
            new List<string>(),
            4.4,
            17619.0);
    }

    public double Price(PayslipClaim item)
    {
        var current = item.WeightedTotal();
        Trace = new List<string>();
        for (var index = 0; index < Steps.Count; index++)
        {
            var step = Steps[index];
            var before = current;
            current = step.Adjust(item, current);
            if (before != current)
            {
                Trace.Add($"{index + 1}. {step.Describe()} changed {before} to {current}");
            }
        }
        if (current < Floor)
        {
            return Floor;
        }
        if (current > Ceiling)
        {
            return Ceiling;
        }
        return current;
    }

    public string Explain()
    {
        return string.Join("\n", Trace);
    }

    public Dictionary<string, double> EvaluateAll(List<PayslipClaim> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
