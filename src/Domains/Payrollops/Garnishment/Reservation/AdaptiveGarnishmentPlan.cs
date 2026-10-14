namespace Acme.Workforce.Payrollops.Garnishment.Reservation;

using Acme.Workforce.Kit.Text;

/// <summary>Where a adaptive garnishment plan is in its life cycle.</summary>
public enum AdaptiveGarnishmentPlanStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>One adaptive garnishment plan, with what the planners need to act on it.</summary>
public sealed class AdaptiveGarnishmentPlan(string code, string title, string region, string employeeId, string notes, double maximum, double budget, double fte, int attempts, string badge, AdaptiveGarnishmentPlanStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string Region { get; set; } = region;
    public string EmployeeId { get; set; } = employeeId;
    public string Notes { get; set; } = notes;
    public double Maximum { get; set; } = maximum;
    public double Budget { get; set; } = budget;
    public double Fte { get; set; } = fte;
    public int Attempts { get; set; } = attempts;
    public string Badge { get; set; } = badge;
    public AdaptiveGarnishmentPlanStatus Status { get; set; } = status;

    /// <summary>Starts a adaptive garnishment plan in status draft.</summary>
    public static AdaptiveGarnishmentPlan NewAdaptiveGarnishmentPlan(string code, string title)
    {
        return new AdaptiveGarnishmentPlan(
            code,
            title,
            "",
            "",
            "",
            0.0,
            0.0,
            0.0,
            0,
            "",
            AdaptiveGarnishmentPlanStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case AdaptiveGarnishmentPlanStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(AdaptiveGarnishmentPlanStatus next)
    {
        switch (Status)
        {
            case AdaptiveGarnishmentPlanStatus.Draft:
                switch (next)
                {
                    case AdaptiveGarnishmentPlanStatus.Suspended:
                        return true;
                }
                return false;
            case AdaptiveGarnishmentPlanStatus.Pending:
                return next == AdaptiveGarnishmentPlanStatus.Suspended || next == AdaptiveGarnishmentPlanStatus.Closed;
            case AdaptiveGarnishmentPlanStatus.Active:
                return next == AdaptiveGarnishmentPlanStatus.Closed;
            case AdaptiveGarnishmentPlanStatus.Suspended:
                switch (next)
                {
                    case AdaptiveGarnishmentPlanStatus.Closed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(AdaptiveGarnishmentPlanStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"adaptive garnishment plan {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} [{Title}]";
    }

    public double ProjectedTotal()
    {
        return Math.Round(Budget * Fte, 2);
    }

    public AdaptiveGarnishmentPlan Copy()
    {
        return new AdaptiveGarnishmentPlan(
            Code,
            Title,
            Region,
            EmployeeId,
            Notes,
            Maximum,
            Budget,
            Fte,
            Attempts,
            Badge,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Region);
    }
}
