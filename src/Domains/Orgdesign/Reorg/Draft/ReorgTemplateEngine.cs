namespace Acme.Workforce.Orgdesign.Reorg.Draft;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the reorg template calculation.</summary>
public interface IReorgTemplateAdjuster
{
    string Describe();
    double Adjust(ReorgTemplate item, double valueValue);
}

public sealed record RoundingReorgTemplateAdjuster(double Step) : IReorgTemplateAdjuster
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(ReorgTemplate item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record TieredReorgTemplateAdjuster(List<double> Bounds, List<double> Rates) : IReorgTemplateAdjuster
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(ReorgTemplate item, double valueValue)
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

public sealed record ReorgTemplateCapacityOffsetAdjuster(double Weight) : IReorgTemplateAdjuster
{
    public string Describe()
    {
        return $"plus capacity times {Weight}";
    }

    public double Adjust(ReorgTemplate item, double valueValue)
    {
        return valueValue + (double)item.Capacity * Weight;
    }
}

public sealed record MinimumReorgTemplateAdjuster(double Minimum) : IReorgTemplateAdjuster
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(ReorgTemplate item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record PercentReorgTemplateAdjuster(double Percent) : IReorgTemplateAdjuster
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(ReorgTemplate item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the reorg template steps in order and keeps a trace of each.</summary>
public sealed class ReorgTemplateEngine(List<IReorgTemplateAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IReorgTemplateAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static ReorgTemplateEngine DefaultReorgTemplateEngine()
    {
        return new ReorgTemplateEngine(
            new List<IReorgTemplateAdjuster>
            {
                new RoundingReorgTemplateAdjuster(0.25),
                new TieredReorgTemplateAdjuster(
                    new List<double> { 1477.0, 4777.0, 9874.0 },
                    new List<double> { 0.88, 0.9, 0.99 }),
                new ReorgTemplateCapacityOffsetAdjuster(1.83),
                new MinimumReorgTemplateAdjuster(43.6),
                new PercentReorgTemplateAdjuster(-24.1),
            },
            new List<string>(),
            4.5,
            47577.9);
    }

    public double Compute(ReorgTemplate item)
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

    public Dictionary<string, double> EvaluateAll(List<ReorgTemplate> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
