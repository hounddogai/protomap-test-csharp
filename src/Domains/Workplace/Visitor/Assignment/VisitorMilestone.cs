namespace Acme.Workforce.Workplace.Visitor.Assignment;

using Acme.Workforce.Kit.Text;

/// <summary>Where a visitor milestone is in its life cycle.</summary>
public enum VisitorMilestoneStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>The record of a visitor milestone and its current status.</summary>
public sealed class VisitorMilestone(string code, string title, double cost, double maximum, string summary, int tenureMonths, string team, string managerId, VisitorMilestoneStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public double Cost { get; set; } = cost;
    public double Maximum { get; set; } = maximum;
    public string Summary { get; set; } = summary;
    public int TenureMonths { get; set; } = tenureMonths;
    public string Team { get; set; } = team;
    public string ManagerId { get; set; } = managerId;
    public VisitorMilestoneStatus Status { get; set; } = status;

    /// <summary>Starts a visitor milestone in status idle.</summary>
    public static VisitorMilestone NewVisitorMilestone(string code, string title)
    {
        return new VisitorMilestone(code, title, 0.0, 0.0, "", 0, "", "", VisitorMilestoneStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case VisitorMilestoneStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(VisitorMilestoneStatus next)
    {
        switch (Status)
        {
            case VisitorMilestoneStatus.Idle:
                switch (next)
                {
                    case VisitorMilestoneStatus.Warming:
                    case VisitorMilestoneStatus.Stopped:
                        return true;
                }
                return false;
            case VisitorMilestoneStatus.Warming:
                switch (next)
                {
                    case VisitorMilestoneStatus.Draining:
                    case VisitorMilestoneStatus.Stopped:
                        return true;
                }
                return false;
            case VisitorMilestoneStatus.Ready:
                return next == VisitorMilestoneStatus.Draining;
            case VisitorMilestoneStatus.Draining:
                switch (next)
                {
                    case VisitorMilestoneStatus.Stopped:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(VisitorMilestoneStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"visitor milestone {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} [{Title}]";
    }

    public double WeightedLoad()
    {
        return Math.Round((double)TenureMonths - Cost, 2);
    }

    public VisitorMilestone Copy()
    {
        return new VisitorMilestone(Code, Title, Cost, Maximum, Summary, TenureMonths, Team, ManagerId, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Team);
    }
}
