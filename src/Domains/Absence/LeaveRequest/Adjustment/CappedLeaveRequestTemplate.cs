namespace Acme.Workforce.Absence.LeaveRequest.Adjustment;

using Acme.Workforce.Kit.Text;

/// <summary>Where a capped leave request template is in its life cycle.</summary>
public enum CappedLeaveRequestTemplateStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>The record of a capped leave request template and its current status.</summary>
public sealed class CappedLeaveRequestTemplate(string code, string label, string department, string description, string timezone, double minimum, CappedLeaveRequestTemplateStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Department { get; set; } = department;
    public string Description { get; set; } = description;
    public string Timezone { get; set; } = timezone;
    public double Minimum { get; set; } = minimum;
    public CappedLeaveRequestTemplateStatus Status { get; set; } = status;

    /// <summary>Starts a capped leave request template in status new.</summary>
    public static CappedLeaveRequestTemplate NewCappedLeaveRequestTemplate(string code, string label)
    {
        return new CappedLeaveRequestTemplate(code, label, "", "", "", 0.0, CappedLeaveRequestTemplateStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case CappedLeaveRequestTemplateStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(CappedLeaveRequestTemplateStatus next)
    {
        switch (Status)
        {
            case CappedLeaveRequestTemplateStatus.New:
                return next == CappedLeaveRequestTemplateStatus.Failed;
            case CappedLeaveRequestTemplateStatus.Queued:
                switch (next)
                {
                    case CappedLeaveRequestTemplateStatus.Running:
                        return true;
                }
                return false;
            case CappedLeaveRequestTemplateStatus.Running:
                return next == CappedLeaveRequestTemplateStatus.Done || next == CappedLeaveRequestTemplateStatus.Failed;
            case CappedLeaveRequestTemplateStatus.Done:
                return next == CappedLeaveRequestTemplateStatus.Failed;
        }
        return false;
    }

    public void MoveTo(CappedLeaveRequestTemplateStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"capped leave request template {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code}: {Label}";
    }

    public double MeasuredValue()
    {
        return Minimum;
    }

    public CappedLeaveRequestTemplate Copy()
    {
        return new CappedLeaveRequestTemplate(Code, Label, Department, Description, Timezone, Minimum, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Department);
    }
}
