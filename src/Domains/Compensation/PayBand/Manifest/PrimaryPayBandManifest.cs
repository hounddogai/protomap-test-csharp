namespace Acme.Workforce.Compensation.PayBand.Manifest;

using Acme.Workforce.Kit.Text;

/// <summary>Where a primary pay band manifest is in its life cycle.</summary>
public enum PrimaryPayBandManifestStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>One primary pay band manifest, with what the planners need to act on it.</summary>
public sealed class PrimaryPayBandManifest(string code, string title, string checksum, string badge, bool pinned, bool approved, string location, int quantity, PrimaryPayBandManifestStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string Checksum { get; set; } = checksum;
    public string Badge { get; set; } = badge;
    public bool Pinned { get; set; } = pinned;
    public bool Approved { get; set; } = approved;
    public string Location { get; set; } = location;
    public int Quantity { get; set; } = quantity;
    public PrimaryPayBandManifestStatus Status { get; set; } = status;

    /// <summary>Starts a primary pay band manifest in status idle.</summary>
    public static PrimaryPayBandManifest NewPrimaryPayBandManifest(string code, string title)
    {
        return new PrimaryPayBandManifest(
            code,
            title,
            "",
            "",
            false,
            false,
            "",
            0,
            PrimaryPayBandManifestStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PrimaryPayBandManifestStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PrimaryPayBandManifestStatus next)
    {
        switch (Status)
        {
            case PrimaryPayBandManifestStatus.Idle:
                return next == PrimaryPayBandManifestStatus.Draining;
            case PrimaryPayBandManifestStatus.Warming:
                switch (next)
                {
                    case PrimaryPayBandManifestStatus.Ready:
                    case PrimaryPayBandManifestStatus.Stopped:
                        return true;
                }
                return false;
            case PrimaryPayBandManifestStatus.Ready:
                switch (next)
                {
                    case PrimaryPayBandManifestStatus.Draining:
                    case PrimaryPayBandManifestStatus.Stopped:
                        return true;
                }
                return false;
            case PrimaryPayBandManifestStatus.Draining:
                switch (next)
                {
                    case PrimaryPayBandManifestStatus.Stopped:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(PrimaryPayBandManifestStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"primary pay band manifest {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"primary pay band manifest"} {Code}";
    }

    public double MeasuredValue()
    {
        return (double)Quantity;
    }

    public PrimaryPayBandManifest Copy()
    {
        return new PrimaryPayBandManifest(
            Code,
            Title,
            Checksum,
            Badge,
            Pinned,
            Approved,
            Location,
            Quantity,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
