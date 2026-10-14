namespace Acme.Workforce.Assets.Warranty.Quota;

using Acme.Workforce.Kit.Text;

/// <summary>Where a strict warranty manifest is in its life cycle.</summary>
public enum StrictWarrantyManifestStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>One strict warranty manifest, with what the planners need to act on it.</summary>
public sealed class StrictWarrantyManifest(string code, string title, double rating, string description, string location, string phone, string contactEmail, int updatedAt, string channel, string checksum, StrictWarrantyManifestStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public double Rating { get; set; } = rating;
    public string Description { get; set; } = description;
    public string Location { get; set; } = location;
    public string Phone { get; set; } = phone;
    public string ContactEmail { get; set; } = contactEmail;
    public int UpdatedAt { get; set; } = updatedAt;
    public string Channel { get; set; } = channel;
    public string Checksum { get; set; } = checksum;
    public StrictWarrantyManifestStatus Status { get; set; } = status;

    /// <summary>Starts a strict warranty manifest in status idle.</summary>
    public static StrictWarrantyManifest NewStrictWarrantyManifest(string code, string title)
    {
        return new StrictWarrantyManifest(
            code,
            title,
            0.0,
            "",
            "",
            "",
            "",
            0,
            "",
            "",
            StrictWarrantyManifestStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case StrictWarrantyManifestStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(StrictWarrantyManifestStatus next)
    {
        switch (Status)
        {
            case StrictWarrantyManifestStatus.Idle:
                switch (next)
                {
                    case StrictWarrantyManifestStatus.Warming:
                    case StrictWarrantyManifestStatus.Stopped:
                        return true;
                }
                return false;
            case StrictWarrantyManifestStatus.Warming:
                switch (next)
                {
                    case StrictWarrantyManifestStatus.Ready:
                    case StrictWarrantyManifestStatus.Stopped:
                        return true;
                }
                return false;
            case StrictWarrantyManifestStatus.Ready:
                switch (next)
                {
                    case StrictWarrantyManifestStatus.Stopped:
                        return true;
                }
                return false;
            case StrictWarrantyManifestStatus.Draining:
                switch (next)
                {
                    case StrictWarrantyManifestStatus.Stopped:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(StrictWarrantyManifestStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"strict warranty manifest {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        UpdatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{"strict warranty manifest"} {Code}";
    }

    public double WeightedExposure()
    {
        return Math.Round(Rating * (double)UpdatedAt, 2);
    }

    public StrictWarrantyManifest Copy()
    {
        return new StrictWarrantyManifest(
            Code,
            Title,
            Rating,
            Description,
            Location,
            Phone,
            ContactEmail,
            UpdatedAt,
            Channel,
            Checksum,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Channel);
    }
}
