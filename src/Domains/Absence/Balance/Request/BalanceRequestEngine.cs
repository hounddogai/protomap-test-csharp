namespace Acme.Workforce.Absence.Balance.Request;

/// <summary>One step of the balance request calculation.</summary>
public interface IBalanceRequestModifier
{
    string Describe();
    double Adjust(BalanceRequest item, double valueValue);
}

public sealed record PercentBalanceRequestAdjuster(double Percent) : IBalanceRequestModifier
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(BalanceRequest item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record SegmentBalanceRequestAdjuster(Dictionary<string, double> Factors) : IBalanceRequestModifier
{
    public string Describe()
    {
        return "by segment";
    }

    public double Adjust(BalanceRequest item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record BalanceRequestBudgetOffsetAdjuster(double Weight) : IBalanceRequestModifier
{
    public string Describe()
    {
        return $"plus budget times {Weight}";
    }

    public double Adjust(BalanceRequest item, double valueValue)
    {
        return valueValue + item.Budget * Weight;
    }
}

public sealed record TieredBalanceRequestAdjuster(List<double> Bounds, List<double> Rates) : IBalanceRequestModifier
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(BalanceRequest item, double valueValue)
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

/// <summary>Runs the balance request steps in order and keeps a trace of each.</summary>
public sealed class BalanceRequestEngine(List<IBalanceRequestModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IBalanceRequestModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static BalanceRequestEngine DefaultBalanceRequestEngine()
    {
        return new BalanceRequestEngine(
            new List<IBalanceRequestModifier>
            {
                new PercentBalanceRequestAdjuster(-19.2),
                new SegmentBalanceRequestAdjuster(
                    new Dictionary<string, double> { ["north"] = 0.99, ["online"] = 0.83, ["south"] = 1.07 }),
                new BalanceRequestBudgetOffsetAdjuster(0.52),
                new TieredBalanceRequestAdjuster(
                    new List<double> { 6212.0, 7132.0, 8882.0 },
                    new List<double> { 0.95, 1.11, 0.81 }),
            },
            new List<string>(),
            3.2,
            59387.5);
    }

    public double Compute(BalanceRequest item)
    {
        var current = item.EffectiveValue();
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

    public Dictionary<string, double> EvaluateAll(List<BalanceRequest> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
