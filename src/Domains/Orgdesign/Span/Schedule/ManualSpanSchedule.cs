namespace Acme.Workforce.Orgdesign.Span.Schedule;

using Acme.Workforce.Kit.Text;

/// <summary>Where a manual span schedule is in its life cycle.</summary>
public enum ManualSpanScheduleStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>A manual span schedule as the span team keeps it.</summary>
public sealed class ManualSpanSchedule(string code, string title, string reviewer, string team, double hours, string source, double threshold, ManualSpanScheduleStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string Reviewer { get; set; } = reviewer;
    public string Team { get; set; } = team;
    public double Hours { get; set; } = hours;
    public string Source { get; set; } = source;
    public double Threshold { get; set; } = threshold;
    public ManualSpanScheduleStatus Status { get; set; } = status;

    /// <summary>Starts a manual span schedule in status proposed.</summary>
    public static ManualSpanSchedule NewManualSpanSchedule(string code, string title)
    {
        return new ManualSpanSchedule(code, title, "", "", 0.0, "", 0.0, ManualSpanScheduleStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case ManualSpanScheduleStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(ManualSpanScheduleStatus next)
    {
        switch (Status)
        {
            case ManualSpanScheduleStatus.Proposed:
                switch (next)
                {
                    case ManualSpanScheduleStatus.Approved:
                        return true;
                }
                return false;
            case ManualSpanScheduleStatus.Approved:
                switch (next)
                {
                    case ManualSpanScheduleStatus.Rejected:
                        return true;
                }
                return false;
            case ManualSpanScheduleStatus.Rejected:
                return next == ManualSpanScheduleStatus.Withdrawn;
        }
        return false;
    }

    public void MoveTo(ManualSpanScheduleStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"manual span schedule {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} / {Title}";
    }

    public double EffectiveTotal()
    {
        return Math.Round(Hours + Threshold, 2);
    }

    public ManualSpanSchedule Copy()
    {
        return new ManualSpanSchedule(Code, Title, Reviewer, Team, Hours, Source, Threshold, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Team);
    }
}
