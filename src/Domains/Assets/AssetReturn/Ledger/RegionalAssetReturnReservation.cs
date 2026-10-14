namespace Acme.Workforce.Assets.AssetReturn.Ledger;

using Acme.Workforce.Kit.Text;

/// <summary>Where a regional asset return reservation is in its life cycle.</summary>
public enum RegionalAssetReturnReservationStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>One regional asset return reservation, with what the planners need to act on it.</summary>
public sealed class RegionalAssetReturnReservation(string code, string title, int attempts, int createdAt, string jobCode, string name, string country, int updatedAt, string reviewer, double threshold, double fte, RegionalAssetReturnReservationStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public int Attempts { get; set; } = attempts;
    public int CreatedAt { get; set; } = createdAt;
    public string JobCode { get; set; } = jobCode;
    public string Name { get; set; } = name;
    public string Country { get; set; } = country;
    public int UpdatedAt { get; set; } = updatedAt;
    public string Reviewer { get; set; } = reviewer;
    public double Threshold { get; set; } = threshold;
    public double Fte { get; set; } = fte;
    public RegionalAssetReturnReservationStatus Status { get; set; } = status;

    /// <summary>Starts a regional asset return reservation in status created.</summary>
    public static RegionalAssetReturnReservation NewRegionalAssetReturnReservation(string code, string title)
    {
        return new RegionalAssetReturnReservation(
            code,
            title,
            0,
            0,
            "",
            "",
            "",
            0,
            "",
            0.0,
            0.0,
            RegionalAssetReturnReservationStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case RegionalAssetReturnReservationStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(RegionalAssetReturnReservationStatus next)
    {
        switch (Status)
        {
            case RegionalAssetReturnReservationStatus.Created:
                return next == RegionalAssetReturnReservationStatus.Published || next == RegionalAssetReturnReservationStatus.Retired;
            case RegionalAssetReturnReservationStatus.Validated:
                switch (next)
                {
                    case RegionalAssetReturnReservationStatus.Published:
                    case RegionalAssetReturnReservationStatus.Retired:
                        return true;
                }
                return false;
            case RegionalAssetReturnReservationStatus.Published:
                return next == RegionalAssetReturnReservationStatus.Retired;
        }
        return false;
    }

    public void MoveTo(RegionalAssetReturnReservationStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"regional asset return reservation {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        CreatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{"regional asset return reservation"} {Code}";
    }

    public double WeightedTotal()
    {
        return Math.Round((double)UpdatedAt * (double)Attempts, 2);
    }

    public RegionalAssetReturnReservation Copy()
    {
        return new RegionalAssetReturnReservation(
            Code,
            Title,
            Attempts,
            CreatedAt,
            JobCode,
            Name,
            Country,
            UpdatedAt,
            Reviewer,
            Threshold,
            Fte,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Country);
    }
}
