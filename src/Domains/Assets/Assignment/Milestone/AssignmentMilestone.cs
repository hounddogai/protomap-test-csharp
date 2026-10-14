namespace Acme.Workforce.Assets.Assignment.Milestone;

using Acme.Workforce.Kit.Text;

/// <summary>Where a assignment milestone is in its life cycle.</summary>
public enum AssignmentMilestoneStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>The record of a assignment milestone and its current status.</summary>
public sealed class AssignmentMilestone(string code, string label, double discount, string location, string channel, string grade, double minimum, string country, string salaryBand, string region, int sequence, AssignmentMilestoneStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public double Discount { get; set; } = discount;
    public string Location { get; set; } = location;
    public string Channel { get; set; } = channel;
    public string Grade { get; set; } = grade;
    public double Minimum { get; set; } = minimum;
    public string Country { get; set; } = country;
    public string SalaryBand { get; set; } = salaryBand;
    public string Region { get; set; } = region;
    public int Sequence { get; set; } = sequence;
    public AssignmentMilestoneStatus Status { get; set; } = status;

    /// <summary>Starts a assignment milestone in status open.</summary>
    public static AssignmentMilestone NewAssignmentMilestone(string code, string label)
    {
        return new AssignmentMilestone(
            code,
            label,
            0.0,
            "",
            "",
            "",
            0.0,
            "",
            "",
            "",
            0,
            AssignmentMilestoneStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case AssignmentMilestoneStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(AssignmentMilestoneStatus next)
    {
        switch (Status)
        {
            case AssignmentMilestoneStatus.Open:
                switch (next)
                {
                    case AssignmentMilestoneStatus.InReview:
                        return true;
                }
                return false;
            case AssignmentMilestoneStatus.InReview:
                switch (next)
                {
                    case AssignmentMilestoneStatus.Resolved:
                    case AssignmentMilestoneStatus.Archived:
                        return true;
                }
                return false;
            case AssignmentMilestoneStatus.Resolved:
                return next == AssignmentMilestoneStatus.Reopened || next == AssignmentMilestoneStatus.Archived;
            case AssignmentMilestoneStatus.Reopened:
                switch (next)
                {
                    case AssignmentMilestoneStatus.Archived:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(AssignmentMilestoneStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"assignment milestone {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code} [{Label}]";
    }

    public double NetValue()
    {
        return Math.Round((double)Sequence * Minimum, 2);
    }

    public AssignmentMilestone Copy()
    {
        return new AssignmentMilestone(
            Code,
            Label,
            Discount,
            Location,
            Channel,
            Grade,
            Minimum,
            Country,
            SalaryBand,
            Region,
            Sequence,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Channel);
    }
}
