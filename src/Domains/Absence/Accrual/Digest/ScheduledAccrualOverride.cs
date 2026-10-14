namespace Acme.Workforce.Absence.Accrual.Digest;

using Acme.Workforce.Kit.Text;

/// <summary>Where a scheduled accrual override is in its life cycle.</summary>
public enum ScheduledAccrualOverrideStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>One scheduled accrual override, with what the planners need to act on it.</summary>
public sealed class ScheduledAccrualOverride(string code, string name, double discount, int updatedAt, double rating, string timezone, ScheduledAccrualOverrideStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public double Discount { get; set; } = discount;
    public int UpdatedAt { get; set; } = updatedAt;
    public double Rating { get; set; } = rating;
    public string Timezone { get; set; } = timezone;
    public ScheduledAccrualOverrideStatus Status { get; set; } = status;

    /// <summary>Starts a scheduled accrual override in status proposed.</summary>
    public static ScheduledAccrualOverride NewScheduledAccrualOverride(string code, string name)
    {
        return new ScheduledAccrualOverride(code, name, 0.0, 0, 0.0, "", ScheduledAccrualOverrideStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case ScheduledAccrualOverrideStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(ScheduledAccrualOverrideStatus next)
    {
        switch (Status)
        {
            case ScheduledAccrualOverrideStatus.Proposed:
                return next == ScheduledAccrualOverrideStatus.Approved || next == ScheduledAccrualOverrideStatus.Rejected;
            case ScheduledAccrualOverrideStatus.Approved:
                switch (next)
                {
                    case ScheduledAccrualOverrideStatus.Rejected:
                    case ScheduledAccrualOverrideStatus.Withdrawn:
                        return true;
                }
                return false;
            case ScheduledAccrualOverrideStatus.Rejected:
                return next == ScheduledAccrualOverrideStatus.Withdrawn;
        }
        return false;
    }

    public void MoveTo(ScheduledAccrualOverrideStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"scheduled accrual override {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        UpdatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{"scheduled accrual override"} {Code}";
    }

    public double WeightedTotal()
    {
        return Math.Round((double)UpdatedAt + Rating, 2);
    }

    public ScheduledAccrualOverride Copy()
    {
        return new ScheduledAccrualOverride(Code, Name, Discount, UpdatedAt, Rating, Timezone, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
