namespace Acme.Workforce.Absence.ReturnPlan.Threshold;

using Acme.Workforce.Kit.Text;

/// <summary>Where a capped return plan review is in its life cycle.</summary>
public enum CappedReturnPlanReviewStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>The record of a capped return plan review and its current status.</summary>
public sealed class CappedReturnPlanReview(string code, string title, double threshold, string employeeId, string currency, string timezone, int limit, string contactEmail, int graceDays, double minimum, int endsAt, CappedReturnPlanReviewStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public double Threshold { get; set; } = threshold;
    public string EmployeeId { get; set; } = employeeId;
    public string Currency { get; set; } = currency;
    public string Timezone { get; set; } = timezone;
    public int Limit { get; set; } = limit;
    public string ContactEmail { get; set; } = contactEmail;
    public int GraceDays { get; set; } = graceDays;
    public double Minimum { get; set; } = minimum;
    public int EndsAt { get; set; } = endsAt;
    public CappedReturnPlanReviewStatus Status { get; set; } = status;

    /// <summary>Starts a capped return plan review in status open.</summary>
    public static CappedReturnPlanReview NewCappedReturnPlanReview(string code, string title)
    {
        return new CappedReturnPlanReview(
            code,
            title,
            0.0,
            "",
            "",
            "",
            0,
            "",
            0,
            0.0,
            0,
            CappedReturnPlanReviewStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case CappedReturnPlanReviewStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(CappedReturnPlanReviewStatus next)
    {
        switch (Status)
        {
            case CappedReturnPlanReviewStatus.Open:
                return next == CappedReturnPlanReviewStatus.InReview || next == CappedReturnPlanReviewStatus.Reopened;
            case CappedReturnPlanReviewStatus.InReview:
                switch (next)
                {
                    case CappedReturnPlanReviewStatus.Archived:
                        return true;
                }
                return false;
            case CappedReturnPlanReviewStatus.Resolved:
                return next == CappedReturnPlanReviewStatus.Reopened;
            case CappedReturnPlanReviewStatus.Reopened:
                switch (next)
                {
                    case CappedReturnPlanReviewStatus.Archived:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(CappedReturnPlanReviewStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"capped return plan review {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        EndsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{"capped return plan review"} {Code}";
    }

    public double EffectiveVolume()
    {
        return Math.Round((double)Limit * Threshold, 2);
    }

    public CappedReturnPlanReview Copy()
    {
        return new CappedReturnPlanReview(
            Code,
            Title,
            Threshold,
            EmployeeId,
            Currency,
            Timezone,
            Limit,
            ContactEmail,
            GraceDays,
            Minimum,
            EndsAt,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
