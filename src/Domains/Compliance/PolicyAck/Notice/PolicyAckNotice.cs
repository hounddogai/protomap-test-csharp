namespace Acme.Workforce.Compliance.PolicyAck.Notice;

using Acme.Workforce.Kit.Text;

/// <summary>Where a policy ack notice is in its life cycle.</summary>
public enum PolicyAckNoticeStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>One policy ack notice, with what the planners need to act on it.</summary>
public sealed class PolicyAckNotice(string code, string title, double minimum, string currency, string team, PolicyAckNoticeStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public double Minimum { get; set; } = minimum;
    public string Currency { get; set; } = currency;
    public string Team { get; set; } = team;
    public PolicyAckNoticeStatus Status { get; set; } = status;

    /// <summary>Starts a policy ack notice in status draft.</summary>
    public static PolicyAckNotice NewPolicyAckNotice(string code, string title)
    {
        return new PolicyAckNotice(code, title, 0.0, "", "", PolicyAckNoticeStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PolicyAckNoticeStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PolicyAckNoticeStatus next)
    {
        switch (Status)
        {
            case PolicyAckNoticeStatus.Draft:
                return next == PolicyAckNoticeStatus.Pending || next == PolicyAckNoticeStatus.Active;
            case PolicyAckNoticeStatus.Pending:
                return next == PolicyAckNoticeStatus.Suspended;
            case PolicyAckNoticeStatus.Active:
                switch (next)
                {
                    case PolicyAckNoticeStatus.Suspended:
                    case PolicyAckNoticeStatus.Closed:
                        return true;
                }
                return false;
            case PolicyAckNoticeStatus.Suspended:
                return next == PolicyAckNoticeStatus.Closed;
        }
        return false;
    }

    public void MoveTo(PolicyAckNoticeStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"policy ack notice {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"policy ack notice"} {Code}";
    }

    public double MeasuredValue()
    {
        return Minimum;
    }

    public PolicyAckNotice Copy()
    {
        return new PolicyAckNotice(Code, Title, Minimum, Currency, Team, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Team);
    }
}
