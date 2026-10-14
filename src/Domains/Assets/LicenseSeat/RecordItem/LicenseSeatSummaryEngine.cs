namespace Acme.Workforce.Assets.LicenseSeat.RecordItem;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the license seat summary calculation.</summary>
public interface ILicenseSeatSummaryStep
{
    string Describe();
    double Adjust(LicenseSeatSummary item, double valueValue);
}

public sealed record LicenseSeatSummaryDiscountOffsetAdjuster(double Weight) : ILicenseSeatSummaryStep
{
    public string Describe()
    {
        return $"plus discount times {Weight}";
    }

    public double Adjust(LicenseSeatSummary item, double valueValue)
    {
        return valueValue + item.Discount * Weight;
    }
}

public sealed record TitleLicenseSeatSummaryAdjuster(Dictionary<string, double> Factors) : ILicenseSeatSummaryStep
{
    public string Describe()
    {
        return "by title";
    }

    public double Adjust(LicenseSeatSummary item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record TieredLicenseSeatSummaryAdjuster(List<double> Bounds, List<double> Rates) : ILicenseSeatSummaryStep
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(LicenseSeatSummary item, double valueValue)
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

public sealed record MinimumLicenseSeatSummaryAdjuster(double Minimum) : ILicenseSeatSummaryStep
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(LicenseSeatSummary item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record PercentLicenseSeatSummaryAdjuster(double Percent) : ILicenseSeatSummaryStep
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(LicenseSeatSummary item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record CapLicenseSeatSummaryAdjuster(double Cap) : ILicenseSeatSummaryStep
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(LicenseSeatSummary item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

/// <summary>Runs the license seat summary steps in order and keeps a trace of each.</summary>
public sealed class LicenseSeatSummaryEngine(List<ILicenseSeatSummaryStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<ILicenseSeatSummaryStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static LicenseSeatSummaryEngine DefaultLicenseSeatSummaryEngine()
    {
        return new LicenseSeatSummaryEngine(
            new List<ILicenseSeatSummaryStep>
            {
                new LicenseSeatSummaryDiscountOffsetAdjuster(0.18),
                new TitleLicenseSeatSummaryAdjuster(new Dictionary<string, double> { ["online"] = 0.81 }),
                new TieredLicenseSeatSummaryAdjuster(
                    new List<double> { 3201.0, 5437.0, 8896.0 },
                    new List<double> { 1.0, 0.91, 1.07 }),
                new MinimumLicenseSeatSummaryAdjuster(12.1),
                new PercentLicenseSeatSummaryAdjuster(-9.8),
                new CapLicenseSeatSummaryAdjuster(19518.0),
            },
            new List<string>(),
            2.5,
            83546.0);
    }

    public double Run(LicenseSeatSummary item)
    {
        var current = item.NetValue();
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

    public Dictionary<string, double> EvaluateAll(List<LicenseSeatSummary> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
