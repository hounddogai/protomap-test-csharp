namespace Acme.Workforce.Contractors.StatementOfWork.Target;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the statement of work baseline calculation.</summary>
public interface IStatementOfWorkBaselineFactor
{
    string Describe();
    double Adjust(StatementOfWorkBaseline item, double valueValue);
}

public sealed record LabelStatementOfWorkBaselineAdjuster(Dictionary<string, double> Factors) : IStatementOfWorkBaselineFactor
{
    public string Describe()
    {
        return "by label";
    }

    public double Adjust(StatementOfWorkBaseline item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record CapStatementOfWorkBaselineAdjuster(double Cap) : IStatementOfWorkBaselineFactor
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(StatementOfWorkBaseline item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record MinimumStatementOfWorkBaselineAdjuster(double Minimum) : IStatementOfWorkBaselineFactor
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(StatementOfWorkBaseline item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record PercentStatementOfWorkBaselineAdjuster(double Percent) : IStatementOfWorkBaselineFactor
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(StatementOfWorkBaseline item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record TieredStatementOfWorkBaselineAdjuster(List<double> Bounds, List<double> Rates) : IStatementOfWorkBaselineFactor
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(StatementOfWorkBaseline item, double valueValue)
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

public sealed record RoundingStatementOfWorkBaselineAdjuster(double Step) : IStatementOfWorkBaselineFactor
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(StatementOfWorkBaseline item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

/// <summary>Runs the statement of work baseline steps in order and keeps a trace of each.</summary>
public sealed class StatementOfWorkBaselineEngine(List<IStatementOfWorkBaselineFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IStatementOfWorkBaselineFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static StatementOfWorkBaselineEngine DefaultStatementOfWorkBaselineEngine()
    {
        return new StatementOfWorkBaselineEngine(
            new List<IStatementOfWorkBaselineFactor>
            {
                new LabelStatementOfWorkBaselineAdjuster(
                    new Dictionary<string, double> { ["central"] = 1.3, ["south"] = 0.79, ["north"] = 0.91 }),
                new CapStatementOfWorkBaselineAdjuster(10579.0),
                new MinimumStatementOfWorkBaselineAdjuster(40.1),
                new PercentStatementOfWorkBaselineAdjuster(-1.2),
                new TieredStatementOfWorkBaselineAdjuster(
                    new List<double> { 397.0, 6068.0, 8612.0 },
                    new List<double> { 1.2, 0.87, 0.86 }),
                new RoundingStatementOfWorkBaselineAdjuster(0.05),
            },
            new List<string>(),
            1.3,
            6037.5);
    }

    public double Run(StatementOfWorkBaseline item)
    {
        var current = item.WeightedVolume();
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

    public Dictionary<string, double> EvaluateAll(List<StatementOfWorkBaseline> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
