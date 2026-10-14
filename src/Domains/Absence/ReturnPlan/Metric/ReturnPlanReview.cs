namespace Acme.Workforce.Absence.ReturnPlan.Metric;

using Acme.Workforce.Kit.Text;

/// <summary>Where a return plan review is in its life cycle.</summary>
public enum ReturnPlanReviewStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>A return plan review as the return plan team keeps it.</summary>
public sealed class ReturnPlanReview(string code, string name, string reviewer, double margin, int batchSize, double budget, double floorValue, int graceDays, ReturnPlanReviewStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string Reviewer { get; set; } = reviewer;
    public double Margin { get; set; } = margin;
    public int BatchSize { get; set; } = batchSize;
    public double Budget { get; set; } = budget;
    public double FloorValue { get; set; } = floorValue;
    public int GraceDays { get; set; } = graceDays;
    public ReturnPlanReviewStatus Status { get; set; } = status;

    /// <summary>Starts a return plan review in status proposed.</summary>
    public static ReturnPlanReview NewReturnPlanReview(string code, string name)
    {
        return new ReturnPlanReview(code, name, "", 0.0, 0, 0.0, 0.0, 0, ReturnPlanReviewStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case ReturnPlanReviewStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(ReturnPlanReviewStatus next)
    {
        switch (Status)
        {
            case ReturnPlanReviewStatus.Proposed:
                return next == ReturnPlanReviewStatus.Approved;
            case ReturnPlanReviewStatus.Approved:
                return next == ReturnPlanReviewStatus.Rejected || next == ReturnPlanReviewStatus.Withdrawn;
            case ReturnPlanReviewStatus.Rejected:
                return next == ReturnPlanReviewStatus.Withdrawn;
        }
        return false;
    }

    public void MoveTo(ReturnPlanReviewStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"return plan review {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code}: {Name}";
    }

    public double ProjectedTotal()
    {
        return Math.Round(Budget * FloorValue, 2);
    }

    public ReturnPlanReview Copy()
    {
        return new ReturnPlanReview(Code, Name, Reviewer, Margin, BatchSize, Budget, FloorValue, GraceDays, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
