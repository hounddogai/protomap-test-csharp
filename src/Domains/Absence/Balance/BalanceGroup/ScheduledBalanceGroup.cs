namespace Acme.Workforce.Absence.Balance.BalanceGroup;

using Acme.Workforce.Kit.Text;

/// <summary>Where a scheduled balance group is in its life cycle.</summary>
public enum ScheduledBalanceGroupStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>A scheduled balance group as the balance team keeps it.</summary>
public sealed class ScheduledBalanceGroup(string code, string name, double cost, string department, int capacity, string locale, string notes, List<string> tags, ScheduledBalanceGroupStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public double Cost { get; set; } = cost;
    public string Department { get; set; } = department;
    public int Capacity { get; set; } = capacity;
    public string Locale { get; set; } = locale;
    public string Notes { get; set; } = notes;
    public List<string> Tags { get; set; } = tags;
    public ScheduledBalanceGroupStatus Status { get; set; } = status;

    /// <summary>Starts a scheduled balance group in status planned.</summary>
    public static ScheduledBalanceGroup NewScheduledBalanceGroup(string code, string name)
    {
        return new ScheduledBalanceGroup(
            code,
            name,
            0.0,
            "",
            0,
            "",
            "",
            new List<string>(),
            ScheduledBalanceGroupStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case ScheduledBalanceGroupStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(ScheduledBalanceGroupStatus next)
    {
        switch (Status)
        {
            case ScheduledBalanceGroupStatus.Planned:
                switch (next)
                {
                    case ScheduledBalanceGroupStatus.Scheduled:
                    case ScheduledBalanceGroupStatus.Cancelled:
                        return true;
                }
                return false;
            case ScheduledBalanceGroupStatus.Scheduled:
                switch (next)
                {
                    case ScheduledBalanceGroupStatus.Completed:
                        return true;
                }
                return false;
            case ScheduledBalanceGroupStatus.InProgress:
                switch (next)
                {
                    case ScheduledBalanceGroupStatus.Cancelled:
                        return true;
                }
                return false;
            case ScheduledBalanceGroupStatus.Completed:
                switch (next)
                {
                    case ScheduledBalanceGroupStatus.Cancelled:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(ScheduledBalanceGroupStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"scheduled balance group {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} [{Name}]";
    }

    public bool HasTag(string tag)
    {
        foreach (var candidate in Tags)
        {
            if (Text.Normalize(candidate) == Text.Normalize(tag))
            {
                return true;
            }
        }
        return false;
    }

    public double AdjustedExposure()
    {
        return Math.Round((double)Capacity * Cost, 2);
    }

    public ScheduledBalanceGroup Copy()
    {
        return new ScheduledBalanceGroup(Code, Name, Cost, Department, Capacity, Locale, Notes, Tags, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Department);
    }
}
