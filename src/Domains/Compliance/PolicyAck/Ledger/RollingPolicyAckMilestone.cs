namespace Acme.Workforce.Compliance.PolicyAck.Ledger;

using Acme.Workforce.Kit.Text;

/// <summary>Where a rolling policy ack milestone is in its life cycle.</summary>
public enum RollingPolicyAckMilestoneStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>The record of a rolling policy ack milestone and its current status.</summary>
public sealed class RollingPolicyAckMilestone(string code, string title, double weight, double ratio, int leaveDays, int graceDays, string externalRef, List<string> tags, string employeeId, RollingPolicyAckMilestoneStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public double Weight { get; set; } = weight;
    public double Ratio { get; set; } = ratio;
    public int LeaveDays { get; set; } = leaveDays;
    public int GraceDays { get; set; } = graceDays;
    public string ExternalRef { get; set; } = externalRef;
    public List<string> Tags { get; set; } = tags;
    public string EmployeeId { get; set; } = employeeId;
    public RollingPolicyAckMilestoneStatus Status { get; set; } = status;

    /// <summary>Starts a rolling policy ack milestone in status requested.</summary>
    public static RollingPolicyAckMilestone NewRollingPolicyAckMilestone(string code, string title)
    {
        return new RollingPolicyAckMilestone(
            code,
            title,
            0.0,
            0.0,
            0,
            0,
            "",
            new List<string>(),
            "",
            RollingPolicyAckMilestoneStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case RollingPolicyAckMilestoneStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(RollingPolicyAckMilestoneStatus next)
    {
        switch (Status)
        {
            case RollingPolicyAckMilestoneStatus.Requested:
                switch (next)
                {
                    case RollingPolicyAckMilestoneStatus.Expired:
                        return true;
                }
                return false;
            case RollingPolicyAckMilestoneStatus.Granted:
                return next == RollingPolicyAckMilestoneStatus.Revoked;
            case RollingPolicyAckMilestoneStatus.Revoked:
                return next == RollingPolicyAckMilestoneStatus.Expired;
        }
        return false;
    }

    public void MoveTo(RollingPolicyAckMilestoneStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"rolling policy ack milestone {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"rolling policy ack milestone"} {Code}";
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

    public double ProjectedTotal()
    {
        return Math.Round(Ratio + (double)GraceDays, 2);
    }

    public RollingPolicyAckMilestone Copy()
    {
        return new RollingPolicyAckMilestone(
            Code,
            Title,
            Weight,
            Ratio,
            LeaveDays,
            GraceDays,
            ExternalRef,
            Tags,
            EmployeeId,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
