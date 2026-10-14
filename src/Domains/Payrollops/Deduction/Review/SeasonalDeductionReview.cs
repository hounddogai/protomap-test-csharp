namespace Acme.Workforce.Payrollops.Deduction.Review;

using Acme.Workforce.Kit.Text;

/// <summary>Where a seasonal deduction review is in its life cycle.</summary>
public enum SeasonalDeductionReviewStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>The record of a seasonal deduction review and its current status.</summary>
public sealed class SeasonalDeductionReview(string code, string label, double ceiling, bool visible, string employeeId, string region, SeasonalDeductionReviewStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public double Ceiling { get; set; } = ceiling;
    public bool Visible { get; set; } = visible;
    public string EmployeeId { get; set; } = employeeId;
    public string Region { get; set; } = region;
    public SeasonalDeductionReviewStatus Status { get; set; } = status;

    /// <summary>Starts a seasonal deduction review in status draft.</summary>
    public static SeasonalDeductionReview NewSeasonalDeductionReview(string code, string label)
    {
        return new SeasonalDeductionReview(code, label, 0.0, false, "", "", SeasonalDeductionReviewStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case SeasonalDeductionReviewStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(SeasonalDeductionReviewStatus next)
    {
        switch (Status)
        {
            case SeasonalDeductionReviewStatus.Draft:
                switch (next)
                {
                    case SeasonalDeductionReviewStatus.Pending:
                    case SeasonalDeductionReviewStatus.Closed:
                        return true;
                }
                return false;
            case SeasonalDeductionReviewStatus.Pending:
                switch (next)
                {
                    case SeasonalDeductionReviewStatus.Suspended:
                    case SeasonalDeductionReviewStatus.Closed:
                        return true;
                }
                return false;
            case SeasonalDeductionReviewStatus.Active:
                return next == SeasonalDeductionReviewStatus.Suspended;
            case SeasonalDeductionReviewStatus.Suspended:
                switch (next)
                {
                    case SeasonalDeductionReviewStatus.Closed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(SeasonalDeductionReviewStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"seasonal deduction review {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code} ({Label})";
    }

    public double MeasuredValue()
    {
        return Ceiling;
    }

    public SeasonalDeductionReview Copy()
    {
        return new SeasonalDeductionReview(Code, Label, Ceiling, Visible, EmployeeId, Region, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Region);
    }
}
