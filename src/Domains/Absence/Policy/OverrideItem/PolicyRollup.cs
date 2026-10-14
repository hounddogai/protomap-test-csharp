namespace Acme.Workforce.Absence.Policy.OverrideItem;

using Acme.Workforce.Kit.Text;

/// <summary>Where a policy rollup is in its life cycle.</summary>
public enum PolicyRollupStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>The record of a policy rollup and its current status.</summary>
public sealed class PolicyRollup(string code, string title, string phone, string badge, string ownerId, int revision, PolicyRollupStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string Phone { get; set; } = phone;
    public string Badge { get; set; } = badge;
    public string OwnerId { get; set; } = ownerId;
    public int Revision { get; set; } = revision;
    public PolicyRollupStatus Status { get; set; } = status;

    /// <summary>Starts a policy rollup in status idle.</summary>
    public static PolicyRollup NewPolicyRollup(string code, string title)
    {
        return new PolicyRollup(code, title, "", "", "", 0, PolicyRollupStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PolicyRollupStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PolicyRollupStatus next)
    {
        switch (Status)
        {
            case PolicyRollupStatus.Idle:
                switch (next)
                {
                    case PolicyRollupStatus.Ready:
                    case PolicyRollupStatus.Stopped:
                        return true;
                }
                return false;
            case PolicyRollupStatus.Warming:
                switch (next)
                {
                    case PolicyRollupStatus.Ready:
                    case PolicyRollupStatus.Stopped:
                        return true;
                }
                return false;
            case PolicyRollupStatus.Ready:
                return next == PolicyRollupStatus.Draining || next == PolicyRollupStatus.Stopped;
            case PolicyRollupStatus.Draining:
                return next == PolicyRollupStatus.Stopped;
        }
        return false;
    }

    public void MoveTo(PolicyRollupStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"policy rollup {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        Revision += 1;
    }

    public string Label()
    {
        return $"{Code} ({Title})";
    }

    public double MeasuredValue()
    {
        return (double)Revision;
    }

    public PolicyRollup Copy()
    {
        return new PolicyRollup(Code, Title, Phone, Badge, OwnerId, Revision, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
