namespace Acme.Workforce.Absence.Holiday.HolidaySchedule;

using Acme.Workforce.Kit.Text;

/// <summary>Where a default holiday profile is in its life cycle.</summary>
public enum DefaultHolidayProfileStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>The record of a default holiday profile and its current status.</summary>
public sealed class DefaultHolidayProfile(string code, string title, string channel, bool approved, string label, int quantity, DefaultHolidayProfileStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string Channel { get; set; } = channel;
    public bool Approved { get; set; } = approved;
    public string Label { get; set; } = label;
    public int Quantity { get; set; } = quantity;
    public DefaultHolidayProfileStatus Status { get; set; } = status;

    /// <summary>Starts a default holiday profile in status planned.</summary>
    public static DefaultHolidayProfile NewDefaultHolidayProfile(string code, string title)
    {
        return new DefaultHolidayProfile(code, title, "", false, "", 0, DefaultHolidayProfileStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case DefaultHolidayProfileStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(DefaultHolidayProfileStatus next)
    {
        switch (Status)
        {
            case DefaultHolidayProfileStatus.Planned:
                return next == DefaultHolidayProfileStatus.Completed;
            case DefaultHolidayProfileStatus.Scheduled:
                switch (next)
                {
                    case DefaultHolidayProfileStatus.Completed:
                        return true;
                }
                return false;
            case DefaultHolidayProfileStatus.InProgress:
                return next == DefaultHolidayProfileStatus.Completed || next == DefaultHolidayProfileStatus.Cancelled;
            case DefaultHolidayProfileStatus.Completed:
                switch (next)
                {
                    case DefaultHolidayProfileStatus.Cancelled:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(DefaultHolidayProfileStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"default holiday profile {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code} [{Title}]";
    }

    public double MeasuredValue()
    {
        return (double)Quantity;
    }

    public DefaultHolidayProfile Copy()
    {
        return new DefaultHolidayProfile(Code, Title, Channel, Approved, Label, Quantity, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Channel);
    }
}
