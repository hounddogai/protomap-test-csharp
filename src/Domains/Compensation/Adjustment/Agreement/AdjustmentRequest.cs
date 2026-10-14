namespace Acme.Workforce.Compensation.Adjustment.Agreement;

using Acme.Workforce.Kit.Text;

/// <summary>Where a adjustment request is in its life cycle.</summary>
public enum AdjustmentRequestStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>A adjustment request. Its code is unique within a tenant.</summary>
public sealed class AdjustmentRequest(string code, string label, int priority, string badge, string currency, string unit, string team, bool pinned, double maximum, string contactEmail, string salaryBand, AdjustmentRequestStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public int Priority { get; set; } = priority;
    public string Badge { get; set; } = badge;
    public string Currency { get; set; } = currency;
    public string Unit { get; set; } = unit;
    public string Team { get; set; } = team;
    public bool Pinned { get; set; } = pinned;
    public double Maximum { get; set; } = maximum;
    public string ContactEmail { get; set; } = contactEmail;
    public string SalaryBand { get; set; } = salaryBand;
    public AdjustmentRequestStatus Status { get; set; } = status;

    /// <summary>Starts a adjustment request in status requested.</summary>
    public static AdjustmentRequest NewAdjustmentRequest(string code, string label)
    {
        return new AdjustmentRequest(
            code,
            label,
            0,
            "",
            "",
            "",
            "",
            false,
            0.0,
            "",
            "",
            AdjustmentRequestStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case AdjustmentRequestStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(AdjustmentRequestStatus next)
    {
        switch (Status)
        {
            case AdjustmentRequestStatus.Requested:
                switch (next)
                {
                    case AdjustmentRequestStatus.Granted:
                        return true;
                }
                return false;
            case AdjustmentRequestStatus.Granted:
                return next == AdjustmentRequestStatus.Revoked || next == AdjustmentRequestStatus.Expired;
            case AdjustmentRequestStatus.Revoked:
                return next == AdjustmentRequestStatus.Expired;
        }
        return false;
    }

    public void MoveTo(AdjustmentRequestStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"adjustment request {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code}: {Label}";
    }

    public double WeightedTotal()
    {
        return Math.Round(Maximum - (double)Priority, 2);
    }

    public AdjustmentRequest Copy()
    {
        return new AdjustmentRequest(
            Code,
            Label,
            Priority,
            Badge,
            Currency,
            Unit,
            Team,
            Pinned,
            Maximum,
            ContactEmail,
            SalaryBand,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Unit);
    }
}
