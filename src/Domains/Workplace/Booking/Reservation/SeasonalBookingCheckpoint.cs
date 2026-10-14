namespace Acme.Workforce.Workplace.Booking.Reservation;

using Acme.Workforce.Kit.Text;

/// <summary>Where a seasonal booking checkpoint is in its life cycle.</summary>
public enum SeasonalBookingCheckpointStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>One seasonal booking checkpoint, with what the planners need to act on it.</summary>
public sealed class SeasonalBookingCheckpoint(string code, string label, Dictionary<string, string> attributes, int endsAt, int graceDays, double margin, int leaveDays, SeasonalBookingCheckpointStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public Dictionary<string, string> Attributes { get; set; } = attributes;
    public int EndsAt { get; set; } = endsAt;
    public int GraceDays { get; set; } = graceDays;
    public double Margin { get; set; } = margin;
    public int LeaveDays { get; set; } = leaveDays;
    public SeasonalBookingCheckpointStatus Status { get; set; } = status;

    /// <summary>Starts a seasonal booking checkpoint in status planned.</summary>
    public static SeasonalBookingCheckpoint NewSeasonalBookingCheckpoint(string code, string label)
    {
        return new SeasonalBookingCheckpoint(
            code,
            label,
            new Dictionary<string, string>(),
            0,
            0,
            0.0,
            0,
            SeasonalBookingCheckpointStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case SeasonalBookingCheckpointStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(SeasonalBookingCheckpointStatus next)
    {
        switch (Status)
        {
            case SeasonalBookingCheckpointStatus.Planned:
                return next == SeasonalBookingCheckpointStatus.Scheduled;
            case SeasonalBookingCheckpointStatus.Scheduled:
                return next == SeasonalBookingCheckpointStatus.Completed || next == SeasonalBookingCheckpointStatus.Cancelled;
            case SeasonalBookingCheckpointStatus.InProgress:
                return next == SeasonalBookingCheckpointStatus.Completed || next == SeasonalBookingCheckpointStatus.Cancelled;
            case SeasonalBookingCheckpointStatus.Completed:
                return next == SeasonalBookingCheckpointStatus.Cancelled;
        }
        return false;
    }

    public void MoveTo(SeasonalBookingCheckpointStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"seasonal booking checkpoint {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        EndsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{Code}: {Label}";
    }

    public string Attribute(string key, string fallback)
    {
        if (!Attributes.TryGetValue(key, out var found))
        {
            return fallback;
        }
        if (found.Trim() == "")
        {
            return fallback;
        }
        return found;
    }

    public double AdjustedValue()
    {
        return Math.Round(Margin + (double)EndsAt, 2);
    }

    public SeasonalBookingCheckpoint Copy()
    {
        return new SeasonalBookingCheckpoint(Code, Label, Attributes, EndsAt, GraceDays, Margin, LeaveDays, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
