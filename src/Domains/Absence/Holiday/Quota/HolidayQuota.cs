namespace Acme.Workforce.Absence.Holiday.Quota;

using Acme.Workforce.Kit.Text;

/// <summary>Where a holiday quota is in its life cycle.</summary>
public enum HolidayQuotaStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>One holiday quota, with what the planners need to act on it.</summary>
public sealed class HolidayQuota(string code, string name, int quantity, double score, string contactEmail, string summary, double amount, bool active, double floorValue, HolidayQuotaStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public int Quantity { get; set; } = quantity;
    public double Score { get; set; } = score;
    public string ContactEmail { get; set; } = contactEmail;
    public string Summary { get; set; } = summary;
    public double Amount { get; set; } = amount;
    public bool Active { get; set; } = active;
    public double FloorValue { get; set; } = floorValue;
    public HolidayQuotaStatus Status { get; set; } = status;

    /// <summary>Starts a holiday quota in status idle.</summary>
    public static HolidayQuota NewHolidayQuota(string code, string name)
    {
        return new HolidayQuota(code, name, 0, 0.0, "", "", 0.0, false, 0.0, HolidayQuotaStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case HolidayQuotaStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(HolidayQuotaStatus next)
    {
        switch (Status)
        {
            case HolidayQuotaStatus.Idle:
                return next == HolidayQuotaStatus.Warming || next == HolidayQuotaStatus.Stopped;
            case HolidayQuotaStatus.Warming:
                switch (next)
                {
                    case HolidayQuotaStatus.Ready:
                    case HolidayQuotaStatus.Draining:
                        return true;
                }
                return false;
            case HolidayQuotaStatus.Ready:
                return next == HolidayQuotaStatus.Stopped;
            case HolidayQuotaStatus.Draining:
                return next == HolidayQuotaStatus.Stopped;
        }
        return false;
    }

    public void MoveTo(HolidayQuotaStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"holiday quota {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} / {Name}";
    }

    public double WeightedExposure()
    {
        return Math.Round((double)Quantity + FloorValue, 2);
    }

    public HolidayQuota Copy()
    {
        return new HolidayQuota(
            Code,
            Name,
            Quantity,
            Score,
            ContactEmail,
            Summary,
            Amount,
            Active,
            FloorValue,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
