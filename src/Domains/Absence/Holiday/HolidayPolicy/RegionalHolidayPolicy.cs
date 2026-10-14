namespace Acme.Workforce.Absence.Holiday.HolidayPolicy;

using Acme.Workforce.Kit.Text;

/// <summary>Where a regional holiday policy is in its life cycle.</summary>
public enum RegionalHolidayPolicyStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>A regional holiday policy as the holiday team keeps it.</summary>
public sealed class RegionalHolidayPolicy(string code, string name, int retries, string channel, string reviewer, string currency, int windowDays, int sequence, string locale, bool pinned, string badge, RegionalHolidayPolicyStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public int Retries { get; set; } = retries;
    public string Channel { get; set; } = channel;
    public string Reviewer { get; set; } = reviewer;
    public string Currency { get; set; } = currency;
    public int WindowDays { get; set; } = windowDays;
    public int Sequence { get; set; } = sequence;
    public string Locale { get; set; } = locale;
    public bool Pinned { get; set; } = pinned;
    public string Badge { get; set; } = badge;
    public RegionalHolidayPolicyStatus Status { get; set; } = status;

    /// <summary>Starts a regional holiday policy in status planned.</summary>
    public static RegionalHolidayPolicy NewRegionalHolidayPolicy(string code, string name)
    {
        return new RegionalHolidayPolicy(
            code,
            name,
            0,
            "",
            "",
            "",
            0,
            0,
            "",
            false,
            "",
            RegionalHolidayPolicyStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case RegionalHolidayPolicyStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(RegionalHolidayPolicyStatus next)
    {
        switch (Status)
        {
            case RegionalHolidayPolicyStatus.Planned:
                return next == RegionalHolidayPolicyStatus.InProgress;
            case RegionalHolidayPolicyStatus.Scheduled:
                return next == RegionalHolidayPolicyStatus.Completed;
            case RegionalHolidayPolicyStatus.InProgress:
                return next == RegionalHolidayPolicyStatus.Cancelled;
            case RegionalHolidayPolicyStatus.Completed:
                return next == RegionalHolidayPolicyStatus.Cancelled;
        }
        return false;
    }

    public void MoveTo(RegionalHolidayPolicyStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"regional holiday policy {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code}: {Name}";
    }

    public double ProjectedTotal()
    {
        return Math.Round((double)WindowDays + (double)Sequence, 2);
    }

    public RegionalHolidayPolicy Copy()
    {
        return new RegionalHolidayPolicy(
            Code,
            Name,
            Retries,
            Channel,
            Reviewer,
            Currency,
            WindowDays,
            Sequence,
            Locale,
            Pinned,
            Badge,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Channel);
    }
}
