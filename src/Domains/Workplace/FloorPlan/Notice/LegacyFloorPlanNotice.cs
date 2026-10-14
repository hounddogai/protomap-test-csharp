namespace Acme.Workforce.Workplace.FloorPlan.Notice;

using Acme.Workforce.Kit.Text;

/// <summary>Where a legacy floor plan notice is in its life cycle.</summary>
public enum LegacyFloorPlanNoticeStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>The record of a legacy floor plan notice and its current status.</summary>
public sealed class LegacyFloorPlanNotice(string code, string title, string summary, string salaryBand, int windowDays, string ownerId, int attempts, LegacyFloorPlanNoticeStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string Summary { get; set; } = summary;
    public string SalaryBand { get; set; } = salaryBand;
    public int WindowDays { get; set; } = windowDays;
    public string OwnerId { get; set; } = ownerId;
    public int Attempts { get; set; } = attempts;
    public LegacyFloorPlanNoticeStatus Status { get; set; } = status;

    /// <summary>Starts a legacy floor plan notice in status requested.</summary>
    public static LegacyFloorPlanNotice NewLegacyFloorPlanNotice(string code, string title)
    {
        return new LegacyFloorPlanNotice(code, title, "", "", 0, "", 0, LegacyFloorPlanNoticeStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case LegacyFloorPlanNoticeStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(LegacyFloorPlanNoticeStatus next)
    {
        switch (Status)
        {
            case LegacyFloorPlanNoticeStatus.Requested:
                return next == LegacyFloorPlanNoticeStatus.Expired;
            case LegacyFloorPlanNoticeStatus.Granted:
                return next == LegacyFloorPlanNoticeStatus.Revoked;
            case LegacyFloorPlanNoticeStatus.Revoked:
                switch (next)
                {
                    case LegacyFloorPlanNoticeStatus.Expired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(LegacyFloorPlanNoticeStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"legacy floor plan notice {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"legacy floor plan notice"} {Code}";
    }

    public double EffectiveExposure()
    {
        return Math.Round((double)Attempts - (double)WindowDays, 2);
    }

    public LegacyFloorPlanNotice Copy()
    {
        return new LegacyFloorPlanNotice(Code, Title, Summary, SalaryBand, WindowDays, OwnerId, Attempts, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
