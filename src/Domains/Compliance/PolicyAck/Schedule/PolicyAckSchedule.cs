namespace Acme.Workforce.Compliance.PolicyAck.Schedule;

using Acme.Workforce.Kit.Text;

/// <summary>Where a policy ack schedule is in its life cycle.</summary>
public enum PolicyAckScheduleStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>A policy ack schedule as the policy ack team keeps it.</summary>
public sealed class PolicyAckSchedule(string code, string title, string segment, List<string> tags, int revision, int graceDays, double budget, int startsAt, int leaveDays, PolicyAckScheduleStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string Segment { get; set; } = segment;
    public List<string> Tags { get; set; } = tags;
    public int Revision { get; set; } = revision;
    public int GraceDays { get; set; } = graceDays;
    public double Budget { get; set; } = budget;
    public int StartsAt { get; set; } = startsAt;
    public int LeaveDays { get; set; } = leaveDays;
    public PolicyAckScheduleStatus Status { get; set; } = status;

    /// <summary>Starts a policy ack schedule in status proposed.</summary>
    public static PolicyAckSchedule NewPolicyAckSchedule(string code, string title)
    {
        return new PolicyAckSchedule(
            code,
            title,
            "",
            new List<string>(),
            0,
            0,
            0.0,
            0,
            0,
            PolicyAckScheduleStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PolicyAckScheduleStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PolicyAckScheduleStatus next)
    {
        switch (Status)
        {
            case PolicyAckScheduleStatus.Proposed:
                switch (next)
                {
                    case PolicyAckScheduleStatus.Approved:
                    case PolicyAckScheduleStatus.Withdrawn:
                        return true;
                }
                return false;
            case PolicyAckScheduleStatus.Approved:
                switch (next)
                {
                    case PolicyAckScheduleStatus.Rejected:
                    case PolicyAckScheduleStatus.Withdrawn:
                        return true;
                }
                return false;
            case PolicyAckScheduleStatus.Rejected:
                return next == PolicyAckScheduleStatus.Withdrawn;
        }
        return false;
    }

    public void MoveTo(PolicyAckScheduleStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"policy ack schedule {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        StartsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        Revision += 1;
    }

    public string Label()
    {
        return $"{Code} [{Title}]";
    }

    public bool HasTag(string tag)
    {
        foreach (var candidate in Tags)
        {
            if (Text.Normalize(candidate) == Text.Normalize(tag))
            {
                return true;
            }
        }
        return false;
    }

    public double AdjustedVolume()
    {
        return Math.Round((double)LeaveDays + (double)StartsAt, 2);
    }

    public PolicyAckSchedule Copy()
    {
        return new PolicyAckSchedule(
            Code,
            Title,
            Segment,
            Tags,
            Revision,
            GraceDays,
            Budget,
            StartsAt,
            LeaveDays,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Segment);
    }
}
