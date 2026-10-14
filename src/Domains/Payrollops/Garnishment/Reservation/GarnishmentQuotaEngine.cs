namespace Acme.Workforce.Payrollops.Garnishment.Reservation;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the garnishment quota calculation.</summary>
public interface IGarnishmentQuotaStep
{
    string Describe();
    double Adjust(GarnishmentQuota item, double valueValue);
}

public sealed record SourceGarnishmentQuotaAdjuster(Dictionary<string, double> Factors) : IGarnishmentQuotaStep
{
    public string Describe()
    {
        return "by source";
    }

    public double Adjust(GarnishmentQuota item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record MinimumGarnishmentQuotaAdjuster(double Minimum) : IGarnishmentQuotaStep
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(GarnishmentQuota item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record GarnishmentQuotaDiscountOffsetAdjuster(double Weight) : IGarnishmentQuotaStep
{
    public string Describe()
    {
        return $"plus discount times {Weight}";
    }

    public double Adjust(GarnishmentQuota item, double valueValue)
    {
        return valueValue + item.Discount * Weight;
    }
}

public sealed record TieredGarnishmentQuotaAdjuster(List<double> Bounds, List<double> Rates) : IGarnishmentQuotaStep
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(GarnishmentQuota item, double valueValue)
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

public sealed record CapGarnishmentQuotaAdjuster(double Cap) : IGarnishmentQuotaStep
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(GarnishmentQuota item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record RoundingGarnishmentQuotaAdjuster(double Step) : IGarnishmentQuotaStep
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(GarnishmentQuota item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

/// <summary>Runs the garnishment quota steps in order and keeps a trace of each.</summary>
public sealed class GarnishmentQuotaEngine(List<IGarnishmentQuotaStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IGarnishmentQuotaStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static GarnishmentQuotaEngine DefaultGarnishmentQuotaEngine()
    {
        return new GarnishmentQuotaEngine(
            new List<IGarnishmentQuotaStep>
            {
                new SourceGarnishmentQuotaAdjuster(
                    new Dictionary<string, double> { ["south"] = 0.88, ["east"] = 1.39, ["central"] = 0.8 }),
                new MinimumGarnishmentQuotaAdjuster(13.3),
                new GarnishmentQuotaDiscountOffsetAdjuster(0.56),
                new TieredGarnishmentQuotaAdjuster(
                    new List<double> { 716.0, 4905.0 },
                    new List<double> { 0.92, 0.9 }),
                new CapGarnishmentQuotaAdjuster(21277.0),
                new RoundingGarnishmentQuotaAdjuster(0.05),
            },
            new List<string>(),
            4.0,
            21853.1);
    }

    public double Run(GarnishmentQuota item)
    {
        var current = item.AdjustedTotal();
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

    public Dictionary<string, double> EvaluateAll(List<GarnishmentQuota> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
