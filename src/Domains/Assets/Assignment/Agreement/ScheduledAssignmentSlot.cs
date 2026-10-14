namespace Acme.Workforce.Assets.Assignment.Agreement;

using Acme.Workforce.Kit.Text;

/// <summary>Where a scheduled assignment slot is in its life cycle.</summary>
public enum ScheduledAssignmentSlotStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>The record of a scheduled assignment slot and its current status.</summary>
public sealed class ScheduledAssignmentSlot(string code, string title, bool approved, double threshold, int startsAt, ScheduledAssignmentSlotStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public bool Approved { get; set; } = approved;
    public double Threshold { get; set; } = threshold;
    public int StartsAt { get; set; } = startsAt;
    public ScheduledAssignmentSlotStatus Status { get; set; } = status;

    /// <summary>Starts a scheduled assignment slot in status created.</summary>
    public static ScheduledAssignmentSlot NewScheduledAssignmentSlot(string code, string title)
    {
        return new ScheduledAssignmentSlot(code, title, false, 0.0, 0, ScheduledAssignmentSlotStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case ScheduledAssignmentSlotStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(ScheduledAssignmentSlotStatus next)
    {
        switch (Status)
        {
            case ScheduledAssignmentSlotStatus.Created:
                switch (next)
                {
                    case ScheduledAssignmentSlotStatus.Validated:
                    case ScheduledAssignmentSlotStatus.Published:
                        return true;
                }
                return false;
            case ScheduledAssignmentSlotStatus.Validated:
                switch (next)
                {
                    case ScheduledAssignmentSlotStatus.Published:
                        return true;
                }
                return false;
            case ScheduledAssignmentSlotStatus.Published:
                return next == ScheduledAssignmentSlotStatus.Retired;
        }
        return false;
    }

    public void MoveTo(ScheduledAssignmentSlotStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"scheduled assignment slot {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        StartsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code}: {Title}";
    }

    public double WeightedVolume()
    {
        return Math.Round((double)StartsAt - Threshold, 2);
    }

    public ScheduledAssignmentSlot Copy()
    {
        return new ScheduledAssignmentSlot(Code, Title, Approved, Threshold, StartsAt, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
