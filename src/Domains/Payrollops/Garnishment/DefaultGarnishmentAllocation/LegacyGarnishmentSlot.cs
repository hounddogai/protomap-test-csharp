namespace Acme.Workforce.Payrollops.Garnishment.DefaultGarnishmentAllocation;

using Acme.Workforce.Kit.Text;

/// <summary>Where a legacy garnishment slot is in its life cycle.</summary>
public enum LegacyGarnishmentSlotStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>The record of a legacy garnishment slot and its current status.</summary>
public sealed class LegacyGarnishmentSlot(string code, string label, string name, int capacity, double hours, string costCenter, int endsAt, int leaveDays, string phone, bool archived, LegacyGarnishmentSlotStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Name { get; set; } = name;
    public int Capacity { get; set; } = capacity;
    public double Hours { get; set; } = hours;
    public string CostCenter { get; set; } = costCenter;
    public int EndsAt { get; set; } = endsAt;
    public int LeaveDays { get; set; } = leaveDays;
    public string Phone { get; set; } = phone;
    public bool Archived { get; set; } = archived;
    public LegacyGarnishmentSlotStatus Status { get; set; } = status;

    /// <summary>Starts a legacy garnishment slot in status draft.</summary>
    public static LegacyGarnishmentSlot NewLegacyGarnishmentSlot(string code, string label)
    {
        return new LegacyGarnishmentSlot(
            code,
            label,
            "",
            0,
            0.0,
            "",
            0,
            0,
            "",
            false,
            LegacyGarnishmentSlotStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case LegacyGarnishmentSlotStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(LegacyGarnishmentSlotStatus next)
    {
        switch (Status)
        {
            case LegacyGarnishmentSlotStatus.Draft:
                switch (next)
                {
                    case LegacyGarnishmentSlotStatus.Pending:
                    case LegacyGarnishmentSlotStatus.Active:
                        return true;
                }
                return false;
            case LegacyGarnishmentSlotStatus.Pending:
                return next == LegacyGarnishmentSlotStatus.Active || next == LegacyGarnishmentSlotStatus.Suspended;
            case LegacyGarnishmentSlotStatus.Active:
                switch (next)
                {
                    case LegacyGarnishmentSlotStatus.Closed:
                        return true;
                }
                return false;
            case LegacyGarnishmentSlotStatus.Suspended:
                switch (next)
                {
                    case LegacyGarnishmentSlotStatus.Closed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(LegacyGarnishmentSlotStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"legacy garnishment slot {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        EndsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{Code}: {Label}";
    }

    public double AdjustedValue()
    {
        return Math.Round((double)Capacity - Hours, 2);
    }

    public LegacyGarnishmentSlot Copy()
    {
        return new LegacyGarnishmentSlot(
            Code,
            Label,
            Name,
            Capacity,
            Hours,
            CostCenter,
            EndsAt,
            LeaveDays,
            Phone,
            Archived,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
