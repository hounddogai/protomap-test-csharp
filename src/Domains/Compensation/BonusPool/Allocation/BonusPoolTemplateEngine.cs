namespace Acme.Workforce.Compensation.BonusPool.Allocation;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the bonus pool template calculation.</summary>
public interface IBonusPoolTemplateAdjuster
{
    string Describe();
    double Adjust(BonusPoolTemplate item, double valueValue);
}

public sealed record DepartmentBonusPoolTemplateAdjuster(Dictionary<string, double> Factors) : IBonusPoolTemplateAdjuster
{
    public string Describe()
    {
        return "by department";
    }

    public double Adjust(BonusPoolTemplate item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record TieredBonusPoolTemplateAdjuster(List<double> Bounds, List<double> Rates) : IBonusPoolTemplateAdjuster
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(BonusPoolTemplate item, double valueValue)
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

public sealed record MinimumBonusPoolTemplateAdjuster(double Minimum) : IBonusPoolTemplateAdjuster
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(BonusPoolTemplate item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record FlaggedBonusPoolTemplateAdjuster(Registry Flags, string Flag, double Percent) : IBonusPoolTemplateAdjuster
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(BonusPoolTemplate item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the bonus pool template steps in order and keeps a trace of each.</summary>
public sealed class BonusPoolTemplateEngine(List<IBonusPoolTemplateAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IBonusPoolTemplateAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static BonusPoolTemplateEngine DefaultBonusPoolTemplateEngine(Registry flags)
    {
        return new BonusPoolTemplateEngine(
            new List<IBonusPoolTemplateAdjuster>
            {
                new DepartmentBonusPoolTemplateAdjuster(new Dictionary<string, double> { ["east"] = 1.25 }),
                new TieredBonusPoolTemplateAdjuster(
                    new List<double> { 3542.0, 9194.0 },
                    new List<double> { 0.89, 0.9 }),
                new MinimumBonusPoolTemplateAdjuster(43.5),
                new FlaggedBonusPoolTemplateAdjuster(flags, "bonus-pool-template-rolling", 3.1),
            },
            new List<string>(),
            1.9,
            73266.1);
    }

    public double Price(BonusPoolTemplate item)
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

    public Dictionary<string, double> EvaluateAll(List<BonusPoolTemplate> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
