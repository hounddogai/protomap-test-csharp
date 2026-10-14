namespace Acme.Workforce.Workplace.AccessBadge.Budget;

using Acme.Workforce.Kit.Text;

/// <summary>Where a access badge adjustment is in its life cycle.</summary>
public enum AccessBadgeAdjustmentStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>A access badge adjustment as the access badge team keeps it.</summary>
public sealed class AccessBadgeAdjustment(string code, string title, string country, double score, int expiresAt, AccessBadgeAdjustmentStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string Country { get; set; } = country;
    public double Score { get; set; } = score;
    public int ExpiresAt { get; set; } = expiresAt;
    public AccessBadgeAdjustmentStatus Status { get; set; } = status;

    /// <summary>Starts a access badge adjustment in status idle.</summary>
    public static AccessBadgeAdjustment NewAccessBadgeAdjustment(string code, string title)
    {
        return new AccessBadgeAdjustment(code, title, "", 0.0, 0, AccessBadgeAdjustmentStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case AccessBadgeAdjustmentStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(AccessBadgeAdjustmentStatus next)
    {
        switch (Status)
        {
            case AccessBadgeAdjustmentStatus.Idle:
                switch (next)
                {
                    case AccessBadgeAdjustmentStatus.Warming:
                    case AccessBadgeAdjustmentStatus.Draining:
                        return true;
                }
                return false;
            case AccessBadgeAdjustmentStatus.Warming:
                switch (next)
                {
                    case AccessBadgeAdjustmentStatus.Ready:
                    case AccessBadgeAdjustmentStatus.Stopped:
                        return true;
                }
                return false;
            case AccessBadgeAdjustmentStatus.Ready:
                switch (next)
                {
                    case AccessBadgeAdjustmentStatus.Stopped:
                        return true;
                }
                return false;
            case AccessBadgeAdjustmentStatus.Draining:
                switch (next)
                {
                    case AccessBadgeAdjustmentStatus.Stopped:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(AccessBadgeAdjustmentStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"access badge adjustment {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        ExpiresAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code} ({Title})";
    }

    public double WeightedTotal()
    {
        return Math.Round((double)ExpiresAt * Score, 2);
    }

    public AccessBadgeAdjustment Copy()
    {
        return new AccessBadgeAdjustment(Code, Title, Country, Score, ExpiresAt, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Country);
    }
}
