namespace Acme.Workforce.Payrollops.Garnishment.Entry;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the rolling garnishment variance calculation.</summary>
public interface IRollingGarnishmentVarianceFactor
{
    string Describe();
    double Adjust(RollingGarnishmentVariance item, double valueValue);
}

public sealed record TieredRollingGarnishmentVarianceAdjuster(List<double> Bounds, List<double> Rates) : IRollingGarnishmentVarianceFactor
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(RollingGarnishmentVariance item, double valueValue)
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

public sealed record CategoryRollingGarnishmentVarianceAdjuster(Dictionary<string, double> Factors) : IRollingGarnishmentVarianceFactor
{
    public string Describe()
    {
        return "by category";
    }

    public double Adjust(RollingGarnishmentVariance item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record MinimumRollingGarnishmentVarianceAdjuster(double Minimum) : IRollingGarnishmentVarianceFactor
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(RollingGarnishmentVariance item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record CapRollingGarnishmentVarianceAdjuster(double Cap) : IRollingGarnishmentVarianceFactor
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(RollingGarnishmentVariance item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record RollingGarnishmentVarianceHoursOffsetAdjuster(double Weight) : IRollingGarnishmentVarianceFactor
{
    public string Describe()
    {
        return $"plus hours times {Weight}";
    }

    public double Adjust(RollingGarnishmentVariance item, double valueValue)
    {
        return valueValue + item.Hours * Weight;
    }
}

/// <summary>Runs the rolling garnishment variance steps in order and keeps a trace of each.</summary>
public sealed class RollingGarnishmentVarianceEngine(List<IRollingGarnishmentVarianceFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IRollingGarnishmentVarianceFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static RollingGarnishmentVarianceEngine DefaultRollingGarnishmentVarianceEngine()
    {
        return new RollingGarnishmentVarianceEngine(
            new List<IRollingGarnishmentVarianceFactor>
            {
                new TieredRollingGarnishmentVarianceAdjuster(
                    new List<double> { 1263.0, 7899.0 },
                    new List<double> { 0.95, 1.1 }),
                new CategoryRollingGarnishmentVarianceAdjuster(
                    new Dictionary<string, double> { ["retail"] = 0.78, ["north"] = 1.02, ["south"] = 1.08 }),
                new MinimumRollingGarnishmentVarianceAdjuster(22.5),
                new CapRollingGarnishmentVarianceAdjuster(12494.0),
                new RollingGarnishmentVarianceHoursOffsetAdjuster(1.13),
            },
            new List<string>(),
            4.5,
            22511.6);
    }

    public double Price(RollingGarnishmentVariance item)
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

    public Dictionary<string, double> EvaluateAll(List<RollingGarnishmentVariance> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
