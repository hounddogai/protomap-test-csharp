namespace Acme.Workforce.Compensation.Adjustment.Schedule;

using Acme.Workforce.Kit.Text;

/// <summary>Where a adjustment draft is in its life cycle.</summary>
public enum AdjustmentDraftStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>A adjustment draft. Its code is unique within a tenant.</summary>
public sealed class AdjustmentDraft(string code, string title, string notes, string department, double cost, string grade, string managerId, string description, AdjustmentDraftStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string Notes { get; set; } = notes;
    public string Department { get; set; } = department;
    public double Cost { get; set; } = cost;
    public string Grade { get; set; } = grade;
    public string ManagerId { get; set; } = managerId;
    public string Description { get; set; } = description;
    public AdjustmentDraftStatus Status { get; set; } = status;

    /// <summary>Starts a adjustment draft in status proposed.</summary>
    public static AdjustmentDraft NewAdjustmentDraft(string code, string title)
    {
        return new AdjustmentDraft(code, title, "", "", 0.0, "", "", "", AdjustmentDraftStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case AdjustmentDraftStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(AdjustmentDraftStatus next)
    {
        switch (Status)
        {
            case AdjustmentDraftStatus.Proposed:
                return next == AdjustmentDraftStatus.Withdrawn;
            case AdjustmentDraftStatus.Approved:
                switch (next)
                {
                    case AdjustmentDraftStatus.Rejected:
                    case AdjustmentDraftStatus.Withdrawn:
                        return true;
                }
                return false;
            case AdjustmentDraftStatus.Rejected:
                switch (next)
                {
                    case AdjustmentDraftStatus.Withdrawn:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(AdjustmentDraftStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"adjustment draft {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"adjustment draft"} {Code}";
    }

    public double MeasuredValue()
    {
        return Cost;
    }

    public AdjustmentDraft Copy()
    {
        return new AdjustmentDraft(Code, Title, Notes, Department, Cost, Grade, ManagerId, Description, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Department);
    }
}
