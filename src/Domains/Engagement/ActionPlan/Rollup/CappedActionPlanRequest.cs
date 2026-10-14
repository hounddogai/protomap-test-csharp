namespace Acme.Workforce.Engagement.ActionPlan.Rollup;

using Acme.Workforce.Kit.Text;

/// <summary>Where a capped action plan request is in its life cycle.</summary>
public enum CappedActionPlanRequestStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>One capped action plan request, with what the planners need to act on it.</summary>
public sealed class CappedActionPlanRequest(string code, string title, string salaryBand, double ratio, int durationMinutes, string description, string externalRef, int startsAt, CappedActionPlanRequestStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string SalaryBand { get; set; } = salaryBand;
    public double Ratio { get; set; } = ratio;
    public int DurationMinutes { get; set; } = durationMinutes;
    public string Description { get; set; } = description;
    public string ExternalRef { get; set; } = externalRef;
    public int StartsAt { get; set; } = startsAt;
    public CappedActionPlanRequestStatus Status { get; set; } = status;

    /// <summary>Starts a capped action plan request in status proposed.</summary>
    public static CappedActionPlanRequest NewCappedActionPlanRequest(string code, string title)
    {
        return new CappedActionPlanRequest(
            code,
            title,
            "",
            0.0,
            0,
            "",
            "",
            0,
            CappedActionPlanRequestStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case CappedActionPlanRequestStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(CappedActionPlanRequestStatus next)
    {
        switch (Status)
        {
            case CappedActionPlanRequestStatus.Proposed:
                return next == CappedActionPlanRequestStatus.Withdrawn;
            case CappedActionPlanRequestStatus.Approved:
                switch (next)
                {
                    case CappedActionPlanRequestStatus.Rejected:
                    case CappedActionPlanRequestStatus.Withdrawn:
                        return true;
                }
                return false;
            case CappedActionPlanRequestStatus.Rejected:
                return next == CappedActionPlanRequestStatus.Withdrawn;
        }
        return false;
    }

    public void MoveTo(CappedActionPlanRequestStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"capped action plan request {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        StartsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code}: {Title}";
    }

    public double WeightedVolume()
    {
        return Math.Round((double)DurationMinutes - Ratio, 2);
    }

    public CappedActionPlanRequest Copy()
    {
        return new CappedActionPlanRequest(
            Code,
            Title,
            SalaryBand,
            Ratio,
            DurationMinutes,
            Description,
            ExternalRef,
            StartsAt,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
