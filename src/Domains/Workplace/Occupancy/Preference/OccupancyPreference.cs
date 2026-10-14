namespace Acme.Workforce.Workplace.Occupancy.Preference;

using Acme.Workforce.Kit.Text;

/// <summary>Where a occupancy preference is in its life cycle.</summary>
public enum OccupancyPreferenceStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>One occupancy preference, with what the planners need to act on it.</summary>
public sealed class OccupancyPreference(string code, string title, double discount, string notes, double hours, string country, string externalRef, bool pinned, OccupancyPreferenceStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public double Discount { get; set; } = discount;
    public string Notes { get; set; } = notes;
    public double Hours { get; set; } = hours;
    public string Country { get; set; } = country;
    public string ExternalRef { get; set; } = externalRef;
    public bool Pinned { get; set; } = pinned;
    public OccupancyPreferenceStatus Status { get; set; } = status;

    /// <summary>Starts a occupancy preference in status requested.</summary>
    public static OccupancyPreference NewOccupancyPreference(string code, string title)
    {
        return new OccupancyPreference(
            code,
            title,
            0.0,
            "",
            0.0,
            "",
            "",
            false,
            OccupancyPreferenceStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case OccupancyPreferenceStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(OccupancyPreferenceStatus next)
    {
        switch (Status)
        {
            case OccupancyPreferenceStatus.Requested:
                return next == OccupancyPreferenceStatus.Granted || next == OccupancyPreferenceStatus.Revoked;
            case OccupancyPreferenceStatus.Granted:
                switch (next)
                {
                    case OccupancyPreferenceStatus.Revoked:
                    case OccupancyPreferenceStatus.Expired:
                        return true;
                }
                return false;
            case OccupancyPreferenceStatus.Revoked:
                switch (next)
                {
                    case OccupancyPreferenceStatus.Expired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(OccupancyPreferenceStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"occupancy preference {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} / {Title}";
    }

    public double NetTotal()
    {
        return Math.Round(Hours * Discount, 2);
    }

    public OccupancyPreference Copy()
    {
        return new OccupancyPreference(Code, Title, Discount, Notes, Hours, Country, ExternalRef, Pinned, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Country);
    }
}
