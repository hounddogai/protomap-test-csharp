namespace Acme.Workforce.Contractors.StatementOfWork.Tier;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the tiered statement of work adjustment calculation.</summary>
public interface ITieredStatementOfWorkAdjustmentAdjuster
{
    string Describe();
    double Adjust(TieredStatementOfWorkAdjustment item, double valueValue);
}

public sealed record RoundingTieredStatementOfWorkAdjustmentAdjuster(double Step) : ITieredStatementOfWorkAdjustmentAdjuster
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(TieredStatementOfWorkAdjustment item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record MinimumTieredStatementOfWorkAdjustmentAdjuster(double Minimum) : ITieredStatementOfWorkAdjustmentAdjuster
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(TieredStatementOfWorkAdjustment item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record CapTieredStatementOfWorkAdjustmentAdjuster(double Cap) : ITieredStatementOfWorkAdjustmentAdjuster
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(TieredStatementOfWorkAdjustment item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record TieredTieredStatementOfWorkAdjustmentAdjuster(List<double> Bounds, List<double> Rates) : ITieredStatementOfWorkAdjustmentAdjuster
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(TieredStatementOfWorkAdjustment item, double valueValue)
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

/// <summary>Runs the tiered statement of work adjustment steps in order and keeps a trace of each.</summary>
public sealed class TieredStatementOfWorkAdjustmentEngine(List<ITieredStatementOfWorkAdjustmentAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<ITieredStatementOfWorkAdjustmentAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static TieredStatementOfWorkAdjustmentEngine DefaultTieredStatementOfWorkAdjustmentEngine()
    {
        return new TieredStatementOfWorkAdjustmentEngine(
            new List<ITieredStatementOfWorkAdjustmentAdjuster>
            {
                new RoundingTieredStatementOfWorkAdjustmentAdjuster(0.5),
                new MinimumTieredStatementOfWorkAdjustmentAdjuster(17.8),
                new CapTieredStatementOfWorkAdjustmentAdjuster(19461.0),
                new TieredTieredStatementOfWorkAdjustmentAdjuster(
                    new List<double> { 2090.0, 5255.0, 6055.0, 8108.0 },
                    new List<double> { 0.81, 0.85, 1.14, 1.14 }),
            },
            new List<string>(),
            2.1,
            90288.1);
    }

    public double Evaluate(TieredStatementOfWorkAdjustment item)
    {
        var current = item.ProjectedTotal();
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

    public Dictionary<string, double> EvaluateAll(List<TieredStatementOfWorkAdjustment> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
