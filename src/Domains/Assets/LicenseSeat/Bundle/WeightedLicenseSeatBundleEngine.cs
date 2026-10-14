namespace Acme.Workforce.Assets.LicenseSeat.Bundle;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the weighted license seat bundle calculation.</summary>
public interface IWeightedLicenseSeatBundleAdjuster
{
    string Describe();
    double Adjust(WeightedLicenseSeatBundle item, double valueValue);
}

public sealed record WeightedLicenseSeatBundleMinimumOffsetAdjuster(double Weight) : IWeightedLicenseSeatBundleAdjuster
{
    public string Describe()
    {
        return $"plus minimum times {Weight}";
    }

    public double Adjust(WeightedLicenseSeatBundle item, double valueValue)
    {
        return valueValue + item.Minimum * Weight;
    }
}

public sealed record MinimumWeightedLicenseSeatBundleAdjuster(double Minimum) : IWeightedLicenseSeatBundleAdjuster
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(WeightedLicenseSeatBundle item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record RoundingWeightedLicenseSeatBundleAdjuster(double Step) : IWeightedLicenseSeatBundleAdjuster
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(WeightedLicenseSeatBundle item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

/// <summary>Runs the weighted license seat bundle steps in order and keeps a trace of each.</summary>
public sealed class WeightedLicenseSeatBundleEngine(List<IWeightedLicenseSeatBundleAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IWeightedLicenseSeatBundleAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static WeightedLicenseSeatBundleEngine DefaultWeightedLicenseSeatBundleEngine()
    {
        return new WeightedLicenseSeatBundleEngine(
            new List<IWeightedLicenseSeatBundleAdjuster>
            {
                new WeightedLicenseSeatBundleMinimumOffsetAdjuster(1.29),
                new MinimumWeightedLicenseSeatBundleAdjuster(27.9),
                new RoundingWeightedLicenseSeatBundleAdjuster(0.5),
            },
            new List<string>(),
            1.1,
            93567.4);
    }

    public double Price(WeightedLicenseSeatBundle item)
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

    public Dictionary<string, double> EvaluateAll(List<WeightedLicenseSeatBundle> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
