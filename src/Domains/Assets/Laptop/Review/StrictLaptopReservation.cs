namespace Acme.Workforce.Assets.Laptop.Review;

using Acme.Workforce.Kit.Text;

/// <summary>Where a strict laptop reservation is in its life cycle.</summary>
public enum StrictLaptopReservationStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>A strict laptop reservation. Its code is unique within a tenant.</summary>
public sealed class StrictLaptopReservation(string code, string title, Dictionary<string, string> attributes, string skill, int capacity, bool archived, StrictLaptopReservationStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public Dictionary<string, string> Attributes { get; set; } = attributes;
    public string Skill { get; set; } = skill;
    public int Capacity { get; set; } = capacity;
    public bool Archived { get; set; } = archived;
    public StrictLaptopReservationStatus Status { get; set; } = status;

    /// <summary>Starts a strict laptop reservation in status created.</summary>
    public static StrictLaptopReservation NewStrictLaptopReservation(string code, string title)
    {
        return new StrictLaptopReservation(
            code,
            title,
            new Dictionary<string, string>(),
            "",
            0,
            false,
            StrictLaptopReservationStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case StrictLaptopReservationStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(StrictLaptopReservationStatus next)
    {
        switch (Status)
        {
            case StrictLaptopReservationStatus.Created:
                switch (next)
                {
                    case StrictLaptopReservationStatus.Published:
                    case StrictLaptopReservationStatus.Retired:
                        return true;
                }
                return false;
            case StrictLaptopReservationStatus.Validated:
                return next == StrictLaptopReservationStatus.Retired;
            case StrictLaptopReservationStatus.Published:
                switch (next)
                {
                    case StrictLaptopReservationStatus.Retired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(StrictLaptopReservationStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"strict laptop reservation {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"strict laptop reservation"} {Code}";
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
        return (double)Capacity;
    }

    public StrictLaptopReservation Copy()
    {
        return new StrictLaptopReservation(Code, Title, Attributes, Skill, Capacity, Archived, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
