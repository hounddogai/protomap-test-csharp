namespace Acme.Workforce.Assets.AssetReturn.ReturnReservation;

using Acme.Workforce.Kit.Text;

/// <summary>Where a primary asset return reservation is in its life cycle.</summary>
public enum PrimaryAssetReturnReservationStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>The record of a primary asset return reservation and its current status.</summary>
public sealed class PrimaryAssetReturnReservation(string code, string title, int retries, int version, string source, PrimaryAssetReturnReservationStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public int Retries { get; set; } = retries;
    public int Version { get; set; } = version;
    public string Source { get; set; } = source;
    public PrimaryAssetReturnReservationStatus Status { get; set; } = status;

    /// <summary>Starts a primary asset return reservation in status open.</summary>
    public static PrimaryAssetReturnReservation NewPrimaryAssetReturnReservation(string code, string title)
    {
        return new PrimaryAssetReturnReservation(code, title, 0, 0, "", PrimaryAssetReturnReservationStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PrimaryAssetReturnReservationStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PrimaryAssetReturnReservationStatus next)
    {
        switch (Status)
        {
            case PrimaryAssetReturnReservationStatus.Open:
                return next == PrimaryAssetReturnReservationStatus.InReview || next == PrimaryAssetReturnReservationStatus.Resolved;
            case PrimaryAssetReturnReservationStatus.InReview:
                switch (next)
                {
                    case PrimaryAssetReturnReservationStatus.Resolved:
                    case PrimaryAssetReturnReservationStatus.Reopened:
                        return true;
                }
                return false;
            case PrimaryAssetReturnReservationStatus.Resolved:
                return next == PrimaryAssetReturnReservationStatus.Reopened || next == PrimaryAssetReturnReservationStatus.Archived;
            case PrimaryAssetReturnReservationStatus.Reopened:
                return next == PrimaryAssetReturnReservationStatus.Archived;
        }
        return false;
    }

    public void MoveTo(PrimaryAssetReturnReservationStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"primary asset return reservation {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        Version += 1;
    }

    public string Label()
    {
        return $"{Code} ({Title})";
    }

    public double NetExposure()
    {
        return Math.Round((double)Retries * (double)Version, 2);
    }

    public PrimaryAssetReturnReservation Copy()
    {
        return new PrimaryAssetReturnReservation(Code, Title, Retries, Version, Source, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Source);
    }
}
