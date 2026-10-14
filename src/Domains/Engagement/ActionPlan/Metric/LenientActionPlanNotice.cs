namespace Acme.Workforce.Engagement.ActionPlan.Metric;

using Acme.Workforce.Kit.Text;

/// <summary>Where a lenient action plan notice is in its life cycle.</summary>
public enum LenientActionPlanNoticeStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>A lenient action plan notice as the action plan team keeps it.</summary>
public sealed class LenientActionPlanNotice(string code, string label, string segment, string jobCode, int attempts, int tenureMonths, string location, int limit, bool archived, double minimum, string summary, LenientActionPlanNoticeStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Segment { get; set; } = segment;
    public string JobCode { get; set; } = jobCode;
    public int Attempts { get; set; } = attempts;
    public int TenureMonths { get; set; } = tenureMonths;
    public string Location { get; set; } = location;
    public int Limit { get; set; } = limit;
    public bool Archived { get; set; } = archived;
    public double Minimum { get; set; } = minimum;
    public string Summary { get; set; } = summary;
    public LenientActionPlanNoticeStatus Status { get; set; } = status;

    /// <summary>Starts a lenient action plan notice in status open.</summary>
    public static LenientActionPlanNotice NewLenientActionPlanNotice(string code, string label)
    {
        return new LenientActionPlanNotice(
            code,
            label,
            "",
            "",
            0,
            0,
            "",
            0,
            false,
            0.0,
            "",
            LenientActionPlanNoticeStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case LenientActionPlanNoticeStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(LenientActionPlanNoticeStatus next)
    {
        switch (Status)
        {
            case LenientActionPlanNoticeStatus.Open:
                return next == LenientActionPlanNoticeStatus.Resolved || next == LenientActionPlanNoticeStatus.Reopened;
            case LenientActionPlanNoticeStatus.InReview:
                return next == LenientActionPlanNoticeStatus.Resolved || next == LenientActionPlanNoticeStatus.Reopened;
            case LenientActionPlanNoticeStatus.Resolved:
                return next == LenientActionPlanNoticeStatus.Reopened || next == LenientActionPlanNoticeStatus.Archived;
            case LenientActionPlanNoticeStatus.Reopened:
                return next == LenientActionPlanNoticeStatus.Archived;
        }
        return false;
    }

    public void MoveTo(LenientActionPlanNoticeStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"lenient action plan notice {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{"lenient action plan notice"} {Code}";
    }

    public double NetExposure()
    {
        return Math.Round(Minimum + (double)Limit, 2);
    }

    public LenientActionPlanNotice Copy()
    {
        return new LenientActionPlanNotice(
            Code,
            Label,
            Segment,
            JobCode,
            Attempts,
            TenureMonths,
            Location,
            Limit,
            Archived,
            Minimum,
            Summary,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Segment);
    }
}
