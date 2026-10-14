namespace Acme.Workforce.Workplace.Booking.Review;

using Acme.Workforce.Kit.Text;

/// <summary>Where a scheduled booking threshold is in its life cycle.</summary>
public enum ScheduledBookingThresholdStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>The record of a scheduled booking threshold and its current status.</summary>
public sealed class ScheduledBookingThreshold(string code, string label, string reviewer, string checksum, string country, Dictionary<string, string> attributes, string grade, int batchSize, ScheduledBookingThresholdStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Reviewer { get; set; } = reviewer;
    public string Checksum { get; set; } = checksum;
    public string Country { get; set; } = country;
    public Dictionary<string, string> Attributes { get; set; } = attributes;
    public string Grade { get; set; } = grade;
    public int BatchSize { get; set; } = batchSize;
    public ScheduledBookingThresholdStatus Status { get; set; } = status;

    /// <summary>Starts a scheduled booking threshold in status requested.</summary>
    public static ScheduledBookingThreshold NewScheduledBookingThreshold(string code, string label)
    {
        return new ScheduledBookingThreshold(
            code,
            label,
            "",
            "",
            "",
            new Dictionary<string, string>(),
            "",
            0,
            ScheduledBookingThresholdStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case ScheduledBookingThresholdStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(ScheduledBookingThresholdStatus next)
    {
        switch (Status)
        {
            case ScheduledBookingThresholdStatus.Requested:
                switch (next)
                {
                    case ScheduledBookingThresholdStatus.Granted:
                        return true;
                }
                return false;
            case ScheduledBookingThresholdStatus.Granted:
                switch (next)
                {
                    case ScheduledBookingThresholdStatus.Revoked:
                        return true;
                }
                return false;
            case ScheduledBookingThresholdStatus.Revoked:
                return next == ScheduledBookingThresholdStatus.Expired;
        }
        return false;
    }

    public void MoveTo(ScheduledBookingThresholdStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"scheduled booking threshold {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{"scheduled booking threshold"} {Code}";
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

    public double MeasuredValue()
    {
        return (double)BatchSize;
    }

    public ScheduledBookingThreshold Copy()
    {
        return new ScheduledBookingThreshold(
            Code,
            Label,
            Reviewer,
            Checksum,
            Country,
            Attributes,
            Grade,
            BatchSize,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Country);
    }
}
