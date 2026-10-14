namespace Acme.Workforce.Absence.LeaveRequest.Reservation;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the primary leave request agreement calculation.</summary>
public interface IPrimaryLeaveRequestAgreementFactor
{
    string Describe();
    double Adjust(PrimaryLeaveRequestAgreement item, double valueValue);
}

public sealed record TieredPrimaryLeaveRequestAgreementAdjuster(List<double> Bounds, List<double> Rates) : IPrimaryLeaveRequestAgreementFactor
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(PrimaryLeaveRequestAgreement item, double valueValue)
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

public sealed record FlaggedPrimaryLeaveRequestAgreementAdjuster(Registry Flags, string Flag, double Percent) : IPrimaryLeaveRequestAgreementFactor
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(PrimaryLeaveRequestAgreement item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record CapPrimaryLeaveRequestAgreementAdjuster(double Cap) : IPrimaryLeaveRequestAgreementFactor
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(PrimaryLeaveRequestAgreement item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record PrimaryLeaveRequestAgreementTenureMonthsOffsetAdjuster(double Weight) : IPrimaryLeaveRequestAgreementFactor
{
    public string Describe()
    {
        return $"plus tenure months times {Weight}";
    }

    public double Adjust(PrimaryLeaveRequestAgreement item, double valueValue)
    {
        return valueValue + (double)item.TenureMonths * Weight;
    }
}

public sealed record MinimumPrimaryLeaveRequestAgreementAdjuster(double Minimum) : IPrimaryLeaveRequestAgreementFactor
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(PrimaryLeaveRequestAgreement item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

/// <summary>Runs the primary leave request agreement steps in order and keeps a trace of each.</summary>
public sealed class PrimaryLeaveRequestAgreementEngine(List<IPrimaryLeaveRequestAgreementFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IPrimaryLeaveRequestAgreementFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static PrimaryLeaveRequestAgreementEngine DefaultPrimaryLeaveRequestAgreementEngine(Registry flags)
    {
        return new PrimaryLeaveRequestAgreementEngine(
            new List<IPrimaryLeaveRequestAgreementFactor>
            {
                new TieredPrimaryLeaveRequestAgreementAdjuster(
                    new List<double> { 1825.0, 3832.0, 7468.0, 9927.0 },
                    new List<double> { 0.89, 1.14, 1.07, 0.95 }),
                new FlaggedPrimaryLeaveRequestAgreementAdjuster(
                    flags,
                    "primary-leave-request-agreement-rolling",
                    -7.6),
                new CapPrimaryLeaveRequestAgreementAdjuster(45338.0),
                new PrimaryLeaveRequestAgreementTenureMonthsOffsetAdjuster(0.31),
                new MinimumPrimaryLeaveRequestAgreementAdjuster(22.2),
            },
            new List<string>(),
            3.0,
            64380.6);
    }

    public double Compute(PrimaryLeaveRequestAgreement item)
    {
        var current = item.AdjustedVolume();
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

    public Dictionary<string, double> EvaluateAll(List<PrimaryLeaveRequestAgreement> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
