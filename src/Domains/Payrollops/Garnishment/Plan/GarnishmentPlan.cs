namespace Acme.Workforce.Payrollops.Garnishment.Plan;

using Acme.Workforce.Kit.Text;

/// <summary>Where a garnishment plan is in its life cycle.</summary>
public enum GarnishmentPlanStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>One garnishment plan, with what the planners need to act on it.</summary>
public sealed class GarnishmentPlan(string code, string name, int leaveDays, int durationMinutes, string skill, string summary, int windowDays, string phone, string timezone, GarnishmentPlanStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public int LeaveDays { get; set; } = leaveDays;
    public int DurationMinutes { get; set; } = durationMinutes;
    public string Skill { get; set; } = skill;
    public string Summary { get; set; } = summary;
    public int WindowDays { get; set; } = windowDays;
    public string Phone { get; set; } = phone;
    public string Timezone { get; set; } = timezone;
    public GarnishmentPlanStatus Status { get; set; } = status;

    /// <summary>Starts a garnishment plan in status draft.</summary>
    public static GarnishmentPlan NewGarnishmentPlan(string code, string name)
    {
        return new GarnishmentPlan(code, name, 0, 0, "", "", 0, "", "", GarnishmentPlanStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case GarnishmentPlanStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(GarnishmentPlanStatus next)
    {
        switch (Status)
        {
            case GarnishmentPlanStatus.Draft:
                switch (next)
                {
                    case GarnishmentPlanStatus.Active:
                    case GarnishmentPlanStatus.Closed:
                        return true;
                }
                return false;
            case GarnishmentPlanStatus.Pending:
                switch (next)
                {
                    case GarnishmentPlanStatus.Active:
                    case GarnishmentPlanStatus.Suspended:
                        return true;
                }
                return false;
            case GarnishmentPlanStatus.Active:
                return next == GarnishmentPlanStatus.Closed;
            case GarnishmentPlanStatus.Suspended:
                switch (next)
                {
                    case GarnishmentPlanStatus.Closed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(GarnishmentPlanStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"garnishment plan {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"garnishment plan"} {Code}";
    }

    public double AdjustedValue()
    {
        return Math.Round((double)DurationMinutes * (double)WindowDays, 2);
    }

    public GarnishmentPlan Copy()
    {
        return new GarnishmentPlan(
            Code,
            Name,
            LeaveDays,
            DurationMinutes,
            Skill,
            Summary,
            WindowDays,
            Phone,
            Timezone,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
