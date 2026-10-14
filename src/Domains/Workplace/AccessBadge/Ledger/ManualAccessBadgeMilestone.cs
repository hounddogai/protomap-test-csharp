namespace Acme.Workforce.Workplace.AccessBadge.Ledger;

using Acme.Workforce.Kit.Text;

/// <summary>Where a manual access badge milestone is in its life cycle.</summary>
public enum ManualAccessBadgeMilestoneStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>One manual access badge milestone, with what the planners need to act on it.</summary>
public sealed class ManualAccessBadgeMilestone(string code, string title, int leaveDays, double margin, int priority, string summary, ManualAccessBadgeMilestoneStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public int LeaveDays { get; set; } = leaveDays;
    public double Margin { get; set; } = margin;
    public int Priority { get; set; } = priority;
    public string Summary { get; set; } = summary;
    public ManualAccessBadgeMilestoneStatus Status { get; set; } = status;

    /// <summary>Starts a manual access badge milestone in status open.</summary>
    public static ManualAccessBadgeMilestone NewManualAccessBadgeMilestone(string code, string title)
    {
        return new ManualAccessBadgeMilestone(code, title, 0, 0.0, 0, "", ManualAccessBadgeMilestoneStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case ManualAccessBadgeMilestoneStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(ManualAccessBadgeMilestoneStatus next)
    {
        switch (Status)
        {
            case ManualAccessBadgeMilestoneStatus.Open:
                switch (next)
                {
                    case ManualAccessBadgeMilestoneStatus.Archived:
                        return true;
                }
                return false;
            case ManualAccessBadgeMilestoneStatus.InReview:
                switch (next)
                {
                    case ManualAccessBadgeMilestoneStatus.Reopened:
                    case ManualAccessBadgeMilestoneStatus.Archived:
                        return true;
                }
                return false;
            case ManualAccessBadgeMilestoneStatus.Resolved:
                switch (next)
                {
                    case ManualAccessBadgeMilestoneStatus.Reopened:
                    case ManualAccessBadgeMilestoneStatus.Archived:
                        return true;
                }
                return false;
            case ManualAccessBadgeMilestoneStatus.Reopened:
                return next == ManualAccessBadgeMilestoneStatus.Archived;
        }
        return false;
    }

    public void MoveTo(ManualAccessBadgeMilestoneStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"manual access badge milestone {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} ({Title})";
    }

    public double WeightedLoad()
    {
        return Math.Round((double)LeaveDays - (double)Priority, 2);
    }

    public ManualAccessBadgeMilestone Copy()
    {
        return new ManualAccessBadgeMilestone(Code, Title, LeaveDays, Margin, Priority, Summary, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
