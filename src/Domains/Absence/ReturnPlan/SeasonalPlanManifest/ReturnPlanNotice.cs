namespace Acme.Workforce.Absence.ReturnPlan.SeasonalPlanManifest;

using Acme.Workforce.Kit.Text;

/// <summary>Where a return plan notice is in its life cycle.</summary>
public enum ReturnPlanNoticeStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>One return plan notice, with what the planners need to act on it.</summary>
public sealed class ReturnPlanNotice(string code, string title, int capacity, string team, double hours, string badge, ReturnPlanNoticeStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public int Capacity { get; set; } = capacity;
    public string Team { get; set; } = team;
    public double Hours { get; set; } = hours;
    public string Badge { get; set; } = badge;
    public ReturnPlanNoticeStatus Status { get; set; } = status;

    /// <summary>Starts a return plan notice in status draft.</summary>
    public static ReturnPlanNotice NewReturnPlanNotice(string code, string title)
    {
        return new ReturnPlanNotice(code, title, 0, "", 0.0, "", ReturnPlanNoticeStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case ReturnPlanNoticeStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(ReturnPlanNoticeStatus next)
    {
        switch (Status)
        {
            case ReturnPlanNoticeStatus.Draft:
                return next == ReturnPlanNoticeStatus.Pending || next == ReturnPlanNoticeStatus.Suspended;
            case ReturnPlanNoticeStatus.Pending:
                switch (next)
                {
                    case ReturnPlanNoticeStatus.Active:
                    case ReturnPlanNoticeStatus.Closed:
                        return true;
                }
                return false;
            case ReturnPlanNoticeStatus.Active:
                return next == ReturnPlanNoticeStatus.Suspended || next == ReturnPlanNoticeStatus.Closed;
            case ReturnPlanNoticeStatus.Suspended:
                return next == ReturnPlanNoticeStatus.Closed;
        }
        return false;
    }

    public void MoveTo(ReturnPlanNoticeStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"return plan notice {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} [{Title}]";
    }

    public double EffectiveLoad()
    {
        return Math.Round(Hours + (double)Capacity, 2);
    }

    public ReturnPlanNotice Copy()
    {
        return new ReturnPlanNotice(Code, Title, Capacity, Team, Hours, Badge, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Team);
    }
}
