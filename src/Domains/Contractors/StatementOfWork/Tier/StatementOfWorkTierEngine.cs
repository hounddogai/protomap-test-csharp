namespace Acme.Workforce.Contractors.StatementOfWork.Tier;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the statement of work tier calculation.</summary>
public interface IStatementOfWorkTierAdjuster
{
    string Describe();
    double Adjust(StatementOfWorkTier item, double valueValue);
}

public sealed record StatementOfWorkTierWindowDaysOffsetAdjuster(double Weight) : IStatementOfWorkTierAdjuster
{
    public string Describe()
    {
        return $"plus window days times {Weight}";
    }

    public double Adjust(StatementOfWorkTier item, double valueValue)
    {
        return valueValue + (double)item.WindowDays * Weight;
    }
}

public sealed record LabelStatementOfWorkTierAdjuster(Dictionary<string, double> Factors) : IStatementOfWorkTierAdjuster
{
    public string Describe()
    {
        return "by label";
    }

    public double Adjust(StatementOfWorkTier item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record TieredStatementOfWorkTierAdjuster(List<double> Bounds, List<double> Rates) : IStatementOfWorkTierAdjuster
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(StatementOfWorkTier item, double valueValue)
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

public sealed record PercentStatementOfWorkTierAdjuster(double Percent) : IStatementOfWorkTierAdjuster
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(StatementOfWorkTier item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record MinimumStatementOfWorkTierAdjuster(double Minimum) : IStatementOfWorkTierAdjuster
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(StatementOfWorkTier item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record RoundingStatementOfWorkTierAdjuster(double Step) : IStatementOfWorkTierAdjuster
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(StatementOfWorkTier item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

/// <summary>Runs the statement of work tier steps in order and keeps a trace of each.</summary>
public sealed class StatementOfWorkTierEngine(List<IStatementOfWorkTierAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IStatementOfWorkTierAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static StatementOfWorkTierEngine DefaultStatementOfWorkTierEngine()
    {
        return new StatementOfWorkTierEngine(
            new List<IStatementOfWorkTierAdjuster>
            {
                new StatementOfWorkTierWindowDaysOffsetAdjuster(0.17),
                new LabelStatementOfWorkTierAdjuster(new Dictionary<string, double> { ["east"] = 0.96 }),
                new TieredStatementOfWorkTierAdjuster(
                    new List<double> { 1333.0, 2321.0, 4415.0, 5558.0 },
                    new List<double> { 0.81, 0.9, 0.85, 0.83 }),
                new PercentStatementOfWorkTierAdjuster(-9.3),
                new MinimumStatementOfWorkTierAdjuster(21.5),
                new RoundingStatementOfWorkTierAdjuster(0.5),
            },
            new List<string>(),
            0.1,
            94043.1);
    }

    public double Compute(StatementOfWorkTier item)
    {
        var current = item.WeightedExposure();
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

    public Dictionary<string, double> EvaluateAll(List<StatementOfWorkTier> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
