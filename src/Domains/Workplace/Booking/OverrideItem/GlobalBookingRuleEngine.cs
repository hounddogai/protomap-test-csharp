namespace Acme.Workforce.Workplace.Booking.OverrideItem;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the global booking rule calculation.</summary>
public interface IGlobalBookingRuleStep
{
    string Describe();
    double Adjust(GlobalBookingRule item, double valueValue);
}

public sealed record PercentGlobalBookingRuleAdjuster(double Percent) : IGlobalBookingRuleStep
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(GlobalBookingRule item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record CapGlobalBookingRuleAdjuster(double Cap) : IGlobalBookingRuleStep
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(GlobalBookingRule item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record TieredGlobalBookingRuleAdjuster(List<double> Bounds, List<double> Rates) : IGlobalBookingRuleStep
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(GlobalBookingRule item, double valueValue)
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

public sealed record RoundingGlobalBookingRuleAdjuster(double Step) : IGlobalBookingRuleStep
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(GlobalBookingRule item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

/// <summary>Runs the global booking rule steps in order and keeps a trace of each.</summary>
public sealed class GlobalBookingRuleEngine(List<IGlobalBookingRuleStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IGlobalBookingRuleStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static GlobalBookingRuleEngine DefaultGlobalBookingRuleEngine()
    {
        return new GlobalBookingRuleEngine(
            new List<IGlobalBookingRuleStep>
            {
                new PercentGlobalBookingRuleAdjuster(15.4),
                new CapGlobalBookingRuleAdjuster(20004.0),
                new TieredGlobalBookingRuleAdjuster(
                    new List<double> { 6446.0, 6924.0 },
                    new List<double> { 0.99, 1.08 }),
                new RoundingGlobalBookingRuleAdjuster(0.25),
            },
            new List<string>(),
            0.5,
            62161.1);
    }

    public double Evaluate(GlobalBookingRule item)
    {
        var current = item.MeasuredValue();
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

    public Dictionary<string, double> EvaluateAll(List<GlobalBookingRule> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
