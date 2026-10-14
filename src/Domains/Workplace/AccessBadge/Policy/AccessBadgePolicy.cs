namespace Acme.Workforce.Workplace.AccessBadge.Policy;

using Acme.Workforce.Kit.Text;

/// <summary>Where a access badge policy is in its life cycle.</summary>
public enum AccessBadgePolicyStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>A access badge policy. Its code is unique within a tenant.</summary>
public sealed class AccessBadgePolicy(string code, string title, int expiresAt, string phone, string description, string segment, AccessBadgePolicyStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public int ExpiresAt { get; set; } = expiresAt;
    public string Phone { get; set; } = phone;
    public string Description { get; set; } = description;
    public string Segment { get; set; } = segment;
    public AccessBadgePolicyStatus Status { get; set; } = status;

    /// <summary>Starts a access badge policy in status new.</summary>
    public static AccessBadgePolicy NewAccessBadgePolicy(string code, string title)
    {
        return new AccessBadgePolicy(code, title, 0, "", "", "", AccessBadgePolicyStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case AccessBadgePolicyStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(AccessBadgePolicyStatus next)
    {
        switch (Status)
        {
            case AccessBadgePolicyStatus.New:
                return next == AccessBadgePolicyStatus.Running || next == AccessBadgePolicyStatus.Failed;
            case AccessBadgePolicyStatus.Queued:
                return next == AccessBadgePolicyStatus.Running || next == AccessBadgePolicyStatus.Failed;
            case AccessBadgePolicyStatus.Running:
                switch (next)
                {
                    case AccessBadgePolicyStatus.Done:
                        return true;
                }
                return false;
            case AccessBadgePolicyStatus.Done:
                return next == AccessBadgePolicyStatus.Failed;
        }
        return false;
    }

    public void MoveTo(AccessBadgePolicyStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"access badge policy {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        ExpiresAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code}: {Title}";
    }

    public double MeasuredValue()
    {
        return (double)ExpiresAt;
    }

    public AccessBadgePolicy Copy()
    {
        return new AccessBadgePolicy(Code, Title, ExpiresAt, Phone, Description, Segment, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Segment);
    }
}
