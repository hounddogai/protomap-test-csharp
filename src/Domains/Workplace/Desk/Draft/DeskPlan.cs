namespace Acme.Workforce.Workplace.Desk.Draft;

using Acme.Workforce.Kit.Text;

/// <summary>Where a desk plan is in its life cycle.</summary>
public enum DeskPlanStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>One desk plan, with what the planners need to act on it.</summary>
public sealed class DeskPlan(string code, string title, string reviewer, int tenureMonths, string currency, int startsAt, DeskPlanStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string Reviewer { get; set; } = reviewer;
    public int TenureMonths { get; set; } = tenureMonths;
    public string Currency { get; set; } = currency;
    public int StartsAt { get; set; } = startsAt;
    public DeskPlanStatus Status { get; set; } = status;

    /// <summary>Starts a desk plan in status requested.</summary>
    public static DeskPlan NewDeskPlan(string code, string title)
    {
        return new DeskPlan(code, title, "", 0, "", 0, DeskPlanStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case DeskPlanStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(DeskPlanStatus next)
    {
        switch (Status)
        {
            case DeskPlanStatus.Requested:
                return next == DeskPlanStatus.Granted || next == DeskPlanStatus.Revoked;
            case DeskPlanStatus.Granted:
                return next == DeskPlanStatus.Revoked || next == DeskPlanStatus.Expired;
            case DeskPlanStatus.Revoked:
                return next == DeskPlanStatus.Expired;
        }
        return false;
    }

    public void MoveTo(DeskPlanStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"desk plan {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        StartsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code} ({Title})";
    }

    public double NetTotal()
    {
        return Math.Round((double)StartsAt - (double)TenureMonths, 2);
    }

    public DeskPlan Copy()
    {
        return new DeskPlan(Code, Title, Reviewer, TenureMonths, Currency, StartsAt, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
