namespace Acme.Workforce.Compensation.EquityGrant.GrantAssignment;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the weighted equity grant record calculation.</summary>
public interface IWeightedEquityGrantRecordStep
{
    string Describe();
    double Adjust(WeightedEquityGrantRecord item, double valueValue);
}

public sealed record TieredWeightedEquityGrantRecordAdjuster(List<double> Bounds, List<double> Rates) : IWeightedEquityGrantRecordStep
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(WeightedEquityGrantRecord item, double valueValue)
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

public sealed record PercentWeightedEquityGrantRecordAdjuster(double Percent) : IWeightedEquityGrantRecordStep
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(WeightedEquityGrantRecord item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record MinimumWeightedEquityGrantRecordAdjuster(double Minimum) : IWeightedEquityGrantRecordStep
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(WeightedEquityGrantRecord item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record CapWeightedEquityGrantRecordAdjuster(double Cap) : IWeightedEquityGrantRecordStep
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(WeightedEquityGrantRecord item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record WeightedEquityGrantRecordDiscountOffsetAdjuster(double Weight) : IWeightedEquityGrantRecordStep
{
    public string Describe()
    {
        return $"plus discount times {Weight}";
    }

    public double Adjust(WeightedEquityGrantRecord item, double valueValue)
    {
        return valueValue + item.Discount * Weight;
    }
}

public sealed record FlaggedWeightedEquityGrantRecordAdjuster(Registry Flags, string Flag, double Percent) : IWeightedEquityGrantRecordStep
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(WeightedEquityGrantRecord item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the weighted equity grant record steps in order and keeps a trace of each.</summary>
public sealed class WeightedEquityGrantRecordEngine(List<IWeightedEquityGrantRecordStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IWeightedEquityGrantRecordStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static WeightedEquityGrantRecordEngine DefaultWeightedEquityGrantRecordEngine(Registry flags)
    {
        return new WeightedEquityGrantRecordEngine(
            new List<IWeightedEquityGrantRecordStep>
            {
                new TieredWeightedEquityGrantRecordAdjuster(
                    new List<double> { 3359.0, 7593.0, 7671.0, 9006.0 },
                    new List<double> { 0.84, 0.93, 0.88, 0.94 }),
                new PercentWeightedEquityGrantRecordAdjuster(-20.4),
                new MinimumWeightedEquityGrantRecordAdjuster(33.1),
                new CapWeightedEquityGrantRecordAdjuster(20473.0),
                new WeightedEquityGrantRecordDiscountOffsetAdjuster(0.1),
                new FlaggedWeightedEquityGrantRecordAdjuster(flags, "weighted-equity-grant-record-regional", -6.0),
            },
            new List<string>(),
            0.9,
            16684.8);
    }

    public double Evaluate(WeightedEquityGrantRecord item)
    {
        var current = item.NetVolume();
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

    public Dictionary<string, double> EvaluateAll(List<WeightedEquityGrantRecord> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
