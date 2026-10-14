namespace Acme.Workforce.Workplace.Visitor.Cycle;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the default visitor rule calculation.</summary>
public interface IDefaultVisitorRuleAdjuster
{
    string Describe();
    double Adjust(DefaultVisitorRule item, double valueValue);
}

public sealed record RoundingDefaultVisitorRuleAdjuster(double Step) : IDefaultVisitorRuleAdjuster
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(DefaultVisitorRule item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record PercentDefaultVisitorRuleAdjuster(double Percent) : IDefaultVisitorRuleAdjuster
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(DefaultVisitorRule item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record MinimumDefaultVisitorRuleAdjuster(double Minimum) : IDefaultVisitorRuleAdjuster
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(DefaultVisitorRule item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record DefaultVisitorRuleAttemptsOffsetAdjuster(double Weight) : IDefaultVisitorRuleAdjuster
{
    public string Describe()
    {
        return $"plus attempts times {Weight}";
    }

    public double Adjust(DefaultVisitorRule item, double valueValue)
    {
        return valueValue + (double)item.Attempts * Weight;
    }
}

public sealed record CapDefaultVisitorRuleAdjuster(double Cap) : IDefaultVisitorRuleAdjuster
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(DefaultVisitorRule item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record FlaggedDefaultVisitorRuleAdjuster(Registry Flags, string Flag, double Percent) : IDefaultVisitorRuleAdjuster
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(DefaultVisitorRule item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the default visitor rule steps in order and keeps a trace of each.</summary>
public sealed class DefaultVisitorRuleEngine(List<IDefaultVisitorRuleAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IDefaultVisitorRuleAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static DefaultVisitorRuleEngine DefaultDefaultVisitorRuleEngine(Registry flags)
    {
        return new DefaultVisitorRuleEngine(
            new List<IDefaultVisitorRuleAdjuster>
            {
                new RoundingDefaultVisitorRuleAdjuster(0.05),
                new PercentDefaultVisitorRuleAdjuster(22.9),
                new MinimumDefaultVisitorRuleAdjuster(9.5),
                new DefaultVisitorRuleAttemptsOffsetAdjuster(0.42),
                new CapDefaultVisitorRuleAdjuster(10439.0),
                new FlaggedDefaultVisitorRuleAdjuster(flags, "default-visitor-rule-seasonal", 0.2),
            },
            new List<string>(),
            1.9,
            58536.8);
    }

    public double Evaluate(DefaultVisitorRule item)
    {
        var current = item.WeightedExposure();
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

    public Dictionary<string, double> EvaluateAll(List<DefaultVisitorRule> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
