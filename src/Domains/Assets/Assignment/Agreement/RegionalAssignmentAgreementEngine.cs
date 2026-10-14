namespace Acme.Workforce.Assets.Assignment.Agreement;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the regional assignment agreement calculation.</summary>
public interface IRegionalAssignmentAgreementAdjuster
{
    string Describe();
    double Adjust(RegionalAssignmentAgreement item, double valueValue);
}

public sealed record MinimumRegionalAssignmentAgreementAdjuster(double Minimum) : IRegionalAssignmentAgreementAdjuster
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(RegionalAssignmentAgreement item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record FlaggedRegionalAssignmentAgreementAdjuster(Registry Flags, string Flag, double Percent) : IRegionalAssignmentAgreementAdjuster
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(RegionalAssignmentAgreement item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record RoundingRegionalAssignmentAgreementAdjuster(double Step) : IRegionalAssignmentAgreementAdjuster
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(RegionalAssignmentAgreement item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

/// <summary>Runs the regional assignment agreement steps in order and keeps a trace of each.</summary>
public sealed class RegionalAssignmentAgreementEngine(List<IRegionalAssignmentAgreementAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IRegionalAssignmentAgreementAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static RegionalAssignmentAgreementEngine DefaultRegionalAssignmentAgreementEngine(Registry flags)
    {
        return new RegionalAssignmentAgreementEngine(
            new List<IRegionalAssignmentAgreementAdjuster>
            {
                new MinimumRegionalAssignmentAgreementAdjuster(4.3),
                new FlaggedRegionalAssignmentAgreementAdjuster(
                    flags,
                    "regional-assignment-agreement-scheduled",
                    -6.9),
                new RoundingRegionalAssignmentAgreementAdjuster(0.05),
            },
            new List<string>(),
            4.1,
            65107.3);
    }

    public double Price(RegionalAssignmentAgreement item)
    {
        var current = item.MeasuredValue();
        Trace = new List<string>();
        for (var index = 0; index < Steps.Count; index++)
        {
            var step = Steps[index];
            var before = current;
            current = step.Adjust(item, current);
            if (before != current)
            {
                Trace.Add($"{index + 1}. {step.Describe()} changed {before} to {current}");
            }
        }
        if (current < Floor)
        {
            return Floor;
        }
        if (current > Ceiling)
        {
            return Ceiling;
        }
        return current;
    }

    public string Explain()
    {
        return string.Join("\n", Trace);
    }

    public Dictionary<string, double> EvaluateAll(List<RegionalAssignmentAgreement> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
