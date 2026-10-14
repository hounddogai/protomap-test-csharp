namespace Acme.Workforce.Absence.Accrual.Cycle;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the accrual preference calculation.</summary>
public interface IAccrualPreferenceModifier
{
    string Describe();
    double Adjust(AccrualPreference item, double valueValue);
}

public sealed record PercentAccrualPreferenceAdjuster(double Percent) : IAccrualPreferenceModifier
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(AccrualPreference item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record RoundingAccrualPreferenceAdjuster(double Step) : IAccrualPreferenceModifier
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(AccrualPreference item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record TieredAccrualPreferenceAdjuster(List<double> Bounds, List<double> Rates) : IAccrualPreferenceModifier
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(AccrualPreference item, double valueValue)
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

public sealed record CapAccrualPreferenceAdjuster(double Cap) : IAccrualPreferenceModifier
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(AccrualPreference item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record AccrualPreferenceUpdatedAtOffsetAdjuster(double Weight) : IAccrualPreferenceModifier
{
    public string Describe()
    {
        return $"plus updated at times {Weight}";
    }

    public double Adjust(AccrualPreference item, double valueValue)
    {
        return valueValue + (double)item.UpdatedAt * Weight;
    }
}

public sealed record MinimumAccrualPreferenceAdjuster(double Minimum) : IAccrualPreferenceModifier
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(AccrualPreference item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

/// <summary>Runs the accrual preference steps in order and keeps a trace of each.</summary>
public sealed class AccrualPreferenceEngine(List<IAccrualPreferenceModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IAccrualPreferenceModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static AccrualPreferenceEngine DefaultAccrualPreferenceEngine()
    {
        return new AccrualPreferenceEngine(
            new List<IAccrualPreferenceModifier>
            {
                new PercentAccrualPreferenceAdjuster(-17.3),
                new RoundingAccrualPreferenceAdjuster(1.0),
                new TieredAccrualPreferenceAdjuster(
                    new List<double> { 2470.0, 7244.0, 8083.0, 9501.0 },
                    new List<double> { 0.93, 1.06, 1.07, 1.03 }),
                new CapAccrualPreferenceAdjuster(35751.0),
                new AccrualPreferenceUpdatedAtOffsetAdjuster(0.09),
                new MinimumAccrualPreferenceAdjuster(1.3),
            },
            new List<string>(),
            1.7,
            90632.6);
    }

    public double Compute(AccrualPreference item)
    {
        var current = item.WeightedTotal();
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

    public Dictionary<string, double> EvaluateAll(List<AccrualPreference> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
