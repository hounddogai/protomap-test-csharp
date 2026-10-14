namespace Acme.Workforce.Orgdesign.Headcount.Agreement;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the headcount agreement calculation.</summary>
public interface IHeadcountAgreementStep
{
    string Describe();
    double Adjust(HeadcountAgreement item, double valueValue);
}

public sealed record MinimumHeadcountAgreementAdjuster(double Minimum) : IHeadcountAgreementStep
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(HeadcountAgreement item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record LabelHeadcountAgreementAdjuster(Dictionary<string, double> Factors) : IHeadcountAgreementStep
{
    public string Describe()
    {
        return "by label";
    }

    public double Adjust(HeadcountAgreement item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record FlaggedHeadcountAgreementAdjuster(Registry Flags, string Flag, double Percent) : IHeadcountAgreementStep
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(HeadcountAgreement item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the headcount agreement steps in order and keeps a trace of each.</summary>
public sealed class HeadcountAgreementEngine(List<IHeadcountAgreementStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IHeadcountAgreementStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static HeadcountAgreementEngine DefaultHeadcountAgreementEngine(Registry flags)
    {
        return new HeadcountAgreementEngine(
            new List<IHeadcountAgreementStep>
            {
                new MinimumHeadcountAgreementAdjuster(31.9),
                new LabelHeadcountAgreementAdjuster(
                    new Dictionary<string, double> { ["south"] = 1.16, ["central"] = 0.81 }),
                new FlaggedHeadcountAgreementAdjuster(flags, "headcount-agreement-legacy", -5.6),
            },
            new List<string>(),
            4.5,
            80725.3);
    }

    public double Compute(HeadcountAgreement item)
    {
        var current = item.NetExposure();
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

    public Dictionary<string, double> EvaluateAll(List<HeadcountAgreement> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
