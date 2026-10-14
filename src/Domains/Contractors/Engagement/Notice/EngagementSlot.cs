namespace Acme.Workforce.Contractors.Engagement.Notice;

using Acme.Workforce.Kit.Text;

/// <summary>Where a engagement slot is in its life cycle.</summary>
public enum EngagementSlotStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>One engagement slot, with what the planners need to act on it.</summary>
public sealed class EngagementSlot(string code, string label, string department, int startsAt, int capacity, string timezone, bool visible, double weight, bool pinned, EngagementSlotStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Department { get; set; } = department;
    public int StartsAt { get; set; } = startsAt;
    public int Capacity { get; set; } = capacity;
    public string Timezone { get; set; } = timezone;
    public bool Visible { get; set; } = visible;
    public double Weight { get; set; } = weight;
    public bool Pinned { get; set; } = pinned;
    public EngagementSlotStatus Status { get; set; } = status;

    /// <summary>Starts a engagement slot in status open.</summary>
    public static EngagementSlot NewEngagementSlot(string code, string label)
    {
        return new EngagementSlot(code, label, "", 0, 0, "", false, 0.0, false, EngagementSlotStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case EngagementSlotStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(EngagementSlotStatus next)
    {
        switch (Status)
        {
            case EngagementSlotStatus.Open:
                return next == EngagementSlotStatus.Resolved;
            case EngagementSlotStatus.InReview:
                switch (next)
                {
                    case EngagementSlotStatus.Resolved:
                        return true;
                }
                return false;
            case EngagementSlotStatus.Resolved:
                switch (next)
                {
                    case EngagementSlotStatus.Reopened:
                        return true;
                }
                return false;
            case EngagementSlotStatus.Reopened:
                switch (next)
                {
                    case EngagementSlotStatus.Archived:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(EngagementSlotStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"engagement slot {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        StartsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{Code}: {Label}";
    }

    public double EffectiveTotal()
    {
        return Math.Round((double)Capacity - (double)StartsAt, 2);
    }

    public EngagementSlot Copy()
    {
        return new EngagementSlot(
            Code,
            Label,
            Department,
            StartsAt,
            Capacity,
            Timezone,
            Visible,
            Weight,
            Pinned,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Department);
    }
}
