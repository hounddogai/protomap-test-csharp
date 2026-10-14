namespace Acme.Workforce.Workplace.Booking.Review;

using Acme.Workforce.Kit.Text;

/// <summary>Where a weighted booking entry is in its life cycle.</summary>
public enum WeightedBookingEntryStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>The record of a weighted booking entry and its current status.</summary>
public sealed class WeightedBookingEntry(string code, string name, Dictionary<string, string> attributes, int version, string channel, WeightedBookingEntryStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public Dictionary<string, string> Attributes { get; set; } = attributes;
    public int Version { get; set; } = version;
    public string Channel { get; set; } = channel;
    public WeightedBookingEntryStatus Status { get; set; } = status;

    /// <summary>Starts a weighted booking entry in status draft.</summary>
    public static WeightedBookingEntry NewWeightedBookingEntry(string code, string name)
    {
        return new WeightedBookingEntry(
            code,
            name,
            new Dictionary<string, string>(),
            0,
            "",
            WeightedBookingEntryStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case WeightedBookingEntryStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(WeightedBookingEntryStatus next)
    {
        switch (Status)
        {
            case WeightedBookingEntryStatus.Draft:
                switch (next)
                {
                    case WeightedBookingEntryStatus.Active:
                    case WeightedBookingEntryStatus.Suspended:
                        return true;
                }
                return false;
            case WeightedBookingEntryStatus.Pending:
                switch (next)
                {
                    case WeightedBookingEntryStatus.Suspended:
                    case WeightedBookingEntryStatus.Closed:
                        return true;
                }
                return false;
            case WeightedBookingEntryStatus.Active:
                switch (next)
                {
                    case WeightedBookingEntryStatus.Suspended:
                    case WeightedBookingEntryStatus.Closed:
                        return true;
                }
                return false;
            case WeightedBookingEntryStatus.Suspended:
                switch (next)
                {
                    case WeightedBookingEntryStatus.Closed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(WeightedBookingEntryStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"weighted booking entry {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        Version += 1;
    }

    public string Label()
    {
        return $"{"weighted booking entry"} {Code}";
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
        return (double)Version;
    }

    public WeightedBookingEntry Copy()
    {
        return new WeightedBookingEntry(Code, Name, Attributes, Version, Channel, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Channel);
    }
}
