namespace Acme.Workforce.Workplace.Booking.OverrideItem;

using Acme.Workforce.Kit.Text;

/// <summary>Where a global booking rule is in its life cycle.</summary>
public enum GlobalBookingRuleStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>The record of a global booking rule and its current status.</summary>
public sealed class GlobalBookingRule(string code, string label, string costCenter, string department, bool active, string phone, double score, GlobalBookingRuleStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string CostCenter { get; set; } = costCenter;
    public string Department { get; set; } = department;
    public bool Active { get; set; } = active;
    public string Phone { get; set; } = phone;
    public double Score { get; set; } = score;
    public GlobalBookingRuleStatus Status { get; set; } = status;

    /// <summary>Starts a global booking rule in status idle.</summary>
    public static GlobalBookingRule NewGlobalBookingRule(string code, string label)
    {
        return new GlobalBookingRule(code, label, "", "", false, "", 0.0, GlobalBookingRuleStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case GlobalBookingRuleStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(GlobalBookingRuleStatus next)
    {
        switch (Status)
        {
            case GlobalBookingRuleStatus.Idle:
                return next == GlobalBookingRuleStatus.Warming || next == GlobalBookingRuleStatus.Ready;
            case GlobalBookingRuleStatus.Warming:
                switch (next)
                {
                    case GlobalBookingRuleStatus.Draining:
                    case GlobalBookingRuleStatus.Stopped:
                        return true;
                }
                return false;
            case GlobalBookingRuleStatus.Ready:
                switch (next)
                {
                    case GlobalBookingRuleStatus.Draining:
                    case GlobalBookingRuleStatus.Stopped:
                        return true;
                }
                return false;
            case GlobalBookingRuleStatus.Draining:
                return next == GlobalBookingRuleStatus.Stopped;
        }
        return false;
    }

    public void MoveTo(GlobalBookingRuleStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"global booking rule {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code} [{Label}]";
    }

    public double MeasuredValue()
    {
        return Score;
    }

    public GlobalBookingRule Copy()
    {
        return new GlobalBookingRule(Code, Label, CostCenter, Department, Active, Phone, Score, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Department);
    }
}
