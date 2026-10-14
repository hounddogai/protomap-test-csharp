namespace Acme.Workforce.Payrollops.Payslip.RollingPayslipSnapshot;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the regional payslip exception calculation.</summary>
public interface IRegionalPayslipExceptionStep
{
    string Describe();
    double Adjust(RegionalPayslipException item, double valueValue);
}

public sealed record CapRegionalPayslipExceptionAdjuster(double Cap) : IRegionalPayslipExceptionStep
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(RegionalPayslipException item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record RegionalPayslipExceptionExpiresAtOffsetAdjuster(double Weight) : IRegionalPayslipExceptionStep
{
    public string Describe()
    {
        return $"plus expires at times {Weight}";
    }

    public double Adjust(RegionalPayslipException item, double valueValue)
    {
        return valueValue + (double)item.ExpiresAt * Weight;
    }
}

public sealed record MinimumRegionalPayslipExceptionAdjuster(double Minimum) : IRegionalPayslipExceptionStep
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(RegionalPayslipException item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record RoundingRegionalPayslipExceptionAdjuster(double Step) : IRegionalPayslipExceptionStep
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(RegionalPayslipException item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record TieredRegionalPayslipExceptionAdjuster(List<double> Bounds, List<double> Rates) : IRegionalPayslipExceptionStep
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(RegionalPayslipException item, double valueValue)
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

public sealed record PercentRegionalPayslipExceptionAdjuster(double Percent) : IRegionalPayslipExceptionStep
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(RegionalPayslipException item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the regional payslip exception steps in order and keeps a trace of each.</summary>
public sealed class RegionalPayslipExceptionEngine(List<IRegionalPayslipExceptionStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IRegionalPayslipExceptionStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static RegionalPayslipExceptionEngine DefaultRegionalPayslipExceptionEngine()
    {
        return new RegionalPayslipExceptionEngine(
            new List<IRegionalPayslipExceptionStep>
            {
                new CapRegionalPayslipExceptionAdjuster(26679.0),
                new RegionalPayslipExceptionExpiresAtOffsetAdjuster(1.26),
                new MinimumRegionalPayslipExceptionAdjuster(18.9),
                new RoundingRegionalPayslipExceptionAdjuster(0.05),
                new TieredRegionalPayslipExceptionAdjuster(
                    new List<double> { 2267.0, 4140.0 },
                    new List<double> { 0.93, 0.9 }),
                new PercentRegionalPayslipExceptionAdjuster(15.3),
            },
            new List<string>(),
            1.3,
            66556.8);
    }

    public double Price(RegionalPayslipException item)
    {
        var current = item.EffectiveExposure();
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

    public Dictionary<string, double> EvaluateAll(List<RegionalPayslipException> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
