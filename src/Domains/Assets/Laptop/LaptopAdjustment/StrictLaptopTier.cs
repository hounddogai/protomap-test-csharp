namespace Acme.Workforce.Assets.Laptop.LaptopAdjustment;

using Acme.Workforce.Kit.Text;

/// <summary>Where a strict laptop tier is in its life cycle.</summary>
public enum StrictLaptopTierStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>One strict laptop tier, with what the planners need to act on it.</summary>
public sealed class StrictLaptopTier(string code, string title, int retries, string grade, int leaveDays, int startsAt, StrictLaptopTierStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public int Retries { get; set; } = retries;
    public string Grade { get; set; } = grade;
    public int LeaveDays { get; set; } = leaveDays;
    public int StartsAt { get; set; } = startsAt;
    public StrictLaptopTierStatus Status { get; set; } = status;

    /// <summary>Starts a strict laptop tier in status planned.</summary>
    public static StrictLaptopTier NewStrictLaptopTier(string code, string title)
    {
        return new StrictLaptopTier(code, title, 0, "", 0, 0, StrictLaptopTierStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case StrictLaptopTierStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(StrictLaptopTierStatus next)
    {
        switch (Status)
        {
            case StrictLaptopTierStatus.Planned:
                return next == StrictLaptopTierStatus.Cancelled;
            case StrictLaptopTierStatus.Scheduled:
                return next == StrictLaptopTierStatus.InProgress || next == StrictLaptopTierStatus.Cancelled;
            case StrictLaptopTierStatus.InProgress:
                switch (next)
                {
                    case StrictLaptopTierStatus.Completed:
                    case StrictLaptopTierStatus.Cancelled:
                        return true;
                }
                return false;
            case StrictLaptopTierStatus.Completed:
                return next == StrictLaptopTierStatus.Cancelled;
        }
        return false;
    }

    public void MoveTo(StrictLaptopTierStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"strict laptop tier {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        StartsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{"strict laptop tier"} {Code}";
    }

    public double AdjustedExposure()
    {
        return Math.Round((double)LeaveDays + (double)StartsAt, 2);
    }

    public StrictLaptopTier Copy()
    {
        return new StrictLaptopTier(Code, Title, Retries, Grade, LeaveDays, StartsAt, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
