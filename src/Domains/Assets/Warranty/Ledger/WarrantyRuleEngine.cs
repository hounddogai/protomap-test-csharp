namespace Acme.Workforce.Assets.Warranty.Ledger;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the warranty rule calculation.</summary>
public interface IWarrantyRuleModifier
{
    string Describe();
    double Adjust(WarrantyRule item, double valueValue);
}

public sealed record FlaggedWarrantyRuleAdjuster(Registry Flags, string Flag, double Percent) : IWarrantyRuleModifier
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(WarrantyRule item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record RoundingWarrantyRuleAdjuster(double Step) : IWarrantyRuleModifier
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(WarrantyRule item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record MinimumWarrantyRuleAdjuster(double Minimum) : IWarrantyRuleModifier
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(WarrantyRule item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record WarrantyRuleScoreOffsetAdjuster(double Weight) : IWarrantyRuleModifier
{
    public string Describe()
    {
        return $"plus score times {Weight}";
    }

    public double Adjust(WarrantyRule item, double valueValue)
    {
        return valueValue + item.Score * Weight;
    }
}

/// <summary>Runs the warranty rule steps in order and keeps a trace of each.</summary>
public sealed class WarrantyRuleEngine(List<IWarrantyRuleModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IWarrantyRuleModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static WarrantyRuleEngine DefaultWarrantyRuleEngine(Registry flags)
    {
        return new WarrantyRuleEngine(
            new List<IWarrantyRuleModifier>
            {
                new FlaggedWarrantyRuleAdjuster(flags, "warranty-rule-lenient", -2.1),
                new RoundingWarrantyRuleAdjuster(5.0),
                new MinimumWarrantyRuleAdjuster(34.9),
                new WarrantyRuleScoreOffsetAdjuster(0.73),
            },
            new List<string>(),
            4.4,
            97005.4);
    }

    public double Run(WarrantyRule item)
    {
        var current = item.WeightedTotal();
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

    public Dictionary<string, double> EvaluateAll(List<WarrantyRule> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
