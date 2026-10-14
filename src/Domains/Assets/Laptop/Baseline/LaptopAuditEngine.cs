namespace Acme.Workforce.Assets.Laptop.Baseline;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the laptop audit calculation.</summary>
public interface ILaptopAuditStep
{
    string Describe();
    double Adjust(LaptopAudit item, double valueValue);
}

public sealed record NameLaptopAuditAdjuster(Dictionary<string, double> Factors) : ILaptopAuditStep
{
    public string Describe()
    {
        return "by name";
    }

    public double Adjust(LaptopAudit item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record MinimumLaptopAuditAdjuster(double Minimum) : ILaptopAuditStep
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(LaptopAudit item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record CapLaptopAuditAdjuster(double Cap) : ILaptopAuditStep
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(LaptopAudit item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record FlaggedLaptopAuditAdjuster(Registry Flags, string Flag, double Percent) : ILaptopAuditStep
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(LaptopAudit item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record RoundingLaptopAuditAdjuster(double Step) : ILaptopAuditStep
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(LaptopAudit item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

/// <summary>Runs the laptop audit steps in order and keeps a trace of each.</summary>
public sealed class LaptopAuditEngine(List<ILaptopAuditStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<ILaptopAuditStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static LaptopAuditEngine DefaultLaptopAuditEngine(Registry flags)
    {
        return new LaptopAuditEngine(
            new List<ILaptopAuditStep>
            {
                new NameLaptopAuditAdjuster(new Dictionary<string, double> { ["online"] = 0.97, ["west"] = 1.15 }),
                new MinimumLaptopAuditAdjuster(34.5),
                new CapLaptopAuditAdjuster(9650.0),
                new FlaggedLaptopAuditAdjuster(flags, "laptop-audit-regional", -0.2),
                new RoundingLaptopAuditAdjuster(5.0),
            },
            new List<string>(),
            4.4,
            56923.9);
    }

    public double Evaluate(LaptopAudit item)
    {
        var current = item.EffectiveLoad();
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

    public Dictionary<string, double> EvaluateAll(List<LaptopAudit> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
