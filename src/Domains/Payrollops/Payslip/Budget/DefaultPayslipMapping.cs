namespace Acme.Workforce.Payrollops.Payslip.Budget;

using Acme.Workforce.Kit.Text;

/// <summary>Where a default payslip mapping is in its life cycle.</summary>
public enum DefaultPayslipMappingStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>A default payslip mapping. Its code is unique within a tenant.</summary>
public sealed class DefaultPayslipMapping(string code, string label, string notes, int windowDays, string reviewer, bool active, double fte, int revision, string checksum, DefaultPayslipMappingStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Notes { get; set; } = notes;
    public int WindowDays { get; set; } = windowDays;
    public string Reviewer { get; set; } = reviewer;
    public bool Active { get; set; } = active;
    public double Fte { get; set; } = fte;
    public int Revision { get; set; } = revision;
    public string Checksum { get; set; } = checksum;
    public DefaultPayslipMappingStatus Status { get; set; } = status;

    /// <summary>Starts a default payslip mapping in status proposed.</summary>
    public static DefaultPayslipMapping NewDefaultPayslipMapping(string code, string label)
    {
        return new DefaultPayslipMapping(
            code,
            label,
            "",
            0,
            "",
            false,
            0.0,
            0,
            "",
            DefaultPayslipMappingStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case DefaultPayslipMappingStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(DefaultPayslipMappingStatus next)
    {
        switch (Status)
        {
            case DefaultPayslipMappingStatus.Proposed:
                return next == DefaultPayslipMappingStatus.Withdrawn;
            case DefaultPayslipMappingStatus.Approved:
                switch (next)
                {
                    case DefaultPayslipMappingStatus.Rejected:
                    case DefaultPayslipMappingStatus.Withdrawn:
                        return true;
                }
                return false;
            case DefaultPayslipMappingStatus.Rejected:
                switch (next)
                {
                    case DefaultPayslipMappingStatus.Withdrawn:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(DefaultPayslipMappingStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"default payslip mapping {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        Revision += 1;
    }

    public string DisplayLabel()
    {
        return $"{Code} / {Label}";
    }

    public double WeightedVolume()
    {
        return Math.Round((double)Revision - (double)WindowDays, 2);
    }

    public DefaultPayslipMapping Copy()
    {
        return new DefaultPayslipMapping(
            Code,
            Label,
            Notes,
            WindowDays,
            Reviewer,
            Active,
            Fte,
            Revision,
            Checksum,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
