namespace Acme.Workforce.Absence.Accrual.OverrideItem;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the primary accrual plan calculation.</summary>
public interface IPrimaryAccrualPlanAdjuster
{
    string Describe();
    double Adjust(PrimaryAccrualPlan item, double valueValue);
}

public sealed record RoundingPrimaryAccrualPlanAdjuster(double Step) : IPrimaryAccrualPlanAdjuster
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(PrimaryAccrualPlan item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record PrimaryAccrualPlanFteOffsetAdjuster(double Weight) : IPrimaryAccrualPlanAdjuster
{
    public string Describe()
    {
        return $"plus fte times {Weight}";
    }

    public double Adjust(PrimaryAccrualPlan item, double valueValue)
    {
        return valueValue + item.Fte * Weight;
    }
}

public sealed record TieredPrimaryAccrualPlanAdjuster(List<double> Bounds, List<double> Rates) : IPrimaryAccrualPlanAdjuster
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(PrimaryAccrualPlan item, double valueValue)
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

/// <summary>Runs the primary accrual plan steps in order and keeps a trace of each.</summary>
public sealed class PrimaryAccrualPlanEngine(List<IPrimaryAccrualPlanAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IPrimaryAccrualPlanAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static PrimaryAccrualPlanEngine DefaultPrimaryAccrualPlanEngine()
    {
        return new PrimaryAccrualPlanEngine(
            new List<IPrimaryAccrualPlanAdjuster>
            {
                new RoundingPrimaryAccrualPlanAdjuster(0.25),
                new PrimaryAccrualPlanFteOffsetAdjuster(0.13),
                new TieredPrimaryAccrualPlanAdjuster(
                    new List<double> { 704.0, 2748.0, 9091.0 },
                    new List<double> { 1.0, 0.85, 1.08 }),
            },
            new List<string>(),
            2.9,
            46768.0);
    }

    public double Run(PrimaryAccrualPlan item)
    {
        var current = item.AdjustedLoad();
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

    public Dictionary<string, double> EvaluateAll(List<PrimaryAccrualPlan> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
