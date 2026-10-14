namespace Acme.Workforce.Talent.SuccessionPlan.Quota;

using Acme.Workforce.Kit.Text;

/// <summary>Where a rolling succession plan request is in its life cycle.</summary>
public enum RollingSuccessionPlanRequestStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>The record of a rolling succession plan request and its current status.</summary>
public sealed class RollingSuccessionPlanRequest(string code, string name, string badge, string locale, double rate, RollingSuccessionPlanRequestStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string Badge { get; set; } = badge;
    public string Locale { get; set; } = locale;
    public double Rate { get; set; } = rate;
    public RollingSuccessionPlanRequestStatus Status { get; set; } = status;

    /// <summary>Starts a rolling succession plan request in status proposed.</summary>
    public static RollingSuccessionPlanRequest NewRollingSuccessionPlanRequest(string code, string name)
    {
        return new RollingSuccessionPlanRequest(
            code,
            name,
            "",
            "",
            0.0,
            RollingSuccessionPlanRequestStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case RollingSuccessionPlanRequestStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(RollingSuccessionPlanRequestStatus next)
    {
        switch (Status)
        {
            case RollingSuccessionPlanRequestStatus.Proposed:
                return next == RollingSuccessionPlanRequestStatus.Rejected || next == RollingSuccessionPlanRequestStatus.Withdrawn;
            case RollingSuccessionPlanRequestStatus.Approved:
                switch (next)
                {
                    case RollingSuccessionPlanRequestStatus.Rejected:
                        return true;
                }
                return false;
            case RollingSuccessionPlanRequestStatus.Rejected:
                return next == RollingSuccessionPlanRequestStatus.Withdrawn;
        }
        return false;
    }

    public void MoveTo(RollingSuccessionPlanRequestStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"rolling succession plan request {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} ({Name})";
    }

    public double MeasuredValue()
    {
        return Rate;
    }

    public RollingSuccessionPlanRequest Copy()
    {
        return new RollingSuccessionPlanRequest(Code, Name, Badge, Locale, Rate, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Locale);
    }
}
