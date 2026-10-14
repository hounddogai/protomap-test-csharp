namespace Acme.Workforce.Compliance.PolicyAck.OverrideItem;

using Acme.Workforce.Kit.Text;

/// <summary>Where a policy ack override is in its life cycle.</summary>
public enum PolicyAckOverrideStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>One policy ack override, with what the planners need to act on it.</summary>
public sealed class PolicyAckOverride(string code, string label, string reviewer, string notes, double ceiling, string channel, string skill, PolicyAckOverrideStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Reviewer { get; set; } = reviewer;
    public string Notes { get; set; } = notes;
    public double Ceiling { get; set; } = ceiling;
    public string Channel { get; set; } = channel;
    public string Skill { get; set; } = skill;
    public PolicyAckOverrideStatus Status { get; set; } = status;

    /// <summary>Starts a policy ack override in status draft.</summary>
    public static PolicyAckOverride NewPolicyAckOverride(string code, string label)
    {
        return new PolicyAckOverride(code, label, "", "", 0.0, "", "", PolicyAckOverrideStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PolicyAckOverrideStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PolicyAckOverrideStatus next)
    {
        switch (Status)
        {
            case PolicyAckOverrideStatus.Draft:
                switch (next)
                {
                    case PolicyAckOverrideStatus.Pending:
                        return true;
                }
                return false;
            case PolicyAckOverrideStatus.Pending:
                switch (next)
                {
                    case PolicyAckOverrideStatus.Suspended:
                        return true;
                }
                return false;
            case PolicyAckOverrideStatus.Active:
                switch (next)
                {
                    case PolicyAckOverrideStatus.Suspended:
                    case PolicyAckOverrideStatus.Closed:
                        return true;
                }
                return false;
            case PolicyAckOverrideStatus.Suspended:
                return next == PolicyAckOverrideStatus.Closed;
        }
        return false;
    }

    public void MoveTo(PolicyAckOverrideStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"policy ack override {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code} / {Label}";
    }

    public double MeasuredValue()
    {
        return Ceiling;
    }

    public PolicyAckOverride Copy()
    {
        return new PolicyAckOverride(Code, Label, Reviewer, Notes, Ceiling, Channel, Skill, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Channel);
    }
}
