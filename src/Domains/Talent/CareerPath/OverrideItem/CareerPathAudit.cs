namespace Acme.Workforce.Talent.CareerPath.OverrideItem;

using Acme.Workforce.Kit.Text;

/// <summary>Where a career path audit is in its life cycle.</summary>
public enum CareerPathAuditStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>The record of a career path audit and its current status.</summary>
public sealed class CareerPathAudit(string code, string label, bool pinned, string title, double fte, string source, int sequence, string employeeId, double maximum, CareerPathAuditStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public bool Pinned { get; set; } = pinned;
    public string Title { get; set; } = title;
    public double Fte { get; set; } = fte;
    public string Source { get; set; } = source;
    public int Sequence { get; set; } = sequence;
    public string EmployeeId { get; set; } = employeeId;
    public double Maximum { get; set; } = maximum;
    public CareerPathAuditStatus Status { get; set; } = status;

    /// <summary>Starts a career path audit in status proposed.</summary>
    public static CareerPathAudit NewCareerPathAudit(string code, string label)
    {
        return new CareerPathAudit(code, label, false, "", 0.0, "", 0, "", 0.0, CareerPathAuditStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case CareerPathAuditStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(CareerPathAuditStatus next)
    {
        switch (Status)
        {
            case CareerPathAuditStatus.Proposed:
                return next == CareerPathAuditStatus.Approved || next == CareerPathAuditStatus.Rejected;
            case CareerPathAuditStatus.Approved:
                switch (next)
                {
                    case CareerPathAuditStatus.Withdrawn:
                        return true;
                }
                return false;
            case CareerPathAuditStatus.Rejected:
                return next == CareerPathAuditStatus.Withdrawn;
        }
        return false;
    }

    public void MoveTo(CareerPathAuditStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"career path audit {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{"career path audit"} {Code}";
    }

    public double AdjustedVolume()
    {
        return Math.Round((double)Sequence + Maximum, 2);
    }

    public CareerPathAudit Copy()
    {
        return new CareerPathAudit(Code, Label, Pinned, Title, Fte, Source, Sequence, EmployeeId, Maximum, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Source);
    }
}
