namespace Acme.Workforce.Compensation.BonusPool.Rollup;

/// <summary>One step of the manual bonus pool draft calculation.</summary>
public interface IManualBonusPoolDraftFactor
{
    string Describe();
    double Adjust(ManualBonusPoolDraft item, double valueValue);
}

public sealed record ChannelManualBonusPoolDraftAdjuster(Dictionary<string, double> Factors) : IManualBonusPoolDraftFactor
{
    public string Describe()
    {
        return "by channel";
    }

    public double Adjust(ManualBonusPoolDraft item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record ManualBonusPoolDraftBudgetOffsetAdjuster(double Weight) : IManualBonusPoolDraftFactor
{
    public string Describe()
    {
        return $"plus budget times {Weight}";
    }

    public double Adjust(ManualBonusPoolDraft item, double valueValue)
    {
        return valueValue + item.Budget * Weight;
    }
}

public sealed record TieredManualBonusPoolDraftAdjuster(List<double> Bounds, List<double> Rates) : IManualBonusPoolDraftFactor
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(ManualBonusPoolDraft item, double valueValue)
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

public sealed record PercentManualBonusPoolDraftAdjuster(double Percent) : IManualBonusPoolDraftFactor
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(ManualBonusPoolDraft item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the manual bonus pool draft steps in order and keeps a trace of each.</summary>
public sealed class ManualBonusPoolDraftEngine(List<IManualBonusPoolDraftFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IManualBonusPoolDraftFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static ManualBonusPoolDraftEngine DefaultManualBonusPoolDraftEngine()
    {
        return new ManualBonusPoolDraftEngine(
            new List<IManualBonusPoolDraftFactor>
            {
                new ChannelManualBonusPoolDraftAdjuster(new Dictionary<string, double> { ["north"] = 1.28 }),
                new ManualBonusPoolDraftBudgetOffsetAdjuster(1.23),
                new TieredManualBonusPoolDraftAdjuster(
                    new List<double> { 3938.0, 8151.0, 8320.0 },
                    new List<double> { 0.88, 1.01, 0.92 }),
                new PercentManualBonusPoolDraftAdjuster(21.1),
            },
            new List<string>(),
            4.1,
            51712.2);
    }

    public double Price(ManualBonusPoolDraft item)
    {
        var current = item.WeightedLoad();
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

    public Dictionary<string, double> EvaluateAll(List<ManualBonusPoolDraft> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
