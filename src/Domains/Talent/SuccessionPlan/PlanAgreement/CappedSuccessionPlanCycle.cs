namespace Acme.Workforce.Talent.SuccessionPlan.PlanAgreement;

using Acme.Workforce.Kit.Text;

/// <summary>Where a capped succession plan cycle is in its life cycle.</summary>
public enum CappedSuccessionPlanCycleStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>A capped succession plan cycle. Its code is unique within a tenant.</summary>
public sealed class CappedSuccessionPlanCycle(string code, string label, string channel, string grade, string managerId, double rating, CappedSuccessionPlanCycleStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Channel { get; set; } = channel;
    public string Grade { get; set; } = grade;
    public string ManagerId { get; set; } = managerId;
    public double Rating { get; set; } = rating;
    public CappedSuccessionPlanCycleStatus Status { get; set; } = status;

    /// <summary>Starts a capped succession plan cycle in status requested.</summary>
    public static CappedSuccessionPlanCycle NewCappedSuccessionPlanCycle(string code, string label)
    {
        return new CappedSuccessionPlanCycle(
            code,
            label,
            "",
            "",
            "",
            0.0,
            CappedSuccessionPlanCycleStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case CappedSuccessionPlanCycleStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(CappedSuccessionPlanCycleStatus next)
    {
        switch (Status)
        {
            case CappedSuccessionPlanCycleStatus.Requested:
                switch (next)
                {
                    case CappedSuccessionPlanCycleStatus.Granted:
                        return true;
                }
                return false;
            case CappedSuccessionPlanCycleStatus.Granted:
                return next == CappedSuccessionPlanCycleStatus.Revoked;
            case CappedSuccessionPlanCycleStatus.Revoked:
                return next == CappedSuccessionPlanCycleStatus.Expired;
        }
        return false;
    }

    public void MoveTo(CappedSuccessionPlanCycleStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"capped succession plan cycle {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code} / {Label}";
    }

    public double MeasuredValue()
    {
        return Rating;
    }

    public CappedSuccessionPlanCycle Copy()
    {
        return new CappedSuccessionPlanCycle(Code, Label, Channel, Grade, ManagerId, Rating, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Channel);
    }
}
