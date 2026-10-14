namespace Acme.Workforce.Assets.LicenseSeat.RecordItem;

using Acme.Workforce.Kit.Text;

/// <summary>Where a adaptive license seat audit is in its life cycle.</summary>
public enum AdaptiveLicenseSeatAuditStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>One adaptive license seat audit, with what the planners need to act on it.</summary>
public sealed class AdaptiveLicenseSeatAudit(string code, string label, string segment, string contactEmail, double rating, int tenureMonths, double cost, string employeeId, AdaptiveLicenseSeatAuditStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Segment { get; set; } = segment;
    public string ContactEmail { get; set; } = contactEmail;
    public double Rating { get; set; } = rating;
    public int TenureMonths { get; set; } = tenureMonths;
    public double Cost { get; set; } = cost;
    public string EmployeeId { get; set; } = employeeId;
    public AdaptiveLicenseSeatAuditStatus Status { get; set; } = status;

    /// <summary>Starts a adaptive license seat audit in status open.</summary>
    public static AdaptiveLicenseSeatAudit NewAdaptiveLicenseSeatAudit(string code, string label)
    {
        return new AdaptiveLicenseSeatAudit(
            code,
            label,
            "",
            "",
            0.0,
            0,
            0.0,
            "",
            AdaptiveLicenseSeatAuditStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case AdaptiveLicenseSeatAuditStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(AdaptiveLicenseSeatAuditStatus next)
    {
        switch (Status)
        {
            case AdaptiveLicenseSeatAuditStatus.Open:
                return next == AdaptiveLicenseSeatAuditStatus.InReview || next == AdaptiveLicenseSeatAuditStatus.Reopened;
            case AdaptiveLicenseSeatAuditStatus.InReview:
                switch (next)
                {
                    case AdaptiveLicenseSeatAuditStatus.Resolved:
                    case AdaptiveLicenseSeatAuditStatus.Archived:
                        return true;
                }
                return false;
            case AdaptiveLicenseSeatAuditStatus.Resolved:
                return next == AdaptiveLicenseSeatAuditStatus.Reopened || next == AdaptiveLicenseSeatAuditStatus.Archived;
            case AdaptiveLicenseSeatAuditStatus.Reopened:
                return next == AdaptiveLicenseSeatAuditStatus.Archived;
        }
        return false;
    }

    public void MoveTo(AdaptiveLicenseSeatAuditStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"adaptive license seat audit {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code}: {Label}";
    }

    public double EffectiveExposure()
    {
        return Math.Round((double)TenureMonths + Rating, 2);
    }

    public AdaptiveLicenseSeatAudit Copy()
    {
        return new AdaptiveLicenseSeatAudit(
            Code,
            Label,
            Segment,
            ContactEmail,
            Rating,
            TenureMonths,
            Cost,
            EmployeeId,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Segment);
    }
}
