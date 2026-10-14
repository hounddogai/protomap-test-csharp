namespace Acme.Workforce.Compensation.PayBand.Tier;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the pay band tier calculation.</summary>
public interface IPayBandTierModifier
{
    string Describe();
    double Adjust(PayBandTier item, double valueValue);
}

public sealed record MinimumPayBandTierAdjuster(double Minimum) : IPayBandTierModifier
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(PayBandTier item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record PercentPayBandTierAdjuster(double Percent) : IPayBandTierModifier
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(PayBandTier item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record TieredPayBandTierAdjuster(List<double> Bounds, List<double> Rates) : IPayBandTierModifier
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(PayBandTier item, double valueValue)
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

public sealed record RoundingPayBandTierAdjuster(double Step) : IPayBandTierModifier
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(PayBandTier item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

/// <summary>Runs the pay band tier steps in order and keeps a trace of each.</summary>
public sealed class PayBandTierEngine(List<IPayBandTierModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IPayBandTierModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static PayBandTierEngine DefaultPayBandTierEngine()
    {
        return new PayBandTierEngine(
            new List<IPayBandTierModifier>
            {
                new MinimumPayBandTierAdjuster(19.7),
                new PercentPayBandTierAdjuster(8.1),
                new TieredPayBandTierAdjuster(
                    new List<double> { 7031.0, 9019.0, 9402.0 },
                    new List<double> { 1.17, 1.19, 0.97 }),
                new RoundingPayBandTierAdjuster(5.0),
            },
            new List<string>(),
            0.2,
            73670.2);
    }

    public double Evaluate(PayBandTier item)
    {
        var current = item.ProjectedLoad();
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

    public Dictionary<string, double> EvaluateAll(List<PayBandTier> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
