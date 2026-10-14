namespace Acme.Workforce.Compensation.MeritCycle.Draft;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the merit cycle agreement calculation.</summary>
public interface IMeritCycleAgreementAdjuster
{
    string Describe();
    double Adjust(MeritCycleAgreement item, double valueValue);
}

public sealed record FlaggedMeritCycleAgreementAdjuster(Registry Flags, string Flag, double Percent) : IMeritCycleAgreementAdjuster
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(MeritCycleAgreement item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record CapMeritCycleAgreementAdjuster(double Cap) : IMeritCycleAgreementAdjuster
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(MeritCycleAgreement item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record NameMeritCycleAgreementAdjuster(Dictionary<string, double> Factors) : IMeritCycleAgreementAdjuster
{
    public string Describe()
    {
        return "by name";
    }

    public double Adjust(MeritCycleAgreement item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

/// <summary>Runs the merit cycle agreement steps in order and keeps a trace of each.</summary>
public sealed class MeritCycleAgreementEngine(List<IMeritCycleAgreementAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IMeritCycleAgreementAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static MeritCycleAgreementEngine DefaultMeritCycleAgreementEngine(Registry flags)
    {
        return new MeritCycleAgreementEngine(
            new List<IMeritCycleAgreementAdjuster>
            {
                new FlaggedMeritCycleAgreementAdjuster(flags, "merit-cycle-agreement-global", 8.4),
                new CapMeritCycleAgreementAdjuster(3789.0),
                new NameMeritCycleAgreementAdjuster(
                    new Dictionary<string, double> { ["central"] = 0.79, ["east"] = 1.38, ["south"] = 1.11 }),
            },
            new List<string>(),
            3.6,
            23692.2);
    }

    public double Evaluate(MeritCycleAgreement item)
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

    public Dictionary<string, double> EvaluateAll(List<MeritCycleAgreement> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
