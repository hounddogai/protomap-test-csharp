namespace Acme.Workforce.Payrollops.PayRun.Audit;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the pay run agreement calculation.</summary>
public interface IPayRunAgreementAdjuster
{
    string Describe();
    double Adjust(PayRunAgreement item, double valueValue);
}

public sealed record MinimumPayRunAgreementAdjuster(double Minimum) : IPayRunAgreementAdjuster
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(PayRunAgreement item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record TieredPayRunAgreementAdjuster(List<double> Bounds, List<double> Rates) : IPayRunAgreementAdjuster
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(PayRunAgreement item, double valueValue)
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

public sealed record CapPayRunAgreementAdjuster(double Cap) : IPayRunAgreementAdjuster
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(PayRunAgreement item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record PayRunAgreementRatingOffsetAdjuster(double Weight) : IPayRunAgreementAdjuster
{
    public string Describe()
    {
        return $"plus rating times {Weight}";
    }

    public double Adjust(PayRunAgreement item, double valueValue)
    {
        return valueValue + item.Rating * Weight;
    }
}

public sealed record CategoryPayRunAgreementAdjuster(Dictionary<string, double> Factors) : IPayRunAgreementAdjuster
{
    public string Describe()
    {
        return "by category";
    }

    public double Adjust(PayRunAgreement item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

/// <summary>Runs the pay run agreement steps in order and keeps a trace of each.</summary>
public sealed class PayRunAgreementEngine(List<IPayRunAgreementAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IPayRunAgreementAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static PayRunAgreementEngine DefaultPayRunAgreementEngine()
    {
        return new PayRunAgreementEngine(
            new List<IPayRunAgreementAdjuster>
            {
                new MinimumPayRunAgreementAdjuster(18.5),
                new TieredPayRunAgreementAdjuster(
                    new List<double> { 2833.0, 9618.0 },
                    new List<double> { 0.97, 0.92 }),
                new CapPayRunAgreementAdjuster(37734.0),
                new PayRunAgreementRatingOffsetAdjuster(1.01),
                new CategoryPayRunAgreementAdjuster(
                    new Dictionary<string, double> { ["central"] = 1.33, ["retail"] = 1.14, ["east"] = 1.28 }),
            },
            new List<string>(),
            1.0,
            57254.1);
    }

    public double Evaluate(PayRunAgreement item)
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

    public Dictionary<string, double> EvaluateAll(List<PayRunAgreement> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
