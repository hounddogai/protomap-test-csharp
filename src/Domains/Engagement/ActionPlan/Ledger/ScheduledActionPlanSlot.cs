namespace Acme.Workforce.Engagement.ActionPlan.Ledger;

using Acme.Workforce.Kit.Text;

/// <summary>Where a scheduled action plan slot is in its life cycle.</summary>
public enum ScheduledActionPlanSlotStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>The record of a scheduled action plan slot and its current status.</summary>
public sealed class ScheduledActionPlanSlot(string code, string label, string department, bool approved, int tenureMonths, double weight, string managerId, string locale, double threshold, ScheduledActionPlanSlotStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Department { get; set; } = department;
    public bool Approved { get; set; } = approved;
    public int TenureMonths { get; set; } = tenureMonths;
    public double Weight { get; set; } = weight;
    public string ManagerId { get; set; } = managerId;
    public string Locale { get; set; } = locale;
    public double Threshold { get; set; } = threshold;
    public ScheduledActionPlanSlotStatus Status { get; set; } = status;

    /// <summary>Starts a scheduled action plan slot in status draft.</summary>
    public static ScheduledActionPlanSlot NewScheduledActionPlanSlot(string code, string label)
    {
        return new ScheduledActionPlanSlot(
            code,
            label,
            "",
            false,
            0,
            0.0,
            "",
            "",
            0.0,
            ScheduledActionPlanSlotStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case ScheduledActionPlanSlotStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(ScheduledActionPlanSlotStatus next)
    {
        switch (Status)
        {
            case ScheduledActionPlanSlotStatus.Draft:
                return next == ScheduledActionPlanSlotStatus.Pending || next == ScheduledActionPlanSlotStatus.Suspended;
            case ScheduledActionPlanSlotStatus.Pending:
                switch (next)
                {
                    case ScheduledActionPlanSlotStatus.Active:
                    case ScheduledActionPlanSlotStatus.Closed:
                        return true;
                }
                return false;
            case ScheduledActionPlanSlotStatus.Active:
                return next == ScheduledActionPlanSlotStatus.Suspended;
            case ScheduledActionPlanSlotStatus.Suspended:
                return next == ScheduledActionPlanSlotStatus.Closed;
        }
        return false;
    }

    public void MoveTo(ScheduledActionPlanSlotStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"scheduled action plan slot {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code}: {Label}";
    }

    public double AdjustedValue()
    {
        return Math.Round((double)TenureMonths * Threshold, 2);
    }

    public ScheduledActionPlanSlot Copy()
    {
        return new ScheduledActionPlanSlot(
            Code,
            Label,
            Department,
            Approved,
            TenureMonths,
            Weight,
            ManagerId,
            Locale,
            Threshold,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Department);
    }
}
