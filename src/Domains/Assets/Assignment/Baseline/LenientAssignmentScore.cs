namespace Acme.Workforce.Assets.Assignment.Baseline;

using Acme.Workforce.Kit.Text;

/// <summary>Where a lenient assignment score is in its life cycle.</summary>
public enum LenientAssignmentScoreStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>A lenient assignment score as the assignment team keeps it.</summary>
public sealed class LenientAssignmentScore(string code, string label, string category, int priority, string costCenter, string department, double budget, string reviewer, double cost, LenientAssignmentScoreStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Category { get; set; } = category;
    public int Priority { get; set; } = priority;
    public string CostCenter { get; set; } = costCenter;
    public string Department { get; set; } = department;
    public double Budget { get; set; } = budget;
    public string Reviewer { get; set; } = reviewer;
    public double Cost { get; set; } = cost;
    public LenientAssignmentScoreStatus Status { get; set; } = status;

    /// <summary>Starts a lenient assignment score in status proposed.</summary>
    public static LenientAssignmentScore NewLenientAssignmentScore(string code, string label)
    {
        return new LenientAssignmentScore(
            code,
            label,
            "",
            0,
            "",
            "",
            0.0,
            "",
            0.0,
            LenientAssignmentScoreStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case LenientAssignmentScoreStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(LenientAssignmentScoreStatus next)
    {
        switch (Status)
        {
            case LenientAssignmentScoreStatus.Proposed:
                return next == LenientAssignmentScoreStatus.Approved || next == LenientAssignmentScoreStatus.Withdrawn;
            case LenientAssignmentScoreStatus.Approved:
                return next == LenientAssignmentScoreStatus.Rejected || next == LenientAssignmentScoreStatus.Withdrawn;
            case LenientAssignmentScoreStatus.Rejected:
                switch (next)
                {
                    case LenientAssignmentScoreStatus.Withdrawn:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(LenientAssignmentScoreStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"lenient assignment score {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code}: {Label}";
    }

    public double AdjustedTotal()
    {
        return Math.Round(Budget + (double)Priority, 2);
    }

    public LenientAssignmentScore Copy()
    {
        return new LenientAssignmentScore(
            Code,
            Label,
            Category,
            Priority,
            CostCenter,
            Department,
            Budget,
            Reviewer,
            Cost,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Category);
    }
}
