namespace Acme.Workforce.Absence.Holiday.Variance;

using Acme.Workforce.Kit.Text;

/// <summary>Where a holiday threshold is in its life cycle.</summary>
public enum HolidayThresholdStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>A holiday threshold as the holiday team keeps it.</summary>
public sealed class HolidayThreshold(string code, string title, double amount, string checksum, string notes, int sequence, double hours, string description, string segment, int expiresAt, string externalRef, HolidayThresholdStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public double Amount { get; set; } = amount;
    public string Checksum { get; set; } = checksum;
    public string Notes { get; set; } = notes;
    public int Sequence { get; set; } = sequence;
    public double Hours { get; set; } = hours;
    public string Description { get; set; } = description;
    public string Segment { get; set; } = segment;
    public int ExpiresAt { get; set; } = expiresAt;
    public string ExternalRef { get; set; } = externalRef;
    public HolidayThresholdStatus Status { get; set; } = status;

    /// <summary>Starts a holiday threshold in status planned.</summary>
    public static HolidayThreshold NewHolidayThreshold(string code, string title)
    {
        return new HolidayThreshold(code, title, 0.0, "", "", 0, 0.0, "", "", 0, "", HolidayThresholdStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case HolidayThresholdStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(HolidayThresholdStatus next)
    {
        switch (Status)
        {
            case HolidayThresholdStatus.Planned:
                return next == HolidayThresholdStatus.Scheduled;
            case HolidayThresholdStatus.Scheduled:
                return next == HolidayThresholdStatus.Cancelled;
            case HolidayThresholdStatus.InProgress:
                return next == HolidayThresholdStatus.Completed;
            case HolidayThresholdStatus.Completed:
                switch (next)
                {
                    case HolidayThresholdStatus.Cancelled:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(HolidayThresholdStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"holiday threshold {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        ExpiresAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code}: {Title}";
    }

    public double WeightedExposure()
    {
        return Math.Round(Hours + (double)ExpiresAt, 2);
    }

    public HolidayThreshold Copy()
    {
        return new HolidayThreshold(
            Code,
            Title,
            Amount,
            Checksum,
            Notes,
            Sequence,
            Hours,
            Description,
            Segment,
            ExpiresAt,
            ExternalRef,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Segment);
    }
}
