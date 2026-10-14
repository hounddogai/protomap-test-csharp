namespace Acme.Workforce.Compensation.PayBand.Draft;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the pay band draft calculation.</summary>
public interface IPayBandDraftModifier
{
    string Describe();
    double Adjust(PayBandDraft item, double valueValue);
}

public sealed record PayBandDraftWeightOffsetAdjuster(double Weight) : IPayBandDraftModifier
{
    public string Describe()
    {
        return $"plus weight times {Weight}";
    }

    public double Adjust(PayBandDraft item, double valueValue)
    {
        return valueValue + item.Weight * Weight;
    }
}

public sealed record MinimumPayBandDraftAdjuster(double Minimum) : IPayBandDraftModifier
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(PayBandDraft item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record PercentPayBandDraftAdjuster(double Percent) : IPayBandDraftModifier
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(PayBandDraft item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record CountryPayBandDraftAdjuster(Dictionary<string, double> Factors) : IPayBandDraftModifier
{
    public string Describe()
    {
        return "by country";
    }

    public double Adjust(PayBandDraft item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

/// <summary>Runs the pay band draft steps in order and keeps a trace of each.</summary>
public sealed class PayBandDraftEngine(List<IPayBandDraftModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IPayBandDraftModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static PayBandDraftEngine DefaultPayBandDraftEngine()
    {
        return new PayBandDraftEngine(
            new List<IPayBandDraftModifier>
            {
                new PayBandDraftWeightOffsetAdjuster(1.1),
                new MinimumPayBandDraftAdjuster(36.1),
                new PercentPayBandDraftAdjuster(-8.9),
                new CountryPayBandDraftAdjuster(new Dictionary<string, double> { ["west"] = 0.84 }),
            },
            new List<string>(),
            3.4,
            43539.2);
    }

    public double Run(PayBandDraft item)
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

    public Dictionary<string, double> EvaluateAll(List<PayBandDraft> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
