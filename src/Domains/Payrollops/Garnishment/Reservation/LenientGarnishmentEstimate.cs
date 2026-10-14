namespace Acme.Workforce.Payrollops.Garnishment.Reservation;

using Acme.Workforce.Kit.Text;

/// <summary>Where a lenient garnishment estimate is in its life cycle.</summary>
public enum LenientGarnishmentEstimateStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>The record of a lenient garnishment estimate and its current status.</summary>
public sealed class LenientGarnishmentEstimate(string code, string name, string timezone, int retries, double discount, string source, LenientGarnishmentEstimateStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string Timezone { get; set; } = timezone;
    public int Retries { get; set; } = retries;
    public double Discount { get; set; } = discount;
    public string Source { get; set; } = source;
    public LenientGarnishmentEstimateStatus Status { get; set; } = status;

    /// <summary>Starts a lenient garnishment estimate in status proposed.</summary>
    public static LenientGarnishmentEstimate NewLenientGarnishmentEstimate(string code, string name)
    {
        return new LenientGarnishmentEstimate(code, name, "", 0, 0.0, "", LenientGarnishmentEstimateStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case LenientGarnishmentEstimateStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(LenientGarnishmentEstimateStatus next)
    {
        switch (Status)
        {
            case LenientGarnishmentEstimateStatus.Proposed:
                switch (next)
                {
                    case LenientGarnishmentEstimateStatus.Approved:
                    case LenientGarnishmentEstimateStatus.Withdrawn:
                        return true;
                }
                return false;
            case LenientGarnishmentEstimateStatus.Approved:
                return next == LenientGarnishmentEstimateStatus.Withdrawn;
            case LenientGarnishmentEstimateStatus.Rejected:
                switch (next)
                {
                    case LenientGarnishmentEstimateStatus.Withdrawn:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(LenientGarnishmentEstimateStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"lenient garnishment estimate {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"lenient garnishment estimate"} {Code}";
    }

    public double ProjectedTotal()
    {
        return Math.Round((double)Retries + Discount, 2);
    }

    public LenientGarnishmentEstimate Copy()
    {
        return new LenientGarnishmentEstimate(Code, Name, Timezone, Retries, Discount, Source, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Source);
    }
}
