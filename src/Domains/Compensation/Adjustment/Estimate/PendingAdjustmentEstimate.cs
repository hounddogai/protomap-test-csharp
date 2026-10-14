namespace Acme.Workforce.Compensation.Adjustment.Estimate;

using Acme.Workforce.Kit.Text;

/// <summary>Where a pending adjustment estimate is in its life cycle.</summary>
public enum PendingAdjustmentEstimateStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>A pending adjustment estimate. Its code is unique within a tenant.</summary>
public sealed class PendingAdjustmentEstimate(string code, string label, string timezone, string contactEmail, string badge, string unit, int limit, int attempts, PendingAdjustmentEstimateStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Timezone { get; set; } = timezone;
    public string ContactEmail { get; set; } = contactEmail;
    public string Badge { get; set; } = badge;
    public string Unit { get; set; } = unit;
    public int Limit { get; set; } = limit;
    public int Attempts { get; set; } = attempts;
    public PendingAdjustmentEstimateStatus Status { get; set; } = status;

    /// <summary>Starts a pending adjustment estimate in status requested.</summary>
    public static PendingAdjustmentEstimate NewPendingAdjustmentEstimate(string code, string label)
    {
        return new PendingAdjustmentEstimate(
            code,
            label,
            "",
            "",
            "",
            "",
            0,
            0,
            PendingAdjustmentEstimateStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PendingAdjustmentEstimateStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PendingAdjustmentEstimateStatus next)
    {
        switch (Status)
        {
            case PendingAdjustmentEstimateStatus.Requested:
                switch (next)
                {
                    case PendingAdjustmentEstimateStatus.Revoked:
                        return true;
                }
                return false;
            case PendingAdjustmentEstimateStatus.Granted:
                return next == PendingAdjustmentEstimateStatus.Revoked;
            case PendingAdjustmentEstimateStatus.Revoked:
                return next == PendingAdjustmentEstimateStatus.Expired;
        }
        return false;
    }

    public void MoveTo(PendingAdjustmentEstimateStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"pending adjustment estimate {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code} / {Label}";
    }

    public double AdjustedValue()
    {
        return Math.Round((double)Limit - (double)Attempts, 2);
    }

    public PendingAdjustmentEstimate Copy()
    {
        return new PendingAdjustmentEstimate(
            Code,
            Label,
            Timezone,
            ContactEmail,
            Badge,
            Unit,
            Limit,
            Attempts,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Unit);
    }
}
