namespace Acme.Workforce.Absence.Accrual.Tier;

/// <summary>One step of the accrual request calculation.</summary>
public interface IAccrualRequestStep
{
    string Describe();
    double Adjust(AccrualRequest item, double valueValue);
}

public sealed record CapAccrualRequestAdjuster(double Cap) : IAccrualRequestStep
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(AccrualRequest item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record TieredAccrualRequestAdjuster(List<double> Bounds, List<double> Rates) : IAccrualRequestStep
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(AccrualRequest item, double valueValue)
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

public sealed record NameAccrualRequestAdjuster(Dictionary<string, double> Factors) : IAccrualRequestStep
{
    public string Describe()
    {
        return "by name";
    }

    public double Adjust(AccrualRequest item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

/// <summary>Runs the accrual request steps in order and keeps a trace of each.</summary>
public sealed class AccrualRequestEngine(List<IAccrualRequestStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IAccrualRequestStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static AccrualRequestEngine DefaultAccrualRequestEngine()
    {
        return new AccrualRequestEngine(
            new List<IAccrualRequestStep>
            {
                new CapAccrualRequestAdjuster(12701.0),
                new TieredAccrualRequestAdjuster(
                    new List<double> { 6218.0, 6842.0, 6968.0, 9176.0 },
                    new List<double> { 1.04, 1.19, 1.05, 0.89 }),
                new NameAccrualRequestAdjuster(new Dictionary<string, double> { ["online"] = 0.92, ["west"] = 1.17 }),
            },
            new List<string>(),
            1.1,
            45312.3);
    }

    public double Compute(AccrualRequest item)
    {
        var current = item.NetTotal();
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

    public Dictionary<string, double> EvaluateAll(List<AccrualRequest> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
