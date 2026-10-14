namespace Acme.Workforce.Compliance.Exception.Group;

using Acme.Workforce.Kit.Text;

/// <summary>Where a exception plan is in its life cycle.</summary>
public enum ExceptionPlanStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>A exception plan. Its code is unique within a tenant.</summary>
public sealed class ExceptionPlan(string code, string label, double margin, string badge, double rate, string skill, ExceptionPlanStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public double Margin { get; set; } = margin;
    public string Badge { get; set; } = badge;
    public double Rate { get; set; } = rate;
    public string Skill { get; set; } = skill;
    public ExceptionPlanStatus Status { get; set; } = status;

    /// <summary>Starts a exception plan in status requested.</summary>
    public static ExceptionPlan NewExceptionPlan(string code, string label)
    {
        return new ExceptionPlan(code, label, 0.0, "", 0.0, "", ExceptionPlanStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case ExceptionPlanStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(ExceptionPlanStatus next)
    {
        switch (Status)
        {
            case ExceptionPlanStatus.Requested:
                switch (next)
                {
                    case ExceptionPlanStatus.Expired:
                        return true;
                }
                return false;
            case ExceptionPlanStatus.Granted:
                return next == ExceptionPlanStatus.Revoked || next == ExceptionPlanStatus.Expired;
            case ExceptionPlanStatus.Revoked:
                return next == ExceptionPlanStatus.Expired;
        }
        return false;
    }

    public void MoveTo(ExceptionPlanStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"exception plan {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code} ({Label})";
    }

    public double EffectiveValue()
    {
        return Math.Round(Rate + Margin, 2);
    }

    public ExceptionPlan Copy()
    {
        return new ExceptionPlan(Code, Label, Margin, Badge, Rate, Skill, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
