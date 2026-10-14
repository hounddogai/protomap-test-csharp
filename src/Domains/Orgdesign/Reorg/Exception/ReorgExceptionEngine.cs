namespace Acme.Workforce.Orgdesign.Reorg.Exception;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the reorg exception calculation.</summary>
public interface IReorgExceptionStep
{
    string Describe();
    double Adjust(ReorgException item, double valueValue);
}

public sealed record MinimumReorgExceptionAdjuster(double Minimum) : IReorgExceptionStep
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(ReorgException item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record DepartmentReorgExceptionAdjuster(Dictionary<string, double> Factors) : IReorgExceptionStep
{
    public string Describe()
    {
        return "by department";
    }

    public double Adjust(ReorgException item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record FlaggedReorgExceptionAdjuster(Registry Flags, string Flag, double Percent) : IReorgExceptionStep
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(ReorgException item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the reorg exception steps in order and keeps a trace of each.</summary>
public sealed class ReorgExceptionEngine(List<IReorgExceptionStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IReorgExceptionStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static ReorgExceptionEngine DefaultReorgExceptionEngine(Registry flags)
    {
        return new ReorgExceptionEngine(
            new List<IReorgExceptionStep>
            {
                new MinimumReorgExceptionAdjuster(45.9),
                new DepartmentReorgExceptionAdjuster(
                    new Dictionary<string, double> { ["north"] = 1.37, ["east"] = 1.22, ["west"] = 1.27 }),
                new FlaggedReorgExceptionAdjuster(flags, "reorg-exception-lenient", 2.6),
            },
            new List<string>(),
            0.5,
            8817.0);
    }

    public double Evaluate(ReorgException item)
    {
        var current = item.EffectiveValue();
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

    public Dictionary<string, double> EvaluateAll(List<ReorgException> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
