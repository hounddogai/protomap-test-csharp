namespace Acme.Workforce.Absence.ReturnPlan.Agreement;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the return plan agreement calculation.</summary>
public interface IReturnPlanAgreementModifier
{
    string Describe();
    double Adjust(ReturnPlanAgreement item, double valueValue);
}

public sealed record CapReturnPlanAgreementAdjuster(double Cap) : IReturnPlanAgreementModifier
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(ReturnPlanAgreement item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record TieredReturnPlanAgreementAdjuster(List<double> Bounds, List<double> Rates) : IReturnPlanAgreementModifier
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(ReturnPlanAgreement item, double valueValue)
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

public sealed record PercentReturnPlanAgreementAdjuster(double Percent) : IReturnPlanAgreementModifier
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(ReturnPlanAgreement item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the return plan agreement steps in order and keeps a trace of each.</summary>
public sealed class ReturnPlanAgreementEngine(List<IReturnPlanAgreementModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IReturnPlanAgreementModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static ReturnPlanAgreementEngine DefaultReturnPlanAgreementEngine()
    {
        return new ReturnPlanAgreementEngine(
            new List<IReturnPlanAgreementModifier>
            {
                new CapReturnPlanAgreementAdjuster(29771.0),
                new TieredReturnPlanAgreementAdjuster(
                    new List<double> { 593.0, 3256.0, 8630.0, 9590.0 },
                    new List<double> { 0.9, 0.83, 0.97, 0.86 }),
                new PercentReturnPlanAgreementAdjuster(-6.4),
            },
            new List<string>(),
            1.8,
            7504.8);
    }

    public double Price(ReturnPlanAgreement item)
    {
        var current = item.NetVolume();
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

    public Dictionary<string, double> EvaluateAll(List<ReturnPlanAgreement> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
