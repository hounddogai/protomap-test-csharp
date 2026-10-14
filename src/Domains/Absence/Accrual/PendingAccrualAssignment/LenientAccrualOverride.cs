namespace Acme.Workforce.Absence.Accrual.PendingAccrualAssignment;

using Acme.Workforce.Kit.Text;

/// <summary>Where a lenient accrual override is in its life cycle.</summary>
public enum LenientAccrualOverrideStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>One lenient accrual override, with what the planners need to act on it.</summary>
public sealed class LenientAccrualOverride(string code, string label, double floorValue, int retries, bool pinned, string ownerId, string region, string jobCode, string contactEmail, int startsAt, LenientAccrualOverrideStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public double FloorValue { get; set; } = floorValue;
    public int Retries { get; set; } = retries;
    public bool Pinned { get; set; } = pinned;
    public string OwnerId { get; set; } = ownerId;
    public string Region { get; set; } = region;
    public string JobCode { get; set; } = jobCode;
    public string ContactEmail { get; set; } = contactEmail;
    public int StartsAt { get; set; } = startsAt;
    public LenientAccrualOverrideStatus Status { get; set; } = status;

    /// <summary>Starts a lenient accrual override in status open.</summary>
    public static LenientAccrualOverride NewLenientAccrualOverride(string code, string label)
    {
        return new LenientAccrualOverride(
            code,
            label,
            0.0,
            0,
            false,
            "",
            "",
            "",
            "",
            0,
            LenientAccrualOverrideStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case LenientAccrualOverrideStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(LenientAccrualOverrideStatus next)
    {
        switch (Status)
        {
            case LenientAccrualOverrideStatus.Open:
                switch (next)
                {
                    case LenientAccrualOverrideStatus.InReview:
                        return true;
                }
                return false;
            case LenientAccrualOverrideStatus.InReview:
                return next == LenientAccrualOverrideStatus.Reopened || next == LenientAccrualOverrideStatus.Archived;
            case LenientAccrualOverrideStatus.Resolved:
                switch (next)
                {
                    case LenientAccrualOverrideStatus.Reopened:
                        return true;
                }
                return false;
            case LenientAccrualOverrideStatus.Reopened:
                switch (next)
                {
                    case LenientAccrualOverrideStatus.Archived:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(LenientAccrualOverrideStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"lenient accrual override {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        StartsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{"lenient accrual override"} {Code}";
    }

    public double ProjectedLoad()
    {
        return Math.Round((double)StartsAt * (double)Retries, 2);
    }

    public LenientAccrualOverride Copy()
    {
        return new LenientAccrualOverride(
            Code,
            Label,
            FloorValue,
            Retries,
            Pinned,
            OwnerId,
            Region,
            JobCode,
            ContactEmail,
            StartsAt,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Region);
    }
}
