namespace Acme.Workforce.Payrollops.RetroPay.Review;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the retro pay agreement calculation.</summary>
public interface IRetroPayAgreementModifier
{
    string Describe();
    double Adjust(RetroPayAgreement item, double valueValue);
}

public sealed record CapRetroPayAgreementAdjuster(double Cap) : IRetroPayAgreementModifier
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(RetroPayAgreement item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record TieredRetroPayAgreementAdjuster(List<double> Bounds, List<double> Rates) : IRetroPayAgreementModifier
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(RetroPayAgreement item, double valueValue)
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

public sealed record MinimumRetroPayAgreementAdjuster(double Minimum) : IRetroPayAgreementModifier
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(RetroPayAgreement item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

/// <summary>Runs the retro pay agreement steps in order and keeps a trace of each.</summary>
public sealed class RetroPayAgreementEngine(List<IRetroPayAgreementModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IRetroPayAgreementModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static RetroPayAgreementEngine DefaultRetroPayAgreementEngine()
    {
        return new RetroPayAgreementEngine(
            new List<IRetroPayAgreementModifier>
            {
                new CapRetroPayAgreementAdjuster(16806.0),
                new TieredRetroPayAgreementAdjuster(
                    new List<double> { 2060.0, 3872.0 },
                    new List<double> { 0.94, 0.96 }),
                new MinimumRetroPayAgreementAdjuster(25.5),
            },
            new List<string>(),
            4.4,
            76572.1);
    }

    public double Price(RetroPayAgreement item)
    {
        var current = item.WeightedTotal();
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

    public Dictionary<string, double> EvaluateAll(List<RetroPayAgreement> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
