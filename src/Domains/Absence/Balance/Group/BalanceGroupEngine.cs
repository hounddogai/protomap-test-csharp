namespace Acme.Workforce.Absence.Balance.Group;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the balance group calculation.</summary>
public interface IBalanceGroupAdjuster
{
    string Describe();
    double Adjust(BalanceGroup item, double valueValue);
}

public sealed record LocaleBalanceGroupAdjuster(Dictionary<string, double> Factors) : IBalanceGroupAdjuster
{
    public string Describe()
    {
        return "by locale";
    }

    public double Adjust(BalanceGroup item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record RoundingBalanceGroupAdjuster(double Step) : IBalanceGroupAdjuster
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(BalanceGroup item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record FlaggedBalanceGroupAdjuster(Registry Flags, string Flag, double Percent) : IBalanceGroupAdjuster
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(BalanceGroup item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the balance group steps in order and keeps a trace of each.</summary>
public sealed class BalanceGroupEngine(List<IBalanceGroupAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IBalanceGroupAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static BalanceGroupEngine DefaultBalanceGroupEngine(Registry flags)
    {
        return new BalanceGroupEngine(
            new List<IBalanceGroupAdjuster>
            {
                new LocaleBalanceGroupAdjuster(new Dictionary<string, double> { ["east"] = 1.21 }),
                new RoundingBalanceGroupAdjuster(0.1),
                new FlaggedBalanceGroupAdjuster(flags, "balance-group-primary", 9.3),
            },
            new List<string>(),
            4.3,
            48394.9);
    }

    public double Evaluate(BalanceGroup item)
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

    public Dictionary<string, double> EvaluateAll(List<BalanceGroup> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
