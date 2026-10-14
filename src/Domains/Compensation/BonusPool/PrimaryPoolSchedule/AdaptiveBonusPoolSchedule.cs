namespace Acme.Workforce.Compensation.BonusPool.PrimaryPoolSchedule;

using Acme.Workforce.Kit.Text;

/// <summary>Where a adaptive bonus pool schedule is in its life cycle.</summary>
public enum AdaptiveBonusPoolScheduleStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>A adaptive bonus pool schedule as the bonus pool team keeps it.</summary>
public sealed class AdaptiveBonusPoolSchedule(string code, string name, string country, int priority, double ceiling, string notes, string description, int limit, string currency, string category, string reviewer, AdaptiveBonusPoolScheduleStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string Country { get; set; } = country;
    public int Priority { get; set; } = priority;
    public double Ceiling { get; set; } = ceiling;
    public string Notes { get; set; } = notes;
    public string Description { get; set; } = description;
    public int Limit { get; set; } = limit;
    public string Currency { get; set; } = currency;
    public string Category { get; set; } = category;
    public string Reviewer { get; set; } = reviewer;
    public AdaptiveBonusPoolScheduleStatus Status { get; set; } = status;

    /// <summary>Starts a adaptive bonus pool schedule in status idle.</summary>
    public static AdaptiveBonusPoolSchedule NewAdaptiveBonusPoolSchedule(string code, string name)
    {
        return new AdaptiveBonusPoolSchedule(
            code,
            name,
            "",
            0,
            0.0,
            "",
            "",
            0,
            "",
            "",
            "",
            AdaptiveBonusPoolScheduleStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case AdaptiveBonusPoolScheduleStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(AdaptiveBonusPoolScheduleStatus next)
    {
        switch (Status)
        {
            case AdaptiveBonusPoolScheduleStatus.Idle:
                return next == AdaptiveBonusPoolScheduleStatus.Stopped;
            case AdaptiveBonusPoolScheduleStatus.Warming:
                switch (next)
                {
                    case AdaptiveBonusPoolScheduleStatus.Stopped:
                        return true;
                }
                return false;
            case AdaptiveBonusPoolScheduleStatus.Ready:
                return next == AdaptiveBonusPoolScheduleStatus.Stopped;
            case AdaptiveBonusPoolScheduleStatus.Draining:
                return next == AdaptiveBonusPoolScheduleStatus.Stopped;
        }
        return false;
    }

    public void MoveTo(AdaptiveBonusPoolScheduleStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"adaptive bonus pool schedule {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} / {Name}";
    }

    public double NetLoad()
    {
        return Math.Round(Ceiling + (double)Priority, 2);
    }

    public AdaptiveBonusPoolSchedule Copy()
    {
        return new AdaptiveBonusPoolSchedule(
            Code,
            Name,
            Country,
            Priority,
            Ceiling,
            Notes,
            Description,
            Limit,
            Currency,
            Category,
            Reviewer,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Country);
    }
}
