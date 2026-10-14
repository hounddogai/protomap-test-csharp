namespace Acme.Workforce.Workplace.Occupancy.Adjustment;

using Acme.Workforce.Kit.Text;

/// <summary>Where a capped occupancy policy is in its life cycle.</summary>
public enum CappedOccupancyPolicyStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>The record of a capped occupancy policy and its current status.</summary>
public sealed class CappedOccupancyPolicy(string code, string name, int attempts, string summary, string channel, string reviewer, CappedOccupancyPolicyStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public int Attempts { get; set; } = attempts;
    public string Summary { get; set; } = summary;
    public string Channel { get; set; } = channel;
    public string Reviewer { get; set; } = reviewer;
    public CappedOccupancyPolicyStatus Status { get; set; } = status;

    /// <summary>Starts a capped occupancy policy in status open.</summary>
    public static CappedOccupancyPolicy NewCappedOccupancyPolicy(string code, string name)
    {
        return new CappedOccupancyPolicy(code, name, 0, "", "", "", CappedOccupancyPolicyStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case CappedOccupancyPolicyStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(CappedOccupancyPolicyStatus next)
    {
        switch (Status)
        {
            case CappedOccupancyPolicyStatus.Open:
                return next == CappedOccupancyPolicyStatus.InReview || next == CappedOccupancyPolicyStatus.Reopened;
            case CappedOccupancyPolicyStatus.InReview:
                switch (next)
                {
                    case CappedOccupancyPolicyStatus.Reopened:
                        return true;
                }
                return false;
            case CappedOccupancyPolicyStatus.Resolved:
                return next == CappedOccupancyPolicyStatus.Reopened || next == CappedOccupancyPolicyStatus.Archived;
            case CappedOccupancyPolicyStatus.Reopened:
                return next == CappedOccupancyPolicyStatus.Archived;
        }
        return false;
    }

    public void MoveTo(CappedOccupancyPolicyStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"capped occupancy policy {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} [{Name}]";
    }

    public double MeasuredValue()
    {
        return (double)Attempts;
    }

    public CappedOccupancyPolicy Copy()
    {
        return new CappedOccupancyPolicy(Code, Name, Attempts, Summary, Channel, Reviewer, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Channel);
    }
}
