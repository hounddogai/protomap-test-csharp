namespace Acme.Workforce.Compliance.TrainingDue.Assignment;

using Acme.Workforce.Kit.Text;

/// <summary>Where a training due assignment is in its life cycle.</summary>
public enum TrainingDueAssignmentStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>A training due assignment as the training due team keeps it.</summary>
public sealed class TrainingDueAssignment(string code, string title, bool approved, string department, double margin, int retries, string costCenter, bool active, string location, string currency, int revision, TrainingDueAssignmentStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public bool Approved { get; set; } = approved;
    public string Department { get; set; } = department;
    public double Margin { get; set; } = margin;
    public int Retries { get; set; } = retries;
    public string CostCenter { get; set; } = costCenter;
    public bool Active { get; set; } = active;
    public string Location { get; set; } = location;
    public string Currency { get; set; } = currency;
    public int Revision { get; set; } = revision;
    public TrainingDueAssignmentStatus Status { get; set; } = status;

    /// <summary>Starts a training due assignment in status draft.</summary>
    public static TrainingDueAssignment NewTrainingDueAssignment(string code, string title)
    {
        return new TrainingDueAssignment(
            code,
            title,
            false,
            "",
            0.0,
            0,
            "",
            false,
            "",
            "",
            0,
            TrainingDueAssignmentStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case TrainingDueAssignmentStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(TrainingDueAssignmentStatus next)
    {
        switch (Status)
        {
            case TrainingDueAssignmentStatus.Draft:
                return next == TrainingDueAssignmentStatus.Active;
            case TrainingDueAssignmentStatus.Pending:
                return next == TrainingDueAssignmentStatus.Active || next == TrainingDueAssignmentStatus.Closed;
            case TrainingDueAssignmentStatus.Active:
                switch (next)
                {
                    case TrainingDueAssignmentStatus.Closed:
                        return true;
                }
                return false;
            case TrainingDueAssignmentStatus.Suspended:
                return next == TrainingDueAssignmentStatus.Closed;
        }
        return false;
    }

    public void MoveTo(TrainingDueAssignmentStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"training due assignment {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        Revision += 1;
    }

    public string Label()
    {
        return $"{Code}: {Title}";
    }

    public double WeightedExposure()
    {
        return Math.Round(Margin * (double)Retries, 2);
    }

    public TrainingDueAssignment Copy()
    {
        return new TrainingDueAssignment(
            Code,
            Title,
            Approved,
            Department,
            Margin,
            Retries,
            CostCenter,
            Active,
            Location,
            Currency,
            Revision,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Department);
    }
}
