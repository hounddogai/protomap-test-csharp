namespace Acme.Workforce.Contractors.Engagement.Score;

using Acme.Workforce.Kit.Text;

/// <summary>Where a engagement digest is in its life cycle.</summary>
public enum EngagementDigestStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>The record of a engagement digest and its current status.</summary>
public sealed class EngagementDigest(string code, string name, bool active, string location, string costCenter, string department, string ownerId, int retries, EngagementDigestStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public bool Active { get; set; } = active;
    public string Location { get; set; } = location;
    public string CostCenter { get; set; } = costCenter;
    public string Department { get; set; } = department;
    public string OwnerId { get; set; } = ownerId;
    public int Retries { get; set; } = retries;
    public EngagementDigestStatus Status { get; set; } = status;

    /// <summary>Starts a engagement digest in status planned.</summary>
    public static EngagementDigest NewEngagementDigest(string code, string name)
    {
        return new EngagementDigest(code, name, false, "", "", "", "", 0, EngagementDigestStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case EngagementDigestStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(EngagementDigestStatus next)
    {
        switch (Status)
        {
            case EngagementDigestStatus.Planned:
                switch (next)
                {
                    case EngagementDigestStatus.InProgress:
                    case EngagementDigestStatus.Completed:
                        return true;
                }
                return false;
            case EngagementDigestStatus.Scheduled:
                switch (next)
                {
                    case EngagementDigestStatus.Completed:
                        return true;
                }
                return false;
            case EngagementDigestStatus.InProgress:
                switch (next)
                {
                    case EngagementDigestStatus.Cancelled:
                        return true;
                }
                return false;
            case EngagementDigestStatus.Completed:
                return next == EngagementDigestStatus.Cancelled;
        }
        return false;
    }

    public void MoveTo(EngagementDigestStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"engagement digest {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"engagement digest"} {Code}";
    }

    public double MeasuredValue()
    {
        return (double)Retries;
    }

    public EngagementDigest Copy()
    {
        return new EngagementDigest(Code, Name, Active, Location, CostCenter, Department, OwnerId, Retries, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Department);
    }
}
