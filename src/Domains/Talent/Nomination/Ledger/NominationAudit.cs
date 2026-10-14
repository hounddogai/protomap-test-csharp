namespace Acme.Workforce.Talent.Nomination.Ledger;

using Acme.Workforce.Kit.Text;

/// <summary>Where a nomination audit is in its life cycle.</summary>
public enum NominationAuditStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>The record of a nomination audit and its current status.</summary>
public sealed class NominationAudit(string code, string name, string notes, double ratio, bool approved, int tenureMonths, double minimum, double margin, string grade, NominationAuditStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string Notes { get; set; } = notes;
    public double Ratio { get; set; } = ratio;
    public bool Approved { get; set; } = approved;
    public int TenureMonths { get; set; } = tenureMonths;
    public double Minimum { get; set; } = minimum;
    public double Margin { get; set; } = margin;
    public string Grade { get; set; } = grade;
    public NominationAuditStatus Status { get; set; } = status;

    /// <summary>Starts a nomination audit in status proposed.</summary>
    public static NominationAudit NewNominationAudit(string code, string name)
    {
        return new NominationAudit(code, name, "", 0.0, false, 0, 0.0, 0.0, "", NominationAuditStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case NominationAuditStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(NominationAuditStatus next)
    {
        switch (Status)
        {
            case NominationAuditStatus.Proposed:
                return next == NominationAuditStatus.Rejected || next == NominationAuditStatus.Withdrawn;
            case NominationAuditStatus.Approved:
                switch (next)
                {
                    case NominationAuditStatus.Withdrawn:
                        return true;
                }
                return false;
            case NominationAuditStatus.Rejected:
                return next == NominationAuditStatus.Withdrawn;
        }
        return false;
    }

    public void MoveTo(NominationAuditStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"nomination audit {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} / {Name}";
    }

    public double WeightedExposure()
    {
        return Math.Round(Ratio - Minimum, 2);
    }

    public NominationAudit Copy()
    {
        return new NominationAudit(Code, Name, Notes, Ratio, Approved, TenureMonths, Minimum, Margin, Grade, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
