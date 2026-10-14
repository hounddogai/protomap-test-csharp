namespace Acme.Workforce.Absence.Holiday.Metric;

using Acme.Workforce.Kit.Text;

/// <summary>Where a default holiday milestone is in its life cycle.</summary>
public enum DefaultHolidayMilestoneStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>A default holiday milestone as the holiday team keeps it.</summary>
public sealed class DefaultHolidayMilestone(string code, string label, int leaveDays, double weight, double budget, bool pinned, int expiresAt, double ceiling, int graceDays, DefaultHolidayMilestoneStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public int LeaveDays { get; set; } = leaveDays;
    public double Weight { get; set; } = weight;
    public double Budget { get; set; } = budget;
    public bool Pinned { get; set; } = pinned;
    public int ExpiresAt { get; set; } = expiresAt;
    public double Ceiling { get; set; } = ceiling;
    public int GraceDays { get; set; } = graceDays;
    public DefaultHolidayMilestoneStatus Status { get; set; } = status;

    /// <summary>Starts a default holiday milestone in status requested.</summary>
    public static DefaultHolidayMilestone NewDefaultHolidayMilestone(string code, string label)
    {
        return new DefaultHolidayMilestone(
            code,
            label,
            0,
            0.0,
            0.0,
            false,
            0,
            0.0,
            0,
            DefaultHolidayMilestoneStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case DefaultHolidayMilestoneStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(DefaultHolidayMilestoneStatus next)
    {
        switch (Status)
        {
            case DefaultHolidayMilestoneStatus.Requested:
                return next == DefaultHolidayMilestoneStatus.Granted || next == DefaultHolidayMilestoneStatus.Revoked;
            case DefaultHolidayMilestoneStatus.Granted:
                switch (next)
                {
                    case DefaultHolidayMilestoneStatus.Expired:
                        return true;
                }
                return false;
            case DefaultHolidayMilestoneStatus.Revoked:
                switch (next)
                {
                    case DefaultHolidayMilestoneStatus.Expired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(DefaultHolidayMilestoneStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"default holiday milestone {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        ExpiresAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{Code} / {Label}";
    }

    public double NetVolume()
    {
        return Math.Round((double)ExpiresAt * Weight, 2);
    }

    public DefaultHolidayMilestone Copy()
    {
        return new DefaultHolidayMilestone(
            Code,
            Label,
            LeaveDays,
            Weight,
            Budget,
            Pinned,
            ExpiresAt,
            Ceiling,
            GraceDays,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
