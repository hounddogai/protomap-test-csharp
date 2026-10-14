namespace Acme.Workforce.Assets.AssetReturn.Budget;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the rolling asset return plan calculation.</summary>
public interface IRollingAssetReturnPlanFactor
{
    string Describe();
    double Adjust(RollingAssetReturnPlan item, double valueValue);
}

public sealed record CapRollingAssetReturnPlanAdjuster(double Cap) : IRollingAssetReturnPlanFactor
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(RollingAssetReturnPlan item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record RoundingRollingAssetReturnPlanAdjuster(double Step) : IRollingAssetReturnPlanFactor
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(RollingAssetReturnPlan item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record TieredRollingAssetReturnPlanAdjuster(List<double> Bounds, List<double> Rates) : IRollingAssetReturnPlanFactor
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(RollingAssetReturnPlan item, double valueValue)
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

public sealed record RollingAssetReturnPlanEndsAtOffsetAdjuster(double Weight) : IRollingAssetReturnPlanFactor
{
    public string Describe()
    {
        return $"plus ends at times {Weight}";
    }

    public double Adjust(RollingAssetReturnPlan item, double valueValue)
    {
        return valueValue + (double)item.EndsAt * Weight;
    }
}

public sealed record PercentRollingAssetReturnPlanAdjuster(double Percent) : IRollingAssetReturnPlanFactor
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(RollingAssetReturnPlan item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the rolling asset return plan steps in order and keeps a trace of each.</summary>
public sealed class RollingAssetReturnPlanEngine(List<IRollingAssetReturnPlanFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IRollingAssetReturnPlanFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static RollingAssetReturnPlanEngine DefaultRollingAssetReturnPlanEngine()
    {
        return new RollingAssetReturnPlanEngine(
            new List<IRollingAssetReturnPlanFactor>
            {
                new CapRollingAssetReturnPlanAdjuster(6176.0),
                new RoundingRollingAssetReturnPlanAdjuster(0.5),
                new TieredRollingAssetReturnPlanAdjuster(
                    new List<double> { 3516.0, 8928.0, 9438.0, 9527.0 },
                    new List<double> { 0.97, 0.96, 1.13, 0.94 }),
                new RollingAssetReturnPlanEndsAtOffsetAdjuster(0.62),
                new PercentRollingAssetReturnPlanAdjuster(5.0),
            },
            new List<string>(),
            2.9,
            92997.4);
    }

    public double Compute(RollingAssetReturnPlan item)
    {
        var current = item.AdjustedTotal();
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

    public Dictionary<string, double> EvaluateAll(List<RollingAssetReturnPlan> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
