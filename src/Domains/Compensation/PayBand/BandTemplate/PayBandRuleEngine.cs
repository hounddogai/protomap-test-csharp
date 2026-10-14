namespace Acme.Workforce.Compensation.PayBand.BandTemplate;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the pay band rule calculation.</summary>
public interface IPayBandRuleFactor
{
    string Describe();
    double Adjust(PayBandRule item, double valueValue);
}

public sealed record MinimumPayBandRuleAdjuster(double Minimum) : IPayBandRuleFactor
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(PayBandRule item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record RoundingPayBandRuleAdjuster(double Step) : IPayBandRuleFactor
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(PayBandRule item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record CapPayBandRuleAdjuster(double Cap) : IPayBandRuleFactor
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(PayBandRule item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record PercentPayBandRuleAdjuster(double Percent) : IPayBandRuleFactor
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(PayBandRule item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record TieredPayBandRuleAdjuster(List<double> Bounds, List<double> Rates) : IPayBandRuleFactor
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(PayBandRule item, double valueValue)
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

public sealed record SourcePayBandRuleAdjuster(Dictionary<string, double> Factors) : IPayBandRuleFactor
{
    public string Describe()
    {
        return "by source";
    }

    public double Adjust(PayBandRule item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

/// <summary>Runs the pay band rule steps in order and keeps a trace of each.</summary>
public sealed class PayBandRuleEngine(List<IPayBandRuleFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IPayBandRuleFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static PayBandRuleEngine DefaultPayBandRuleEngine()
    {
        return new PayBandRuleEngine(
            new List<IPayBandRuleFactor>
            {
                new MinimumPayBandRuleAdjuster(21.2),
                new RoundingPayBandRuleAdjuster(5.0),
                new CapPayBandRuleAdjuster(7507.0),
                new PercentPayBandRuleAdjuster(18.8),
                new TieredPayBandRuleAdjuster(new List<double> { 2109.0, 4966.0 }, new List<double> { 1.15, 1.16 }),
                new SourcePayBandRuleAdjuster(
                    new Dictionary<string, double> { ["online"] = 0.95, ["retail"] = 0.92 }),
            },
            new List<string>(),
            1.9,
            28752.5);
    }

    public double Evaluate(PayBandRule item)
    {
        var current = item.AdjustedLoad();
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

    public Dictionary<string, double> EvaluateAll(List<PayBandRule> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
