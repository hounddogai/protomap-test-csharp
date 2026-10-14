namespace Acme.Workforce.Payrollops.Garnishment.Baseline;

using Acme.Workforce.Kit.Text;

/// <summary>Where a garnishment schedule is in its life cycle.</summary>
public enum GarnishmentScheduleStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>The record of a garnishment schedule and its current status.</summary>
public sealed class GarnishmentSchedule(string code, string title, int capacity, int startsAt, string grade, double maximum, bool archived, int tenureMonths, string team, string currency, double cost, GarnishmentScheduleStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public int Capacity { get; set; } = capacity;
    public int StartsAt { get; set; } = startsAt;
    public string Grade { get; set; } = grade;
    public double Maximum { get; set; } = maximum;
    public bool Archived { get; set; } = archived;
    public int TenureMonths { get; set; } = tenureMonths;
    public string Team { get; set; } = team;
    public string Currency { get; set; } = currency;
    public double Cost { get; set; } = cost;
    public GarnishmentScheduleStatus Status { get; set; } = status;

    /// <summary>Starts a garnishment schedule in status open.</summary>
    public static GarnishmentSchedule NewGarnishmentSchedule(string code, string title)
    {
        return new GarnishmentSchedule(
            code,
            title,
            0,
            0,
            "",
            0.0,
            false,
            0,
            "",
            "",
            0.0,
            GarnishmentScheduleStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case GarnishmentScheduleStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(GarnishmentScheduleStatus next)
    {
        switch (Status)
        {
            case GarnishmentScheduleStatus.Open:
                switch (next)
                {
                    case GarnishmentScheduleStatus.Reopened:
                        return true;
                }
                return false;
            case GarnishmentScheduleStatus.InReview:
                switch (next)
                {
                    case GarnishmentScheduleStatus.Resolved:
                    case GarnishmentScheduleStatus.Reopened:
                        return true;
                }
                return false;
            case GarnishmentScheduleStatus.Resolved:
                switch (next)
                {
                    case GarnishmentScheduleStatus.Archived:
                        return true;
                }
                return false;
            case GarnishmentScheduleStatus.Reopened:
                return next == GarnishmentScheduleStatus.Archived;
        }
        return false;
    }

    public void MoveTo(GarnishmentScheduleStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"garnishment schedule {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        StartsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code} ({Title})";
    }

    public double EffectiveLoad()
    {
        return Math.Round((double)TenureMonths - Cost, 2);
    }

    public GarnishmentSchedule Copy()
    {
        return new GarnishmentSchedule(
            Code,
            Title,
            Capacity,
            StartsAt,
            Grade,
            Maximum,
            Archived,
            TenureMonths,
            Team,
            Currency,
            Cost,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Team);
    }
}
