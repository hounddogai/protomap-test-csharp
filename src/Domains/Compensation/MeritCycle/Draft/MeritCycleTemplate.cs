namespace Acme.Workforce.Compensation.MeritCycle.Draft;

using Acme.Workforce.Kit.Text;

/// <summary>Where a merit cycle template is in its life cycle.</summary>
public enum MeritCycleTemplateStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>A merit cycle template as the merit cycle team keeps it.</summary>
public sealed class MeritCycleTemplate(string code, string title, int durationMinutes, string costCenter, double rating, string location, string jobCode, double floorValue, string team, int leaveDays, string badge, MeritCycleTemplateStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public int DurationMinutes { get; set; } = durationMinutes;
    public string CostCenter { get; set; } = costCenter;
    public double Rating { get; set; } = rating;
    public string Location { get; set; } = location;
    public string JobCode { get; set; } = jobCode;
    public double FloorValue { get; set; } = floorValue;
    public string Team { get; set; } = team;
    public int LeaveDays { get; set; } = leaveDays;
    public string Badge { get; set; } = badge;
    public MeritCycleTemplateStatus Status { get; set; } = status;

    /// <summary>Starts a merit cycle template in status proposed.</summary>
    public static MeritCycleTemplate NewMeritCycleTemplate(string code, string title)
    {
        return new MeritCycleTemplate(
            code,
            title,
            0,
            "",
            0.0,
            "",
            "",
            0.0,
            "",
            0,
            "",
            MeritCycleTemplateStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case MeritCycleTemplateStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(MeritCycleTemplateStatus next)
    {
        switch (Status)
        {
            case MeritCycleTemplateStatus.Proposed:
                switch (next)
                {
                    case MeritCycleTemplateStatus.Withdrawn:
                        return true;
                }
                return false;
            case MeritCycleTemplateStatus.Approved:
                switch (next)
                {
                    case MeritCycleTemplateStatus.Withdrawn:
                        return true;
                }
                return false;
            case MeritCycleTemplateStatus.Rejected:
                return next == MeritCycleTemplateStatus.Withdrawn;
        }
        return false;
    }

    public void MoveTo(MeritCycleTemplateStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"merit cycle template {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} ({Title})";
    }

    public double NetExposure()
    {
        return Math.Round(Rating + FloorValue, 2);
    }

    public MeritCycleTemplate Copy()
    {
        return new MeritCycleTemplate(
            Code,
            Title,
            DurationMinutes,
            CostCenter,
            Rating,
            Location,
            JobCode,
            FloorValue,
            Team,
            LeaveDays,
            Badge,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Team);
    }
}
