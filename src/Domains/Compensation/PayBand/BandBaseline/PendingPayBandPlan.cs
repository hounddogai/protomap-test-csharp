namespace Acme.Workforce.Compensation.PayBand.BandBaseline;

using Acme.Workforce.Kit.Text;

/// <summary>Where a pending pay band plan is in its life cycle.</summary>
public enum PendingPayBandPlanStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>A pending pay band plan. Its code is unique within a tenant.</summary>
public sealed class PendingPayBandPlan(string code, string title, string skill, int leaveDays, string currency, double cost, double budget, PendingPayBandPlanStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string Skill { get; set; } = skill;
    public int LeaveDays { get; set; } = leaveDays;
    public string Currency { get; set; } = currency;
    public double Cost { get; set; } = cost;
    public double Budget { get; set; } = budget;
    public PendingPayBandPlanStatus Status { get; set; } = status;

    /// <summary>Starts a pending pay band plan in status requested.</summary>
    public static PendingPayBandPlan NewPendingPayBandPlan(string code, string title)
    {
        return new PendingPayBandPlan(code, title, "", 0, "", 0.0, 0.0, PendingPayBandPlanStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PendingPayBandPlanStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PendingPayBandPlanStatus next)
    {
        switch (Status)
        {
            case PendingPayBandPlanStatus.Requested:
                return next == PendingPayBandPlanStatus.Revoked;
            case PendingPayBandPlanStatus.Granted:
                return next == PendingPayBandPlanStatus.Revoked || next == PendingPayBandPlanStatus.Expired;
            case PendingPayBandPlanStatus.Revoked:
                switch (next)
                {
                    case PendingPayBandPlanStatus.Expired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(PendingPayBandPlanStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"pending pay band plan {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"pending pay band plan"} {Code}";
    }

    public double EffectiveLoad()
    {
        return Math.Round((double)LeaveDays + Cost, 2);
    }

    public PendingPayBandPlan Copy()
    {
        return new PendingPayBandPlan(Code, Title, Skill, LeaveDays, Currency, Cost, Budget, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
