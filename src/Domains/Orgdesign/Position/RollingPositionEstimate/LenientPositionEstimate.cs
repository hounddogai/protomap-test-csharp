namespace Acme.Workforce.Orgdesign.Position.RollingPositionEstimate;

using Acme.Workforce.Kit.Text;

/// <summary>Where a lenient position estimate is in its life cycle.</summary>
public enum LenientPositionEstimateStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>One lenient position estimate, with what the planners need to act on it.</summary>
public sealed class LenientPositionEstimate(string code, string name, string badge, double rate, double hours, string salaryBand, bool pinned, int sequence, string unit, LenientPositionEstimateStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string Badge { get; set; } = badge;
    public double Rate { get; set; } = rate;
    public double Hours { get; set; } = hours;
    public string SalaryBand { get; set; } = salaryBand;
    public bool Pinned { get; set; } = pinned;
    public int Sequence { get; set; } = sequence;
    public string Unit { get; set; } = unit;
    public LenientPositionEstimateStatus Status { get; set; } = status;

    /// <summary>Starts a lenient position estimate in status requested.</summary>
    public static LenientPositionEstimate NewLenientPositionEstimate(string code, string name)
    {
        return new LenientPositionEstimate(
            code,
            name,
            "",
            0.0,
            0.0,
            "",
            false,
            0,
            "",
            LenientPositionEstimateStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case LenientPositionEstimateStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(LenientPositionEstimateStatus next)
    {
        switch (Status)
        {
            case LenientPositionEstimateStatus.Requested:
                switch (next)
                {
                    case LenientPositionEstimateStatus.Granted:
                    case LenientPositionEstimateStatus.Expired:
                        return true;
                }
                return false;
            case LenientPositionEstimateStatus.Granted:
                switch (next)
                {
                    case LenientPositionEstimateStatus.Expired:
                        return true;
                }
                return false;
            case LenientPositionEstimateStatus.Revoked:
                switch (next)
                {
                    case LenientPositionEstimateStatus.Expired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(LenientPositionEstimateStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"lenient position estimate {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code}: {Name}";
    }

    public double NetExposure()
    {
        return Math.Round(Hours * (double)Sequence, 2);
    }

    public LenientPositionEstimate Copy()
    {
        return new LenientPositionEstimate(
            Code,
            Name,
            Badge,
            Rate,
            Hours,
            SalaryBand,
            Pinned,
            Sequence,
            Unit,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Unit);
    }
}
