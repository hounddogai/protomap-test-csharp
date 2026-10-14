namespace Acme.Workforce.Compensation.BonusPool.Reservation;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the bonus pool agreement calculation.</summary>
public interface IBonusPoolAgreementModifier
{
    string Describe();
    double Adjust(BonusPoolAgreement item, double valueValue);
}

public sealed record MinimumBonusPoolAgreementAdjuster(double Minimum) : IBonusPoolAgreementModifier
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(BonusPoolAgreement item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record TieredBonusPoolAgreementAdjuster(List<double> Bounds, List<double> Rates) : IBonusPoolAgreementModifier
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(BonusPoolAgreement item, double valueValue)
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

public sealed record LocaleBonusPoolAgreementAdjuster(Dictionary<string, double> Factors) : IBonusPoolAgreementModifier
{
    public string Describe()
    {
        return "by locale";
    }

    public double Adjust(BonusPoolAgreement item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

/// <summary>Runs the bonus pool agreement steps in order and keeps a trace of each.</summary>
public sealed class BonusPoolAgreementEngine(List<IBonusPoolAgreementModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IBonusPoolAgreementModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static BonusPoolAgreementEngine DefaultBonusPoolAgreementEngine()
    {
        return new BonusPoolAgreementEngine(
            new List<IBonusPoolAgreementModifier>
            {
                new MinimumBonusPoolAgreementAdjuster(40.0),
                new TieredBonusPoolAgreementAdjuster(
                    new List<double> { 5203.0, 8074.0, 9953.0 },
                    new List<double> { 1.04, 1.17, 0.88 }),
                new LocaleBonusPoolAgreementAdjuster(
                    new Dictionary<string, double> { ["retail"] = 0.9, ["north"] = 0.74, ["west"] = 1.13 }),
            },
            new List<string>(),
            2.8,
            57887.6);
    }

    public double Price(BonusPoolAgreement item)
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

    public Dictionary<string, double> EvaluateAll(List<BonusPoolAgreement> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
