namespace Acme.Workforce.Workplace.Desk.DeskBudget;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the rolling desk mapping calculation.</summary>
public interface IRollingDeskMappingStep
{
    string Describe();
    double Adjust(RollingDeskMapping item, double valueValue);
}

public sealed record TieredRollingDeskMappingAdjuster(List<double> Bounds, List<double> Rates) : IRollingDeskMappingStep
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(RollingDeskMapping item, double valueValue)
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

public sealed record SegmentRollingDeskMappingAdjuster(Dictionary<string, double> Factors) : IRollingDeskMappingStep
{
    public string Describe()
    {
        return "by segment";
    }

    public double Adjust(RollingDeskMapping item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record MinimumRollingDeskMappingAdjuster(double Minimum) : IRollingDeskMappingStep
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(RollingDeskMapping item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record CapRollingDeskMappingAdjuster(double Cap) : IRollingDeskMappingStep
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(RollingDeskMapping item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record PercentRollingDeskMappingAdjuster(double Percent) : IRollingDeskMappingStep
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(RollingDeskMapping item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the rolling desk mapping steps in order and keeps a trace of each.</summary>
public sealed class RollingDeskMappingEngine(List<IRollingDeskMappingStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IRollingDeskMappingStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static RollingDeskMappingEngine DefaultRollingDeskMappingEngine()
    {
        return new RollingDeskMappingEngine(
            new List<IRollingDeskMappingStep>
            {
                new TieredRollingDeskMappingAdjuster(
                    new List<double> { 6532.0, 7373.0, 9375.0 },
                    new List<double> { 0.9, 1.14, 0.89 }),
                new SegmentRollingDeskMappingAdjuster(
                    new Dictionary<string, double> { ["north"] = 1.1, ["south"] = 1.16, ["east"] = 1.19 }),
                new MinimumRollingDeskMappingAdjuster(11.9),
                new CapRollingDeskMappingAdjuster(35245.0),
                new PercentRollingDeskMappingAdjuster(-22.8),
            },
            new List<string>(),
            3.4,
            53691.2);
    }

    public double Price(RollingDeskMapping item)
    {
        var current = item.NetTotal();
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

    public Dictionary<string, double> EvaluateAll(List<RollingDeskMapping> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
