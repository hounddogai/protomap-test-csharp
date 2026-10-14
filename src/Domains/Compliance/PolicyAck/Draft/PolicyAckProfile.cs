namespace Acme.Workforce.Compliance.PolicyAck.Draft;

using Acme.Workforce.Kit.Text;

/// <summary>Where a policy ack profile is in its life cycle.</summary>
public enum PolicyAckProfileStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>The record of a policy ack profile and its current status.</summary>
public sealed class PolicyAckProfile(string code, string title, double ceiling, string description, bool pinned, PolicyAckProfileStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public double Ceiling { get; set; } = ceiling;
    public string Description { get; set; } = description;
    public bool Pinned { get; set; } = pinned;
    public PolicyAckProfileStatus Status { get; set; } = status;

    /// <summary>Starts a policy ack profile in status requested.</summary>
    public static PolicyAckProfile NewPolicyAckProfile(string code, string title)
    {
        return new PolicyAckProfile(code, title, 0.0, "", false, PolicyAckProfileStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PolicyAckProfileStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PolicyAckProfileStatus next)
    {
        switch (Status)
        {
            case PolicyAckProfileStatus.Requested:
                switch (next)
                {
                    case PolicyAckProfileStatus.Revoked:
                    case PolicyAckProfileStatus.Expired:
                        return true;
                }
                return false;
            case PolicyAckProfileStatus.Granted:
                switch (next)
                {
                    case PolicyAckProfileStatus.Revoked:
                    case PolicyAckProfileStatus.Expired:
                        return true;
                }
                return false;
            case PolicyAckProfileStatus.Revoked:
                switch (next)
                {
                    case PolicyAckProfileStatus.Expired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(PolicyAckProfileStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"policy ack profile {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"policy ack profile"} {Code}";
    }

    public double MeasuredValue()
    {
        return Ceiling;
    }

    public PolicyAckProfile Copy()
    {
        return new PolicyAckProfile(Code, Title, Ceiling, Description, Pinned, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
