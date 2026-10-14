namespace Acme.Workforce.Contractors.StatementOfWork.Bundle;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the statement of work bundle calculation.</summary>
public interface IStatementOfWorkBundleFactor
{
    string Describe();
    double Adjust(StatementOfWorkBundle item, double valueValue);
}

public sealed record StatementOfWorkBundleBudgetOffsetAdjuster(double Weight) : IStatementOfWorkBundleFactor
{
    public string Describe()
    {
        return $"plus budget times {Weight}";
    }

    public double Adjust(StatementOfWorkBundle item, double valueValue)
    {
        return valueValue + item.Budget * Weight;
    }
}

public sealed record TieredStatementOfWorkBundleAdjuster(List<double> Bounds, List<double> Rates) : IStatementOfWorkBundleFactor
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(StatementOfWorkBundle item, double valueValue)
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

public sealed record RoundingStatementOfWorkBundleAdjuster(double Step) : IStatementOfWorkBundleFactor
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(StatementOfWorkBundle item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

/// <summary>Runs the statement of work bundle steps in order and keeps a trace of each.</summary>
public sealed class StatementOfWorkBundleEngine(List<IStatementOfWorkBundleFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IStatementOfWorkBundleFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static StatementOfWorkBundleEngine DefaultStatementOfWorkBundleEngine()
    {
        return new StatementOfWorkBundleEngine(
            new List<IStatementOfWorkBundleFactor>
            {
                new StatementOfWorkBundleBudgetOffsetAdjuster(0.76),
                new TieredStatementOfWorkBundleAdjuster(
                    new List<double> { 2807.0, 7769.0, 9077.0, 9507.0 },
                    new List<double> { 0.83, 1.16, 1.04, 1.19 }),
                new RoundingStatementOfWorkBundleAdjuster(0.5),
            },
            new List<string>(),
            1.3,
            87448.0);
    }

    public double Run(StatementOfWorkBundle item)
    {
        var current = item.EffectiveLoad();
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

    public Dictionary<string, double> EvaluateAll(List<StatementOfWorkBundle> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
