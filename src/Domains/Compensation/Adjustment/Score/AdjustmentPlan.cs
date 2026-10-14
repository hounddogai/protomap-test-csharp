namespace Acme.Workforce.Compensation.Adjustment.Score;

using Acme.Workforce.Kit.Text;

/// <summary>Where a adjustment plan is in its life cycle.</summary>
public enum AdjustmentPlanStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>A adjustment plan as the adjustment team keeps it.</summary>
public sealed class AdjustmentPlan(string code, string label, string skill, double ratio, double ceiling, int capacity, string jobCode, double discount, AdjustmentPlanStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Skill { get; set; } = skill;
    public double Ratio { get; set; } = ratio;
    public double Ceiling { get; set; } = ceiling;
    public int Capacity { get; set; } = capacity;
    public string JobCode { get; set; } = jobCode;
    public double Discount { get; set; } = discount;
    public AdjustmentPlanStatus Status { get; set; } = status;

    /// <summary>Starts a adjustment plan in status requested.</summary>
    public static AdjustmentPlan NewAdjustmentPlan(string code, string label)
    {
        return new AdjustmentPlan(code, label, "", 0.0, 0.0, 0, "", 0.0, AdjustmentPlanStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case AdjustmentPlanStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(AdjustmentPlanStatus next)
    {
        switch (Status)
        {
            case AdjustmentPlanStatus.Requested:
                return next == AdjustmentPlanStatus.Granted || next == AdjustmentPlanStatus.Revoked;
            case AdjustmentPlanStatus.Granted:
                switch (next)
                {
                    case AdjustmentPlanStatus.Revoked:
                        return true;
                }
                return false;
            case AdjustmentPlanStatus.Revoked:
                return next == AdjustmentPlanStatus.Expired;
        }
        return false;
    }

    public void MoveTo(AdjustmentPlanStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"adjustment plan {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code} [{Label}]";
    }

    public double EffectiveValue()
    {
        return Math.Round(Discount * (double)Capacity, 2);
    }

    public AdjustmentPlan Copy()
    {
        return new AdjustmentPlan(Code, Label, Skill, Ratio, Ceiling, Capacity, JobCode, Discount, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
