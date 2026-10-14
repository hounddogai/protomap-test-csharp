namespace Acme.Workforce.Compliance.PolicyAck.WeightedAckGroup;

using Acme.Workforce.Kit.Text;

/// <summary>Where a legacy policy ack group is in its life cycle.</summary>
public enum LegacyPolicyAckGroupStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>One legacy policy ack group, with what the planners need to act on it.</summary>
public sealed class LegacyPolicyAckGroup(string code, string label, string channel, string category, int limit, string country, int updatedAt, string skill, int priority, string phone, LegacyPolicyAckGroupStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Channel { get; set; } = channel;
    public string Category { get; set; } = category;
    public int Limit { get; set; } = limit;
    public string Country { get; set; } = country;
    public int UpdatedAt { get; set; } = updatedAt;
    public string Skill { get; set; } = skill;
    public int Priority { get; set; } = priority;
    public string Phone { get; set; } = phone;
    public LegacyPolicyAckGroupStatus Status { get; set; } = status;

    /// <summary>Starts a legacy policy ack group in status planned.</summary>
    public static LegacyPolicyAckGroup NewLegacyPolicyAckGroup(string code, string label)
    {
        return new LegacyPolicyAckGroup(
            code,
            label,
            "",
            "",
            0,
            "",
            0,
            "",
            0,
            "",
            LegacyPolicyAckGroupStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case LegacyPolicyAckGroupStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(LegacyPolicyAckGroupStatus next)
    {
        switch (Status)
        {
            case LegacyPolicyAckGroupStatus.Planned:
                switch (next)
                {
                    case LegacyPolicyAckGroupStatus.Completed:
                        return true;
                }
                return false;
            case LegacyPolicyAckGroupStatus.Scheduled:
                return next == LegacyPolicyAckGroupStatus.InProgress || next == LegacyPolicyAckGroupStatus.Completed;
            case LegacyPolicyAckGroupStatus.InProgress:
                return next == LegacyPolicyAckGroupStatus.Cancelled;
            case LegacyPolicyAckGroupStatus.Completed:
                switch (next)
                {
                    case LegacyPolicyAckGroupStatus.Cancelled:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(LegacyPolicyAckGroupStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"legacy policy ack group {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        UpdatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{Code} [{Label}]";
    }

    public double EffectiveExposure()
    {
        return Math.Round((double)Priority + (double)UpdatedAt, 2);
    }

    public LegacyPolicyAckGroup Copy()
    {
        return new LegacyPolicyAckGroup(
            Code,
            Label,
            Channel,
            Category,
            Limit,
            Country,
            UpdatedAt,
            Skill,
            Priority,
            Phone,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Channel);
    }
}
