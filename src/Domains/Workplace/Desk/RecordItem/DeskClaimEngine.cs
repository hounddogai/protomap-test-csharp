namespace Acme.Workforce.Workplace.Desk.RecordItem;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the desk claim calculation.</summary>
public interface IDeskClaimModifier
{
    string Describe();
    double Adjust(DeskClaim item, double valueValue);
}

public sealed record RoundingDeskClaimAdjuster(double Step) : IDeskClaimModifier
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(DeskClaim item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record PercentDeskClaimAdjuster(double Percent) : IDeskClaimModifier
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(DeskClaim item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record FlaggedDeskClaimAdjuster(Registry Flags, string Flag, double Percent) : IDeskClaimModifier
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(DeskClaim item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record DepartmentDeskClaimAdjuster(Dictionary<string, double> Factors) : IDeskClaimModifier
{
    public string Describe()
    {
        return "by department";
    }

    public double Adjust(DeskClaim item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

/// <summary>Runs the desk claim steps in order and keeps a trace of each.</summary>
public sealed class DeskClaimEngine(List<IDeskClaimModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IDeskClaimModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static DeskClaimEngine DefaultDeskClaimEngine(Registry flags)
    {
        return new DeskClaimEngine(
            new List<IDeskClaimModifier>
            {
                new RoundingDeskClaimAdjuster(0.5),
                new PercentDeskClaimAdjuster(-11.4),
                new FlaggedDeskClaimAdjuster(flags, "desk-claim-capped", -6.8),
                new DepartmentDeskClaimAdjuster(new Dictionary<string, double> { ["south"] = 1.38, ["west"] = 1.32 }),
            },
            new List<string>(),
            4.4,
            88279.8);
    }

    public double Run(DeskClaim item)
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

    public Dictionary<string, double> EvaluateAll(List<DeskClaim> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
