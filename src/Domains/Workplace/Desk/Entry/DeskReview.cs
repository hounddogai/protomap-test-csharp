namespace Acme.Workforce.Workplace.Desk.Entry;

using Acme.Workforce.Kit.Text;

/// <summary>Where a desk review is in its life cycle.</summary>
public enum DeskReviewStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>A desk review. Its code is unique within a tenant.</summary>
public sealed class DeskReview(string code, string name, List<string> tags, string managerId, string description, string reviewer, string employeeId, string skill, double cost, DeskReviewStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public List<string> Tags { get; set; } = tags;
    public string ManagerId { get; set; } = managerId;
    public string Description { get; set; } = description;
    public string Reviewer { get; set; } = reviewer;
    public string EmployeeId { get; set; } = employeeId;
    public string Skill { get; set; } = skill;
    public double Cost { get; set; } = cost;
    public DeskReviewStatus Status { get; set; } = status;

    /// <summary>Starts a desk review in status planned.</summary>
    public static DeskReview NewDeskReview(string code, string name)
    {
        return new DeskReview(code, name, new List<string>(), "", "", "", "", "", 0.0, DeskReviewStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case DeskReviewStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(DeskReviewStatus next)
    {
        switch (Status)
        {
            case DeskReviewStatus.Planned:
                return next == DeskReviewStatus.InProgress || next == DeskReviewStatus.Cancelled;
            case DeskReviewStatus.Scheduled:
                switch (next)
                {
                    case DeskReviewStatus.Cancelled:
                        return true;
                }
                return false;
            case DeskReviewStatus.InProgress:
                return next == DeskReviewStatus.Cancelled;
            case DeskReviewStatus.Completed:
                return next == DeskReviewStatus.Cancelled;
        }
        return false;
    }

    public void MoveTo(DeskReviewStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"desk review {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"desk review"} {Code}";
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

    public double MeasuredValue()
    {
        return Cost;
    }

    public DeskReview Copy()
    {
        return new DeskReview(Code, Name, Tags, ManagerId, Description, Reviewer, EmployeeId, Skill, Cost, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
