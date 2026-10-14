namespace Acme.Workforce.Workplace.Booking.Baseline;

using Acme.Workforce.Kit.Text;

/// <summary>Where a booking adjustment is in its life cycle.</summary>
public enum BookingAdjustmentStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>One booking adjustment, with what the planners need to act on it.</summary>
public sealed class BookingAdjustment(string code, string label, string segment, double minimum, string currency, Dictionary<string, string> attributes, bool active, int createdAt, int version, int updatedAt, int leaveDays, BookingAdjustmentStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Segment { get; set; } = segment;
    public double Minimum { get; set; } = minimum;
    public string Currency { get; set; } = currency;
    public Dictionary<string, string> Attributes { get; set; } = attributes;
    public bool Active { get; set; } = active;
    public int CreatedAt { get; set; } = createdAt;
    public int Version { get; set; } = version;
    public int UpdatedAt { get; set; } = updatedAt;
    public int LeaveDays { get; set; } = leaveDays;
    public BookingAdjustmentStatus Status { get; set; } = status;

    /// <summary>Starts a booking adjustment in status idle.</summary>
    public static BookingAdjustment NewBookingAdjustment(string code, string label)
    {
        return new BookingAdjustment(
            code,
            label,
            "",
            0.0,
            "",
            new Dictionary<string, string>(),
            false,
            0,
            0,
            0,
            0,
            BookingAdjustmentStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case BookingAdjustmentStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(BookingAdjustmentStatus next)
    {
        switch (Status)
        {
            case BookingAdjustmentStatus.Idle:
                return next == BookingAdjustmentStatus.Draining;
            case BookingAdjustmentStatus.Warming:
                return next == BookingAdjustmentStatus.Stopped;
            case BookingAdjustmentStatus.Ready:
                return next == BookingAdjustmentStatus.Stopped;
            case BookingAdjustmentStatus.Draining:
                switch (next)
                {
                    case BookingAdjustmentStatus.Stopped:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(BookingAdjustmentStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"booking adjustment {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        CreatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        Version += 1;
    }

    public string DisplayLabel()
    {
        return $"{"booking adjustment"} {Code}";
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

    public double EffectiveValue()
    {
        return Math.Round((double)CreatedAt * Minimum, 2);
    }

    public BookingAdjustment Copy()
    {
        return new BookingAdjustment(
            Code,
            Label,
            Segment,
            Minimum,
            Currency,
            Attributes,
            Active,
            CreatedAt,
            Version,
            UpdatedAt,
            LeaveDays,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Segment);
    }
}
