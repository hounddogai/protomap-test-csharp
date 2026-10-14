namespace Acme.Workforce.Workplace.Desk.DeskVariance;

using Acme.Workforce.Kit.Text;

/// <summary>Where a desk notice is in its life cycle.</summary>
public enum DeskNoticeStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>A desk notice as the desk team keeps it.</summary>
public sealed class DeskNotice(string code, string title, string timezone, string team, string costCenter, double budget, string reviewer, double rate, double ratio, DeskNoticeStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string Timezone { get; set; } = timezone;
    public string Team { get; set; } = team;
    public string CostCenter { get; set; } = costCenter;
    public double Budget { get; set; } = budget;
    public string Reviewer { get; set; } = reviewer;
    public double Rate { get; set; } = rate;
    public double Ratio { get; set; } = ratio;
    public DeskNoticeStatus Status { get; set; } = status;

    /// <summary>Starts a desk notice in status new.</summary>
    public static DeskNotice NewDeskNotice(string code, string title)
    {
        return new DeskNotice(code, title, "", "", "", 0.0, "", 0.0, 0.0, DeskNoticeStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case DeskNoticeStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(DeskNoticeStatus next)
    {
        switch (Status)
        {
            case DeskNoticeStatus.New:
                switch (next)
                {
                    case DeskNoticeStatus.Done:
                    case DeskNoticeStatus.Failed:
                        return true;
                }
                return false;
            case DeskNoticeStatus.Queued:
                return next == DeskNoticeStatus.Running;
            case DeskNoticeStatus.Running:
                return next == DeskNoticeStatus.Failed;
            case DeskNoticeStatus.Done:
                return next == DeskNoticeStatus.Failed;
        }
        return false;
    }

    public void MoveTo(DeskNoticeStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"desk notice {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"desk notice"} {Code}";
    }

    public double WeightedLoad()
    {
        return Math.Round(Budget * Rate, 2);
    }

    public DeskNotice Copy()
    {
        return new DeskNotice(Code, Title, Timezone, Team, CostCenter, Budget, Reviewer, Rate, Ratio, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Team);
    }
}
