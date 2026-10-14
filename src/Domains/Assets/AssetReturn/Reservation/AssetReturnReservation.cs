namespace Acme.Workforce.Assets.AssetReturn.Reservation;

using Acme.Workforce.Kit.Text;

/// <summary>Where a asset return reservation is in its life cycle.</summary>
public enum AssetReturnReservationStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>One asset return reservation, with what the planners need to act on it.</summary>
public sealed class AssetReturnReservation(string code, string label, string department, bool archived, int windowDays, string name, double fte, string country, string unit, AssetReturnReservationStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Department { get; set; } = department;
    public bool Archived { get; set; } = archived;
    public int WindowDays { get; set; } = windowDays;
    public string Name { get; set; } = name;
    public double Fte { get; set; } = fte;
    public string Country { get; set; } = country;
    public string Unit { get; set; } = unit;
    public AssetReturnReservationStatus Status { get; set; } = status;

    /// <summary>Starts a asset return reservation in status open.</summary>
    public static AssetReturnReservation NewAssetReturnReservation(string code, string label)
    {
        return new AssetReturnReservation(
            code,
            label,
            "",
            false,
            0,
            "",
            0.0,
            "",
            "",
            AssetReturnReservationStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case AssetReturnReservationStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(AssetReturnReservationStatus next)
    {
        switch (Status)
        {
            case AssetReturnReservationStatus.Open:
                switch (next)
                {
                    case AssetReturnReservationStatus.InReview:
                    case AssetReturnReservationStatus.Resolved:
                        return true;
                }
                return false;
            case AssetReturnReservationStatus.InReview:
                switch (next)
                {
                    case AssetReturnReservationStatus.Resolved:
                        return true;
                }
                return false;
            case AssetReturnReservationStatus.Resolved:
                return next == AssetReturnReservationStatus.Reopened || next == AssetReturnReservationStatus.Archived;
            case AssetReturnReservationStatus.Reopened:
                switch (next)
                {
                    case AssetReturnReservationStatus.Archived:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(AssetReturnReservationStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"asset return reservation {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code} [{Label}]";
    }

    public double ProjectedValue()
    {
        return Math.Round(Fte + (double)WindowDays, 2);
    }

    public AssetReturnReservation Copy()
    {
        return new AssetReturnReservation(
            Code,
            Label,
            Department,
            Archived,
            WindowDays,
            Name,
            Fte,
            Country,
            Unit,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Department);
    }
}
