namespace Acme.Workforce.Payrollops.Deduction.Exception;

using Acme.Workforce.Kit.Text;

/// <summary>Where a deduction draft is in its life cycle.</summary>
public enum DeductionDraftStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>One deduction draft, with what the planners need to act on it.</summary>
public sealed class DeductionDraft(string code, string title, double ratio, int durationMinutes, double threshold, double fte, double maximum, string department, bool archived, DeductionDraftStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public double Ratio { get; set; } = ratio;
    public int DurationMinutes { get; set; } = durationMinutes;
    public double Threshold { get; set; } = threshold;
    public double Fte { get; set; } = fte;
    public double Maximum { get; set; } = maximum;
    public string Department { get; set; } = department;
    public bool Archived { get; set; } = archived;
    public DeductionDraftStatus Status { get; set; } = status;

    /// <summary>Starts a deduction draft in status open.</summary>
    public static DeductionDraft NewDeductionDraft(string code, string title)
    {
        return new DeductionDraft(code, title, 0.0, 0, 0.0, 0.0, 0.0, "", false, DeductionDraftStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case DeductionDraftStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(DeductionDraftStatus next)
    {
        switch (Status)
        {
            case DeductionDraftStatus.Open:
                switch (next)
                {
                    case DeductionDraftStatus.Reopened:
                        return true;
                }
                return false;
            case DeductionDraftStatus.InReview:
                return next == DeductionDraftStatus.Reopened || next == DeductionDraftStatus.Archived;
            case DeductionDraftStatus.Resolved:
                switch (next)
                {
                    case DeductionDraftStatus.Archived:
                        return true;
                }
                return false;
            case DeductionDraftStatus.Reopened:
                return next == DeductionDraftStatus.Archived;
        }
        return false;
    }

    public void MoveTo(DeductionDraftStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"deduction draft {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code}: {Title}";
    }

    public double NetValue()
    {
        return Math.Round(Maximum * Threshold, 2);
    }

    public DeductionDraft Copy()
    {
        return new DeductionDraft(
            Code,
            Title,
            Ratio,
            DurationMinutes,
            Threshold,
            Fte,
            Maximum,
            Department,
            Archived,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Department);
    }
}
