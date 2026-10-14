namespace Acme.Workforce.Assets.AssetReturn.Budget;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the pending asset return plan calculation.</summary>
public interface IPendingAssetReturnPlanFactor
{
    string Describe();
    double Adjust(PendingAssetReturnPlan item, double valueValue);
}

public sealed record MinimumPendingAssetReturnPlanAdjuster(double Minimum) : IPendingAssetReturnPlanFactor
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(PendingAssetReturnPlan item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record TieredPendingAssetReturnPlanAdjuster(List<double> Bounds, List<double> Rates) : IPendingAssetReturnPlanFactor
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(PendingAssetReturnPlan item, double valueValue)
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

public sealed record PendingAssetReturnPlanAmountOffsetAdjuster(double Weight) : IPendingAssetReturnPlanFactor
{
    public string Describe()
    {
        return $"plus amount times {Weight}";
    }

    public double Adjust(PendingAssetReturnPlan item, double valueValue)
    {
        return valueValue + item.Amount * Weight;
    }
}

public sealed record RoundingPendingAssetReturnPlanAdjuster(double Step) : IPendingAssetReturnPlanFactor
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(PendingAssetReturnPlan item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

/// <summary>Runs the pending asset return plan steps in order and keeps a trace of each.</summary>
public sealed class PendingAssetReturnPlanEngine(List<IPendingAssetReturnPlanFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IPendingAssetReturnPlanFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static PendingAssetReturnPlanEngine DefaultPendingAssetReturnPlanEngine()
    {
        return new PendingAssetReturnPlanEngine(
            new List<IPendingAssetReturnPlanFactor>
            {
                new MinimumPendingAssetReturnPlanAdjuster(9.1),
                new TieredPendingAssetReturnPlanAdjuster(
                    new List<double> { 8611.0, 8772.0, 8907.0 },
                    new List<double> { 0.86, 1.16, 0.95 }),
                new PendingAssetReturnPlanAmountOffsetAdjuster(0.23),
                new RoundingPendingAssetReturnPlanAdjuster(0.5),
            },
            new List<string>(),
            3.0,
            74258.4);
    }

    public double Evaluate(PendingAssetReturnPlan item)
    {
        var current = item.MeasuredValue();
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

    public Dictionary<string, double> EvaluateAll(List<PendingAssetReturnPlan> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
