namespace Acme.Workforce.Engagement.ActionPlan.Slot;

using Acme.Workforce.Kit.Text;

/// <summary>Where a action plan slot is in its life cycle.</summary>
public enum ActionPlanSlotStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>A action plan slot. Its code is unique within a tenant.</summary>
public sealed class ActionPlanSlot(string code, string name, int updatedAt, string salaryBand, string checksum, int attempts, double discount, ActionPlanSlotStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public int UpdatedAt { get; set; } = updatedAt;
    public string SalaryBand { get; set; } = salaryBand;
    public string Checksum { get; set; } = checksum;
    public int Attempts { get; set; } = attempts;
    public double Discount { get; set; } = discount;
    public ActionPlanSlotStatus Status { get; set; } = status;

    /// <summary>Starts a action plan slot in status requested.</summary>
    public static ActionPlanSlot NewActionPlanSlot(string code, string name)
    {
        return new ActionPlanSlot(code, name, 0, "", "", 0, 0.0, ActionPlanSlotStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case ActionPlanSlotStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(ActionPlanSlotStatus next)
    {
        switch (Status)
        {
            case ActionPlanSlotStatus.Requested:
                switch (next)
                {
                    case ActionPlanSlotStatus.Granted:
                    case ActionPlanSlotStatus.Expired:
                        return true;
                }
                return false;
            case ActionPlanSlotStatus.Granted:
                return next == ActionPlanSlotStatus.Revoked || next == ActionPlanSlotStatus.Expired;
            case ActionPlanSlotStatus.Revoked:
                return next == ActionPlanSlotStatus.Expired;
        }
        return false;
    }

    public void MoveTo(ActionPlanSlotStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"action plan slot {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        UpdatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code} [{Name}]";
    }

    public double WeightedValue()
    {
        return Math.Round(Discount - (double)UpdatedAt, 2);
    }

    public ActionPlanSlot Copy()
    {
        return new ActionPlanSlot(Code, Name, UpdatedAt, SalaryBand, Checksum, Attempts, Discount, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
