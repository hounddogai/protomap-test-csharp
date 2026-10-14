namespace Acme.Workforce.Workplace.AccessBadge.Budget;

using Acme.Workforce.Kit.Text;

/// <summary>Where a access badge snapshot is in its life cycle.</summary>
public enum AccessBadgeSnapshotStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>One access badge snapshot, with what the planners need to act on it.</summary>
public sealed class AccessBadgeSnapshot(string code, string label, int capacity, string ownerId, string reviewer, int revision, double cost, int updatedAt, double discount, AccessBadgeSnapshotStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public int Capacity { get; set; } = capacity;
    public string OwnerId { get; set; } = ownerId;
    public string Reviewer { get; set; } = reviewer;
    public int Revision { get; set; } = revision;
    public double Cost { get; set; } = cost;
    public int UpdatedAt { get; set; } = updatedAt;
    public double Discount { get; set; } = discount;
    public AccessBadgeSnapshotStatus Status { get; set; } = status;

    /// <summary>Starts a access badge snapshot in status open.</summary>
    public static AccessBadgeSnapshot NewAccessBadgeSnapshot(string code, string label)
    {
        return new AccessBadgeSnapshot(code, label, 0, "", "", 0, 0.0, 0, 0.0, AccessBadgeSnapshotStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case AccessBadgeSnapshotStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(AccessBadgeSnapshotStatus next)
    {
        switch (Status)
        {
            case AccessBadgeSnapshotStatus.Open:
                switch (next)
                {
                    case AccessBadgeSnapshotStatus.Reopened:
                    case AccessBadgeSnapshotStatus.Archived:
                        return true;
                }
                return false;
            case AccessBadgeSnapshotStatus.InReview:
                return next == AccessBadgeSnapshotStatus.Resolved || next == AccessBadgeSnapshotStatus.Archived;
            case AccessBadgeSnapshotStatus.Resolved:
                return next == AccessBadgeSnapshotStatus.Archived;
            case AccessBadgeSnapshotStatus.Reopened:
                return next == AccessBadgeSnapshotStatus.Archived;
        }
        return false;
    }

    public void MoveTo(AccessBadgeSnapshotStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"access badge snapshot {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        UpdatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        Revision += 1;
    }

    public string DisplayLabel()
    {
        return $"{Code} [{Label}]";
    }

    public double EffectiveValue()
    {
        return Math.Round(Cost + (double)Revision, 2);
    }

    public AccessBadgeSnapshot Copy()
    {
        return new AccessBadgeSnapshot(
            Code,
            Label,
            Capacity,
            OwnerId,
            Reviewer,
            Revision,
            Cost,
            UpdatedAt,
            Discount,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
