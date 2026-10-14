namespace Acme.Workforce.Assets.AssetReturn.Reservation;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the asset return reservation calculation.</summary>
public interface IAssetReturnReservationModifier
{
    string Describe();
    double Adjust(AssetReturnReservation item, double valueValue);
}

public sealed record TieredAssetReturnReservationAdjuster(List<double> Bounds, List<double> Rates) : IAssetReturnReservationModifier
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(AssetReturnReservation item, double valueValue)
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

public sealed record DepartmentAssetReturnReservationAdjuster(Dictionary<string, double> Factors) : IAssetReturnReservationModifier
{
    public string Describe()
    {
        return "by department";
    }

    public double Adjust(AssetReturnReservation item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record MinimumAssetReturnReservationAdjuster(double Minimum) : IAssetReturnReservationModifier
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(AssetReturnReservation item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record RoundingAssetReturnReservationAdjuster(double Step) : IAssetReturnReservationModifier
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(AssetReturnReservation item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

/// <summary>Runs the asset return reservation steps in order and keeps a trace of each.</summary>
public sealed class AssetReturnReservationEngine(List<IAssetReturnReservationModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IAssetReturnReservationModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static AssetReturnReservationEngine DefaultAssetReturnReservationEngine()
    {
        return new AssetReturnReservationEngine(
            new List<IAssetReturnReservationModifier>
            {
                new TieredAssetReturnReservationAdjuster(
                    new List<double> { 2522.0, 5338.0, 7121.0 },
                    new List<double> { 1.1, 0.96, 1.04 }),
                new DepartmentAssetReturnReservationAdjuster(new Dictionary<string, double> { ["online"] = 1.39 }),
                new MinimumAssetReturnReservationAdjuster(17.4),
                new RoundingAssetReturnReservationAdjuster(0.1),
            },
            new List<string>(),
            3.2,
            82792.7);
    }

    public double Evaluate(AssetReturnReservation item)
    {
        var current = item.ProjectedValue();
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

    public Dictionary<string, double> EvaluateAll(List<AssetReturnReservation> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
