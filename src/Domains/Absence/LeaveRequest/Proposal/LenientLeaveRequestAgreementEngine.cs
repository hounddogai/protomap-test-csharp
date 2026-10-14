namespace Acme.Workforce.Absence.LeaveRequest.Proposal;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the lenient leave request agreement calculation.</summary>
public interface ILenientLeaveRequestAgreementStep
{
    string Describe();
    double Adjust(LenientLeaveRequestAgreement item, double valueValue);
}

public sealed record RoundingLenientLeaveRequestAgreementAdjuster(double Step) : ILenientLeaveRequestAgreementStep
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(LenientLeaveRequestAgreement item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record LenientLeaveRequestAgreementAmountOffsetAdjuster(double Weight) : ILenientLeaveRequestAgreementStep
{
    public string Describe()
    {
        return $"plus amount times {Weight}";
    }

    public double Adjust(LenientLeaveRequestAgreement item, double valueValue)
    {
        return valueValue + item.Amount * Weight;
    }
}

public sealed record PercentLenientLeaveRequestAgreementAdjuster(double Percent) : ILenientLeaveRequestAgreementStep
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(LenientLeaveRequestAgreement item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record TieredLenientLeaveRequestAgreementAdjuster(List<double> Bounds, List<double> Rates) : ILenientLeaveRequestAgreementStep
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(LenientLeaveRequestAgreement item, double valueValue)
    {
        for (var index = 0; index < Bounds.Count; index++)
        {
            if (valueValue <= Bounds[index] && index < Rates.Count)
            {
                return valueValue * Rates[index];
            }
        }
        return valueValue;
    }
}

public sealed record FlaggedLenientLeaveRequestAgreementAdjuster(Registry Flags, string Flag, double Percent) : ILenientLeaveRequestAgreementStep
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(LenientLeaveRequestAgreement item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the lenient leave request agreement steps in order and keeps a trace of each.</summary>
public sealed class LenientLeaveRequestAgreementEngine(List<ILenientLeaveRequestAgreementStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<ILenientLeaveRequestAgreementStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static LenientLeaveRequestAgreementEngine DefaultLenientLeaveRequestAgreementEngine(Registry flags)
    {
        return new LenientLeaveRequestAgreementEngine(
            new List<ILenientLeaveRequestAgreementStep>
            {
                new RoundingLenientLeaveRequestAgreementAdjuster(0.5),
                new LenientLeaveRequestAgreementAmountOffsetAdjuster(0.16),
                new PercentLenientLeaveRequestAgreementAdjuster(-14.1),
                new TieredLenientLeaveRequestAgreementAdjuster(
                    new List<double> { 2502.0, 5414.0, 7198.0, 9580.0 },
                    new List<double> { 1.09, 1.18, 0.81, 1.16 }),
                new FlaggedLenientLeaveRequestAgreementAdjuster(
                    flags,
                    "lenient-leave-request-agreement-batched",
                    -9.5),
            },
            new List<string>(),
            3.9,
            27975.3);
    }

    public double Price(LenientLeaveRequestAgreement item)
    {
        var current = item.WeightedExposure();
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

    public Dictionary<string, double> EvaluateAll(List<LenientLeaveRequestAgreement> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
