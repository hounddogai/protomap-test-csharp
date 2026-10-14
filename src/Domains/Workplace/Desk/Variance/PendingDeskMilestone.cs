namespace Acme.Workforce.Workplace.Desk.Variance;

using Acme.Workforce.Kit.Text;

/// <summary>Where a pending desk milestone is in its life cycle.</summary>
public enum PendingDeskMilestoneStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>A pending desk milestone as the desk team keeps it.</summary>
public sealed class PendingDeskMilestone(string code, string name, string phone, int limit, string summary, bool archived, string segment, PendingDeskMilestoneStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string Phone { get; set; } = phone;
    public int Limit { get; set; } = limit;
    public string Summary { get; set; } = summary;
    public bool Archived { get; set; } = archived;
    public string Segment { get; set; } = segment;
    public PendingDeskMilestoneStatus Status { get; set; } = status;

    /// <summary>Starts a pending desk milestone in status draft.</summary>
    public static PendingDeskMilestone NewPendingDeskMilestone(string code, string name)
    {
        return new PendingDeskMilestone(code, name, "", 0, "", false, "", PendingDeskMilestoneStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PendingDeskMilestoneStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PendingDeskMilestoneStatus next)
    {
        switch (Status)
        {
            case PendingDeskMilestoneStatus.Draft:
                return next == PendingDeskMilestoneStatus.Active || next == PendingDeskMilestoneStatus.Suspended;
            case PendingDeskMilestoneStatus.Pending:
                return next == PendingDeskMilestoneStatus.Active;
            case PendingDeskMilestoneStatus.Active:
                switch (next)
                {
                    case PendingDeskMilestoneStatus.Suspended:
                    case PendingDeskMilestoneStatus.Closed:
                        return true;
                }
                return false;
            case PendingDeskMilestoneStatus.Suspended:
                switch (next)
                {
                    case PendingDeskMilestoneStatus.Closed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(PendingDeskMilestoneStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"pending desk milestone {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"pending desk milestone"} {Code}";
    }

    public double MeasuredValue()
    {
        return (double)Limit;
    }

    public PendingDeskMilestone Copy()
    {
        return new PendingDeskMilestone(Code, Name, Phone, Limit, Summary, Archived, Segment, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Segment);
    }
}
