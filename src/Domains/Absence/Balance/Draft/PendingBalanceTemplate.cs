namespace Acme.Workforce.Absence.Balance.Draft;

using Acme.Workforce.Kit.Text;

/// <summary>Where a pending balance template is in its life cycle.</summary>
public enum PendingBalanceTemplateStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>A pending balance template. Its code is unique within a tenant.</summary>
public sealed class PendingBalanceTemplate(string code, string label, string reviewer, int leaveDays, int updatedAt, string location, string notes, double margin, string unit, string salaryBand, PendingBalanceTemplateStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Reviewer { get; set; } = reviewer;
    public int LeaveDays { get; set; } = leaveDays;
    public int UpdatedAt { get; set; } = updatedAt;
    public string Location { get; set; } = location;
    public string Notes { get; set; } = notes;
    public double Margin { get; set; } = margin;
    public string Unit { get; set; } = unit;
    public string SalaryBand { get; set; } = salaryBand;
    public PendingBalanceTemplateStatus Status { get; set; } = status;

    /// <summary>Starts a pending balance template in status proposed.</summary>
    public static PendingBalanceTemplate NewPendingBalanceTemplate(string code, string label)
    {
        return new PendingBalanceTemplate(
            code,
            label,
            "",
            0,
            0,
            "",
            "",
            0.0,
            "",
            "",
            PendingBalanceTemplateStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PendingBalanceTemplateStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PendingBalanceTemplateStatus next)
    {
        switch (Status)
        {
            case PendingBalanceTemplateStatus.Proposed:
                return next == PendingBalanceTemplateStatus.Rejected;
            case PendingBalanceTemplateStatus.Approved:
                return next == PendingBalanceTemplateStatus.Rejected || next == PendingBalanceTemplateStatus.Withdrawn;
            case PendingBalanceTemplateStatus.Rejected:
                switch (next)
                {
                    case PendingBalanceTemplateStatus.Withdrawn:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(PendingBalanceTemplateStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"pending balance template {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        UpdatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{"pending balance template"} {Code}";
    }

    public double AdjustedValue()
    {
        return Math.Round((double)LeaveDays * Margin, 2);
    }

    public PendingBalanceTemplate Copy()
    {
        return new PendingBalanceTemplate(
            Code,
            Label,
            Reviewer,
            LeaveDays,
            UpdatedAt,
            Location,
            Notes,
            Margin,
            Unit,
            SalaryBand,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Unit);
    }
}
