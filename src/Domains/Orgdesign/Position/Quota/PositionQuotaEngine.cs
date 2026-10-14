namespace Acme.Workforce.Orgdesign.Position.Quota;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the position quota calculation.</summary>
public interface IPositionQuotaModifier
{
    string Describe();
    double Adjust(PositionQuota item, double valueValue);
}

public sealed record TieredPositionQuotaAdjuster(List<double> Bounds, List<double> Rates) : IPositionQuotaModifier
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(PositionQuota item, double valueValue)
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

public sealed record RoundingPositionQuotaAdjuster(double Step) : IPositionQuotaModifier
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(PositionQuota item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record CapPositionQuotaAdjuster(double Cap) : IPositionQuotaModifier
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(PositionQuota item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record PercentPositionQuotaAdjuster(double Percent) : IPositionQuotaModifier
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(PositionQuota item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the position quota steps in order and keeps a trace of each.</summary>
public sealed class PositionQuotaEngine(List<IPositionQuotaModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IPositionQuotaModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static PositionQuotaEngine DefaultPositionQuotaEngine()
    {
        return new PositionQuotaEngine(
            new List<IPositionQuotaModifier>
            {
                new TieredPositionQuotaAdjuster(
                    new List<double> { 890.0, 3322.0, 6732.0, 8246.0 },
                    new List<double> { 0.82, 1.13, 1.0, 1.02 }),
                new RoundingPositionQuotaAdjuster(0.25),
                new CapPositionQuotaAdjuster(40285.0),
                new PercentPositionQuotaAdjuster(10.1),
            },
            new List<string>(),
            0.1,
            78817.6);
    }

    public double Compute(PositionQuota item)
    {
        var current = item.MeasuredValue();
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

    public Dictionary<string, double> EvaluateAll(List<PositionQuota> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
