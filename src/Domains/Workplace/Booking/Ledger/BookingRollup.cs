namespace Acme.Workforce.Workplace.Booking.Ledger;

using Acme.Workforce.Kit.Text;

/// <summary>Where a booking rollup is in its life cycle.</summary>
public enum BookingRollupStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>The record of a booking rollup and its current status.</summary>
public sealed class BookingRollup(string code, string title, Dictionary<string, string> attributes, double fte, string timezone, List<string> tags, string source, BookingRollupStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public Dictionary<string, string> Attributes { get; set; } = attributes;
    public double Fte { get; set; } = fte;
    public string Timezone { get; set; } = timezone;
    public List<string> Tags { get; set; } = tags;
    public string Source { get; set; } = source;
    public BookingRollupStatus Status { get; set; } = status;

    /// <summary>Starts a booking rollup in status requested.</summary>
    public static BookingRollup NewBookingRollup(string code, string title)
    {
        return new BookingRollup(
            code,
            title,
            new Dictionary<string, string>(),
            0.0,
            "",
            new List<string>(),
            "",
            BookingRollupStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case BookingRollupStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(BookingRollupStatus next)
    {
        switch (Status)
        {
            case BookingRollupStatus.Requested:
                return next == BookingRollupStatus.Granted;
            case BookingRollupStatus.Granted:
                return next == BookingRollupStatus.Revoked || next == BookingRollupStatus.Expired;
            case BookingRollupStatus.Revoked:
                return next == BookingRollupStatus.Expired;
        }
        return false;
    }

    public void MoveTo(BookingRollupStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"booking rollup {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code}: {Title}";
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
        return Fte;
    }

    public BookingRollup Copy()
    {
        return new BookingRollup(Code, Title, Attributes, Fte, Timezone, Tags, Source, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Source);
    }
}
