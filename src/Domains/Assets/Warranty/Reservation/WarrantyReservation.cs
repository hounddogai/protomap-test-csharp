namespace Acme.Workforce.Assets.Warranty.Reservation;

using Acme.Workforce.Kit.Text;

/// <summary>Where a warranty reservation is in its life cycle.</summary>
public enum WarrantyReservationStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>A warranty reservation as the warranty team keeps it.</summary>
public sealed class WarrantyReservation(string code, string title, int limit, Dictionary<string, string> attributes, int attempts, double threshold, double maximum, int tenureMonths, string badge, string managerId, string location, WarrantyReservationStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public int Limit { get; set; } = limit;
    public Dictionary<string, string> Attributes { get; set; } = attributes;
    public int Attempts { get; set; } = attempts;
    public double Threshold { get; set; } = threshold;
    public double Maximum { get; set; } = maximum;
    public int TenureMonths { get; set; } = tenureMonths;
    public string Badge { get; set; } = badge;
    public string ManagerId { get; set; } = managerId;
    public string Location { get; set; } = location;
    public WarrantyReservationStatus Status { get; set; } = status;

    /// <summary>Starts a warranty reservation in status requested.</summary>
    public static WarrantyReservation NewWarrantyReservation(string code, string title)
    {
        return new WarrantyReservation(
            code,
            title,
            0,
            new Dictionary<string, string>(),
            0,
            0.0,
            0.0,
            0,
            "",
            "",
            "",
            WarrantyReservationStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case WarrantyReservationStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(WarrantyReservationStatus next)
    {
        switch (Status)
        {
            case WarrantyReservationStatus.Requested:
                switch (next)
                {
                    case WarrantyReservationStatus.Revoked:
                    case WarrantyReservationStatus.Expired:
                        return true;
                }
                return false;
            case WarrantyReservationStatus.Granted:
                switch (next)
                {
                    case WarrantyReservationStatus.Revoked:
                        return true;
                }
                return false;
            case WarrantyReservationStatus.Revoked:
                return next == WarrantyReservationStatus.Expired;
        }
        return false;
    }

    public void MoveTo(WarrantyReservationStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"warranty reservation {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} / {Title}";
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

    public double ProjectedTotal()
    {
        return Math.Round(Threshold + (double)TenureMonths, 2);
    }

    public WarrantyReservation Copy()
    {
        return new WarrantyReservation(
            Code,
            Title,
            Limit,
            Attributes,
            Attempts,
            Threshold,
            Maximum,
            TenureMonths,
            Badge,
            ManagerId,
            Location,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
