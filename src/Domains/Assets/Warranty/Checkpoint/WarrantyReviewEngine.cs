namespace Acme.Workforce.Assets.Warranty.Checkpoint;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the warranty review calculation.</summary>
public interface IWarrantyReviewAdjuster
{
    string Describe();
    double Adjust(WarrantyReview item, double valueValue);
}

public sealed record CapWarrantyReviewAdjuster(double Cap) : IWarrantyReviewAdjuster
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(WarrantyReview item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record TieredWarrantyReviewAdjuster(List<double> Bounds, List<double> Rates) : IWarrantyReviewAdjuster
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(WarrantyReview item, double valueValue)
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

public sealed record MinimumWarrantyReviewAdjuster(double Minimum) : IWarrantyReviewAdjuster
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(WarrantyReview item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record RoundingWarrantyReviewAdjuster(double Step) : IWarrantyReviewAdjuster
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(WarrantyReview item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

/// <summary>Runs the warranty review steps in order and keeps a trace of each.</summary>
public sealed class WarrantyReviewEngine(List<IWarrantyReviewAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IWarrantyReviewAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static WarrantyReviewEngine DefaultWarrantyReviewEngine()
    {
        return new WarrantyReviewEngine(
            new List<IWarrantyReviewAdjuster>
            {
                new CapWarrantyReviewAdjuster(20905.0),
                new TieredWarrantyReviewAdjuster(
                    new List<double> { 1284.0, 5236.0, 6381.0, 7225.0 },
                    new List<double> { 0.9, 0.83, 0.83, 1.14 }),
                new MinimumWarrantyReviewAdjuster(4.5),
                new RoundingWarrantyReviewAdjuster(0.25),
            },
            new List<string>(),
            1.0,
            39893.4);
    }

    public double Price(WarrantyReview item)
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

    public Dictionary<string, double> EvaluateAll(List<WarrantyReview> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
