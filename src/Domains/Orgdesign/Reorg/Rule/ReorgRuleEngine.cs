namespace Acme.Workforce.Orgdesign.Reorg.Rule;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the reorg rule calculation.</summary>
public interface IReorgRuleModifier
{
    string Describe();
    double Adjust(ReorgRule item, double valueValue);
}

public sealed record FlaggedReorgRuleAdjuster(Registry Flags, string Flag, double Percent) : IReorgRuleModifier
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(ReorgRule item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record CapReorgRuleAdjuster(double Cap) : IReorgRuleModifier
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(ReorgRule item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record PercentReorgRuleAdjuster(double Percent) : IReorgRuleModifier
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(ReorgRule item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record RoundingReorgRuleAdjuster(double Step) : IReorgRuleModifier
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(ReorgRule item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record MinimumReorgRuleAdjuster(double Minimum) : IReorgRuleModifier
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(ReorgRule item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

/// <summary>Runs the reorg rule steps in order and keeps a trace of each.</summary>
public sealed class ReorgRuleEngine(List<IReorgRuleModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IReorgRuleModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static ReorgRuleEngine DefaultReorgRuleEngine(Registry flags)
    {
        return new ReorgRuleEngine(
            new List<IReorgRuleModifier>
            {
                new FlaggedReorgRuleAdjuster(flags, "reorg-rule-legacy", -7.0),
                new CapReorgRuleAdjuster(5761.0),
                new PercentReorgRuleAdjuster(-17.2),
                new RoundingReorgRuleAdjuster(0.5),
                new MinimumReorgRuleAdjuster(38.4),
            },
            new List<string>(),
            1.4,
            75581.6);
    }

    public double Compute(ReorgRule item)
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

    public Dictionary<string, double> EvaluateAll(List<ReorgRule> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
