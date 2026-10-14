namespace Acme.Workforce.Payrollops.Payslip.Allocation;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the payslip forecast calculation.</summary>
public interface IPayslipForecastModifier
{
    string Describe();
    double Adjust(PayslipForecast item, double valueValue);
}

public sealed record TieredPayslipForecastAdjuster(List<double> Bounds, List<double> Rates) : IPayslipForecastModifier
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(PayslipForecast item, double valueValue)
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

public sealed record PercentPayslipForecastAdjuster(double Percent) : IPayslipForecastModifier
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(PayslipForecast item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record MinimumPayslipForecastAdjuster(double Minimum) : IPayslipForecastModifier
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(PayslipForecast item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record RoundingPayslipForecastAdjuster(double Step) : IPayslipForecastModifier
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(PayslipForecast item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record LabelPayslipForecastAdjuster(Dictionary<string, double> Factors) : IPayslipForecastModifier
{
    public string Describe()
    {
        return "by label";
    }

    public double Adjust(PayslipForecast item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

/// <summary>Runs the payslip forecast steps in order and keeps a trace of each.</summary>
public sealed class PayslipForecastEngine(List<IPayslipForecastModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IPayslipForecastModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static PayslipForecastEngine DefaultPayslipForecastEngine()
    {
        return new PayslipForecastEngine(
            new List<IPayslipForecastModifier>
            {
                new TieredPayslipForecastAdjuster(
                    new List<double> { 1717.0, 3691.0, 7984.0, 9466.0 },
                    new List<double> { 1.0, 1.06, 1.01, 1.16 }),
                new PercentPayslipForecastAdjuster(-11.3),
                new MinimumPayslipForecastAdjuster(5.4),
                new RoundingPayslipForecastAdjuster(5.0),
                new LabelPayslipForecastAdjuster(
                    new Dictionary<string, double> { ["north"] = 1.29, ["online"] = 0.73 }),
            },
            new List<string>(),
            4.0,
            53969.4);
    }

    public double Run(PayslipForecast item)
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

    public Dictionary<string, double> EvaluateAll(List<PayslipForecast> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
