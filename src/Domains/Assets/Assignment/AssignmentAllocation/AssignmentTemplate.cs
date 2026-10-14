namespace Acme.Workforce.Assets.Assignment.AssignmentAllocation;

using Acme.Workforce.Kit.Text;

/// <summary>Where a assignment template is in its life cycle.</summary>
public enum AssignmentTemplateStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>A assignment template as the assignment team keeps it.</summary>
public sealed class AssignmentTemplate(string code, string name, string label, string description, double rate, double threshold, double budget, string summary, string managerId, bool visible, AssignmentTemplateStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string Label { get; set; } = label;
    public string Description { get; set; } = description;
    public double Rate { get; set; } = rate;
    public double Threshold { get; set; } = threshold;
    public double Budget { get; set; } = budget;
    public string Summary { get; set; } = summary;
    public string ManagerId { get; set; } = managerId;
    public bool Visible { get; set; } = visible;
    public AssignmentTemplateStatus Status { get; set; } = status;

    /// <summary>Starts a assignment template in status planned.</summary>
    public static AssignmentTemplate NewAssignmentTemplate(string code, string name)
    {
        return new AssignmentTemplate(
            code,
            name,
            "",
            "",
            0.0,
            0.0,
            0.0,
            "",
            "",
            false,
            AssignmentTemplateStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case AssignmentTemplateStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(AssignmentTemplateStatus next)
    {
        switch (Status)
        {
            case AssignmentTemplateStatus.Planned:
                switch (next)
                {
                    case AssignmentTemplateStatus.Cancelled:
                        return true;
                }
                return false;
            case AssignmentTemplateStatus.Scheduled:
                switch (next)
                {
                    case AssignmentTemplateStatus.InProgress:
                    case AssignmentTemplateStatus.Cancelled:
                        return true;
                }
                return false;
            case AssignmentTemplateStatus.InProgress:
                switch (next)
                {
                    case AssignmentTemplateStatus.Completed:
                    case AssignmentTemplateStatus.Cancelled:
                        return true;
                }
                return false;
            case AssignmentTemplateStatus.Completed:
                return next == AssignmentTemplateStatus.Cancelled;
        }
        return false;
    }

    public void MoveTo(AssignmentTemplateStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"assignment template {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code}: {Name}";
    }

    public double AdjustedExposure()
    {
        return Math.Round(Threshold + Budget, 2);
    }

    public AssignmentTemplate Copy()
    {
        return new AssignmentTemplate(
            Code,
            Name,
            Label,
            Description,
            Rate,
            Threshold,
            Budget,
            Summary,
            ManagerId,
            Visible,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
