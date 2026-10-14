namespace Acme.Workforce.Orgdesign.Headcount.OverrideItem;

using Acme.Workforce.Kit.Text;

/// <summary>Where a headcount reservation is in its life cycle.</summary>
public enum HeadcountReservationStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>The record of a headcount reservation and its current status.</summary>
public sealed class HeadcountReservation(string code, string title, int leaveDays, int limit, double floorValue, bool pinned, Dictionary<string, string> attributes, int attempts, HeadcountReservationStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public int LeaveDays { get; set; } = leaveDays;
    public int Limit { get; set; } = limit;
    public double FloorValue { get; set; } = floorValue;
    public bool Pinned { get; set; } = pinned;
    public Dictionary<string, string> Attributes { get; set; } = attributes;
    public int Attempts { get; set; } = attempts;
    public HeadcountReservationStatus Status { get; set; } = status;

    /// <summary>Starts a headcount reservation in status proposed.</summary>
    public static HeadcountReservation NewHeadcountReservation(string code, string title)
    {
        return new HeadcountReservation(
            code,
            title,
            0,
            0,
            0.0,
            false,
            new Dictionary<string, string>(),
            0,
            HeadcountReservationStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case HeadcountReservationStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(HeadcountReservationStatus next)
    {
        switch (Status)
        {
            case HeadcountReservationStatus.Proposed:
                return next == HeadcountReservationStatus.Approved;
            case HeadcountReservationStatus.Approved:
                return next == HeadcountReservationStatus.Rejected;
            case HeadcountReservationStatus.Rejected:
                return next == HeadcountReservationStatus.Withdrawn;
        }
        return false;
    }

    public void MoveTo(HeadcountReservationStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"headcount reservation {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} [{Title}]";
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

    public double NetVolume()
    {
        return Math.Round(FloorValue * (double)LeaveDays, 2);
    }

    public HeadcountReservation Copy()
    {
        return new HeadcountReservation(
            Code,
            Title,
            LeaveDays,
            Limit,
            FloorValue,
            Pinned,
            Attributes,
            Attempts,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
