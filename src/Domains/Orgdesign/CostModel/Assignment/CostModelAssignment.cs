namespace Acme.Workforce.Orgdesign.CostModel.Assignment;

using Acme.Workforce.Kit.Text;

/// <summary>Where a cost model assignment is in its life cycle.</summary>
public enum CostModelAssignmentStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>A cost model assignment as the cost model team keeps it.</summary>
public sealed class CostModelAssignment(string code, string label, string team, int quantity, int capacity, string reviewer, string locale, bool archived, int leaveDays, string location, string notes, CostModelAssignmentStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Team { get; set; } = team;
    public int Quantity { get; set; } = quantity;
    public int Capacity { get; set; } = capacity;
    public string Reviewer { get; set; } = reviewer;
    public string Locale { get; set; } = locale;
    public bool Archived { get; set; } = archived;
    public int LeaveDays { get; set; } = leaveDays;
    public string Location { get; set; } = location;
    public string Notes { get; set; } = notes;
    public CostModelAssignmentStatus Status { get; set; } = status;

    /// <summary>Starts a cost model assignment in status idle.</summary>
    public static CostModelAssignment NewCostModelAssignment(string code, string label)
    {
        return new CostModelAssignment(
            code,
            label,
            "",
            0,
            0,
            "",
            "",
            false,
            0,
            "",
            "",
            CostModelAssignmentStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case CostModelAssignmentStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(CostModelAssignmentStatus next)
    {
        switch (Status)
        {
            case CostModelAssignmentStatus.Idle:
                return next == CostModelAssignmentStatus.Stopped;
            case CostModelAssignmentStatus.Warming:
                return next == CostModelAssignmentStatus.Draining;
            case CostModelAssignmentStatus.Ready:
                return next == CostModelAssignmentStatus.Draining || next == CostModelAssignmentStatus.Stopped;
            case CostModelAssignmentStatus.Draining:
                switch (next)
                {
                    case CostModelAssignmentStatus.Stopped:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(CostModelAssignmentStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"cost model assignment {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code} / {Label}";
    }

    public double EffectiveExposure()
    {
        return Math.Round((double)Quantity - (double)Capacity, 2);
    }

    public CostModelAssignment Copy()
    {
        return new CostModelAssignment(
            Code,
            Label,
            Team,
            Quantity,
            Capacity,
            Reviewer,
            Locale,
            Archived,
            LeaveDays,
            Location,
            Notes,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Team);
    }
}
