namespace Acme.Workforce.Absence.Accrual.Slot;

using Acme.Workforce.Kit.Text;

/// <summary>Where a accrual slot is in its life cycle.</summary>
public enum AccrualSlotStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>The record of a accrual slot and its current status.</summary>
public sealed class AccrualSlot(string code, string name, string locale, string region, double minimum, int startsAt, string phone, string costCenter, double cost, string category, int limit, AccrualSlotStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string Locale { get; set; } = locale;
    public string Region { get; set; } = region;
    public double Minimum { get; set; } = minimum;
    public int StartsAt { get; set; } = startsAt;
    public string Phone { get; set; } = phone;
    public string CostCenter { get; set; } = costCenter;
    public double Cost { get; set; } = cost;
    public string Category { get; set; } = category;
    public int Limit { get; set; } = limit;
    public AccrualSlotStatus Status { get; set; } = status;

    /// <summary>Starts a accrual slot in status requested.</summary>
    public static AccrualSlot NewAccrualSlot(string code, string name)
    {
        return new AccrualSlot(code, name, "", "", 0.0, 0, "", "", 0.0, "", 0, AccrualSlotStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case AccrualSlotStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(AccrualSlotStatus next)
    {
        switch (Status)
        {
            case AccrualSlotStatus.Requested:
                switch (next)
                {
                    case AccrualSlotStatus.Granted:
                    case AccrualSlotStatus.Revoked:
                        return true;
                }
                return false;
            case AccrualSlotStatus.Granted:
                switch (next)
                {
                    case AccrualSlotStatus.Revoked:
                    case AccrualSlotStatus.Expired:
                        return true;
                }
                return false;
            case AccrualSlotStatus.Revoked:
                switch (next)
                {
                    case AccrualSlotStatus.Expired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(AccrualSlotStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"accrual slot {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        StartsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code}: {Name}";
    }

    public double AdjustedExposure()
    {
        return Math.Round(Cost + (double)Limit, 2);
    }

    public AccrualSlot Copy()
    {
        return new AccrualSlot(
            Code,
            Name,
            Locale,
            Region,
            Minimum,
            StartsAt,
            Phone,
            CostCenter,
            Cost,
            Category,
            Limit,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Locale);
    }
}
