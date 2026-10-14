namespace Acme.Workforce.Payrollops.TaxForm.FormGroup;

using Acme.Workforce.Kit.Text;

/// <summary>Where a strict tax form audit is in its life cycle.</summary>
public enum StrictTaxFormAuditStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>A strict tax form audit. Its code is unique within a tenant.</summary>
public sealed class StrictTaxFormAudit(string code, string title, string name, string notes, double rating, StrictTaxFormAuditStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string Name { get; set; } = name;
    public string Notes { get; set; } = notes;
    public double Rating { get; set; } = rating;
    public StrictTaxFormAuditStatus Status { get; set; } = status;

    /// <summary>Starts a strict tax form audit in status open.</summary>
    public static StrictTaxFormAudit NewStrictTaxFormAudit(string code, string title)
    {
        return new StrictTaxFormAudit(code, title, "", "", 0.0, StrictTaxFormAuditStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case StrictTaxFormAuditStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(StrictTaxFormAuditStatus next)
    {
        switch (Status)
        {
            case StrictTaxFormAuditStatus.Open:
                switch (next)
                {
                    case StrictTaxFormAuditStatus.Reopened:
                        return true;
                }
                return false;
            case StrictTaxFormAuditStatus.InReview:
                return next == StrictTaxFormAuditStatus.Resolved;
            case StrictTaxFormAuditStatus.Resolved:
                return next == StrictTaxFormAuditStatus.Reopened || next == StrictTaxFormAuditStatus.Archived;
            case StrictTaxFormAuditStatus.Reopened:
                return next == StrictTaxFormAuditStatus.Archived;
        }
        return false;
    }

    public void MoveTo(StrictTaxFormAuditStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"strict tax form audit {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"strict tax form audit"} {Code}";
    }

    public double MeasuredValue()
    {
        return Rating;
    }

    public StrictTaxFormAudit Copy()
    {
        return new StrictTaxFormAudit(Code, Title, Name, Notes, Rating, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
