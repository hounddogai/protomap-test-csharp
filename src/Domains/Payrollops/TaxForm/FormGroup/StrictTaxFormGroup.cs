namespace Acme.Workforce.Payrollops.TaxForm.FormGroup;

using Acme.Workforce.Kit.Text;

/// <summary>Where a strict tax form group is in its life cycle.</summary>
public enum StrictTaxFormGroupStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>A strict tax form group. Its code is unique within a tenant.</summary>
public sealed class StrictTaxFormGroup(string code, string label, string source, int retries, string unit, string notes, double fte, StrictTaxFormGroupStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Source { get; set; } = source;
    public int Retries { get; set; } = retries;
    public string Unit { get; set; } = unit;
    public string Notes { get; set; } = notes;
    public double Fte { get; set; } = fte;
    public StrictTaxFormGroupStatus Status { get; set; } = status;

    /// <summary>Starts a strict tax form group in status open.</summary>
    public static StrictTaxFormGroup NewStrictTaxFormGroup(string code, string label)
    {
        return new StrictTaxFormGroup(code, label, "", 0, "", "", 0.0, StrictTaxFormGroupStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case StrictTaxFormGroupStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(StrictTaxFormGroupStatus next)
    {
        switch (Status)
        {
            case StrictTaxFormGroupStatus.Open:
                return next == StrictTaxFormGroupStatus.Reopened;
            case StrictTaxFormGroupStatus.InReview:
                return next == StrictTaxFormGroupStatus.Archived;
            case StrictTaxFormGroupStatus.Resolved:
                switch (next)
                {
                    case StrictTaxFormGroupStatus.Reopened:
                        return true;
                }
                return false;
            case StrictTaxFormGroupStatus.Reopened:
                switch (next)
                {
                    case StrictTaxFormGroupStatus.Archived:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(StrictTaxFormGroupStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"strict tax form group {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code}: {Label}";
    }

    public double WeightedTotal()
    {
        return Math.Round(Fte * (double)Retries, 2);
    }

    public StrictTaxFormGroup Copy()
    {
        return new StrictTaxFormGroup(Code, Label, Source, Retries, Unit, Notes, Fte, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Source);
    }
}
