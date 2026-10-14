namespace Acme.Workforce.Assets.AssetReturn.Reservation;

/// <summary>One step of the asset return cycle calculation.</summary>
public interface IAssetReturnCycleStep
{
    string Describe();
    double Adjust(AssetReturnCycle item, double valueValue);
}

public sealed record DepartmentAssetReturnCycleAdjuster(Dictionary<string, double> Factors) : IAssetReturnCycleStep
{
    public string Describe()
    {
        return "by department";
    }

    public double Adjust(AssetReturnCycle item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record MinimumAssetReturnCycleAdjuster(double Minimum) : IAssetReturnCycleStep
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(AssetReturnCycle item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record AssetReturnCycleUpdatedAtOffsetAdjuster(double Weight) : IAssetReturnCycleStep
{
    public string Describe()
    {
        return $"plus updated at times {Weight}";
    }

    public double Adjust(AssetReturnCycle item, double valueValue)
    {
        return valueValue + (double)item.UpdatedAt * Weight;
    }
}

/// <summary>Runs the asset return cycle steps in order and keeps a trace of each.</summary>
public sealed class AssetReturnCycleEngine(List<IAssetReturnCycleStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IAssetReturnCycleStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static AssetReturnCycleEngine DefaultAssetReturnCycleEngine()
    {
        return new AssetReturnCycleEngine(
            new List<IAssetReturnCycleStep>
            {
                new DepartmentAssetReturnCycleAdjuster(
                    new Dictionary<string, double> { ["central"] = 1.01, ["online"] = 1.28 }),
                new MinimumAssetReturnCycleAdjuster(40.3),
                new AssetReturnCycleUpdatedAtOffsetAdjuster(0.62),
            },
            new List<string>(),
            2.1,
            10927.2);
    }

    public double Compute(AssetReturnCycle item)
    {
        var current = item.EffectiveExposure();
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

    public Dictionary<string, double> EvaluateAll(List<AssetReturnCycle> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
