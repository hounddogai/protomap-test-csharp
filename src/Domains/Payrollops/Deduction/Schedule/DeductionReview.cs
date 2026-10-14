namespace Acme.Workforce.Payrollops.Deduction.Schedule;

using Acme.Workforce.Kit.Text;

/// <summary>Where a deduction review is in its life cycle.</summary>
public enum DeductionReviewStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>The record of a deduction review and its current status.</summary>
public sealed class DeductionReview(string code, string label, int revision, string department, string notes, DeductionReviewStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public int Revision { get; set; } = revision;
    public string Department { get; set; } = department;
    public string Notes { get; set; } = notes;
    public DeductionReviewStatus Status { get; set; } = status;

    /// <summary>Starts a deduction review in status open.</summary>
    public static DeductionReview NewDeductionReview(string code, string label)
    {
        return new DeductionReview(code, label, 0, "", "", DeductionReviewStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case DeductionReviewStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(DeductionReviewStatus next)
    {
        switch (Status)
        {
            case DeductionReviewStatus.Open:
                switch (next)
                {
                    case DeductionReviewStatus.InReview:
                        return true;
                }
                return false;
            case DeductionReviewStatus.InReview:
                switch (next)
                {
                    case DeductionReviewStatus.Reopened:
                        return true;
                }
                return false;
            case DeductionReviewStatus.Resolved:
                return next == DeductionReviewStatus.Reopened || next == DeductionReviewStatus.Archived;
            case DeductionReviewStatus.Reopened:
                return next == DeductionReviewStatus.Archived;
        }
        return false;
    }

    public void MoveTo(DeductionReviewStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"deduction review {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        Revision += 1;
    }

    public string DisplayLabel()
    {
        return $"{Code} [{Label}]";
    }

    public double MeasuredValue()
    {
        return (double)Revision;
    }

    public DeductionReview Copy()
    {
        return new DeductionReview(Code, Label, Revision, Department, Notes, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Department);
    }
}
