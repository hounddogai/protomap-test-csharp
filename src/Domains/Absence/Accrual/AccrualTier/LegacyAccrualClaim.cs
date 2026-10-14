namespace Acme.Workforce.Absence.Accrual.AccrualTier;

using Acme.Workforce.Kit.Text;

/// <summary>Where a legacy accrual claim is in its life cycle.</summary>
public enum LegacyAccrualClaimStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>One legacy accrual claim, with what the planners need to act on it.</summary>
public sealed class LegacyAccrualClaim(string code, string title, int expiresAt, string description, double threshold, int leaveDays, LegacyAccrualClaimStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public int ExpiresAt { get; set; } = expiresAt;
    public string Description { get; set; } = description;
    public double Threshold { get; set; } = threshold;
    public int LeaveDays { get; set; } = leaveDays;
    public LegacyAccrualClaimStatus Status { get; set; } = status;

    /// <summary>Starts a legacy accrual claim in status planned.</summary>
    public static LegacyAccrualClaim NewLegacyAccrualClaim(string code, string title)
    {
        return new LegacyAccrualClaim(code, title, 0, "", 0.0, 0, LegacyAccrualClaimStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case LegacyAccrualClaimStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(LegacyAccrualClaimStatus next)
    {
        switch (Status)
        {
            case LegacyAccrualClaimStatus.Planned:
                switch (next)
                {
                    case LegacyAccrualClaimStatus.Scheduled:
                        return true;
                }
                return false;
            case LegacyAccrualClaimStatus.Scheduled:
                return next == LegacyAccrualClaimStatus.InProgress || next == LegacyAccrualClaimStatus.Cancelled;
            case LegacyAccrualClaimStatus.InProgress:
                switch (next)
                {
                    case LegacyAccrualClaimStatus.Completed:
                        return true;
                }
                return false;
            case LegacyAccrualClaimStatus.Completed:
                switch (next)
                {
                    case LegacyAccrualClaimStatus.Cancelled:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(LegacyAccrualClaimStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"legacy accrual claim {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        ExpiresAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code} / {Title}";
    }

    public double NetValue()
    {
        return Math.Round(Threshold * (double)ExpiresAt, 2);
    }

    public LegacyAccrualClaim Copy()
    {
        return new LegacyAccrualClaim(Code, Title, ExpiresAt, Description, Threshold, LeaveDays, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
