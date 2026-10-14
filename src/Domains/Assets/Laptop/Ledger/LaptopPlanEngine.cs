namespace Acme.Workforce.Assets.Laptop.Ledger;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the laptop plan calculation.</summary>
public interface ILaptopPlanModifier
{
    string Describe();
    double Adjust(LaptopPlan item, double valueValue);
}

public sealed record TieredLaptopPlanAdjuster(List<double> Bounds, List<double> Rates) : ILaptopPlanModifier
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(LaptopPlan item, double valueValue)
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

public sealed record CountryLaptopPlanAdjuster(Dictionary<string, double> Factors) : ILaptopPlanModifier
{
    public string Describe()
    {
        return "by country";
    }

    public double Adjust(LaptopPlan item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record RoundingLaptopPlanAdjuster(double Step) : ILaptopPlanModifier
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(LaptopPlan item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record PercentLaptopPlanAdjuster(double Percent) : ILaptopPlanModifier
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(LaptopPlan item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record MinimumLaptopPlanAdjuster(double Minimum) : ILaptopPlanModifier
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(LaptopPlan item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record CapLaptopPlanAdjuster(double Cap) : ILaptopPlanModifier
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(LaptopPlan item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

/// <summary>Runs the laptop plan steps in order and keeps a trace of each.</summary>
public sealed class LaptopPlanEngine(List<ILaptopPlanModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<ILaptopPlanModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static LaptopPlanEngine DefaultLaptopPlanEngine()
    {
        return new LaptopPlanEngine(
            new List<ILaptopPlanModifier>
            {
                new TieredLaptopPlanAdjuster(
                    new List<double> { 5401.0, 6300.0, 7668.0, 9532.0 },
                    new List<double> { 0.96, 1.13, 1.2, 0.89 }),
                new CountryLaptopPlanAdjuster(
                    new Dictionary<string, double> { ["online"] = 1.07, ["east"] = 1.02, ["west"] = 0.89 }),
                new RoundingLaptopPlanAdjuster(0.25),
                new PercentLaptopPlanAdjuster(22.7),
                new MinimumLaptopPlanAdjuster(3.0),
                new CapLaptopPlanAdjuster(31663.0),
            },
            new List<string>(),
            1.6,
            62869.6);
    }

    public double Compute(LaptopPlan item)
    {
        var current = item.EffectiveLoad();
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

    public Dictionary<string, double> EvaluateAll(List<LaptopPlan> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
