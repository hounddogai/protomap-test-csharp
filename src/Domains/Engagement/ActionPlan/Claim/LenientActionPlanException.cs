namespace Acme.Workforce.Engagement.ActionPlan.Claim;

using Acme.Workforce.Kit.Text;

/// <summary>Where a lenient action plan exception is in its life cycle.</summary>
public enum LenientActionPlanExceptionStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>One lenient action plan exception, with what the planners need to act on it.</summary>
public sealed class LenientActionPlanException(string code, string label, double rating, string salaryBand, string location, string costCenter, int graceDays, bool archived, LenientActionPlanExceptionStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public double Rating { get; set; } = rating;
    public string SalaryBand { get; set; } = salaryBand;
    public string Location { get; set; } = location;
    public string CostCenter { get; set; } = costCenter;
    public int GraceDays { get; set; } = graceDays;
    public bool Archived { get; set; } = archived;
    public LenientActionPlanExceptionStatus Status { get; set; } = status;

    /// <summary>Starts a lenient action plan exception in status draft.</summary>
    public static LenientActionPlanException NewLenientActionPlanException(string code, string label)
    {
        return new LenientActionPlanException(
            code,
            label,
            0.0,
            "",
            "",
            "",
            0,
            false,
            LenientActionPlanExceptionStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case LenientActionPlanExceptionStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(LenientActionPlanExceptionStatus next)
    {
        switch (Status)
        {
            case LenientActionPlanExceptionStatus.Draft:
                switch (next)
                {
                    case LenientActionPlanExceptionStatus.Active:
                        return true;
                }
                return false;
            case LenientActionPlanExceptionStatus.Pending:
                return next == LenientActionPlanExceptionStatus.Suspended || next == LenientActionPlanExceptionStatus.Closed;
            case LenientActionPlanExceptionStatus.Active:
                switch (next)
                {
                    case LenientActionPlanExceptionStatus.Closed:
                        return true;
                }
                return false;
            case LenientActionPlanExceptionStatus.Suspended:
                return next == LenientActionPlanExceptionStatus.Closed;
        }
        return false;
    }

    public void MoveTo(LenientActionPlanExceptionStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"lenient action plan exception {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code}: {Label}";
    }

    public double AdjustedLoad()
    {
        return Math.Round((double)GraceDays + Rating, 2);
    }

    public LenientActionPlanException Copy()
    {
        return new LenientActionPlanException(
            Code,
            Label,
            Rating,
            SalaryBand,
            Location,
            CostCenter,
            GraceDays,
            Archived,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
