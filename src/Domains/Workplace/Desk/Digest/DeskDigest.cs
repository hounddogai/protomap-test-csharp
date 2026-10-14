namespace Acme.Workforce.Workplace.Desk.Digest;

using Acme.Workforce.Kit.Text;

/// <summary>Where a desk digest is in its life cycle.</summary>
public enum DeskDigestStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>The record of a desk digest and its current status.</summary>
public sealed class DeskDigest(string code, string label, bool visible, int revision, double cost, string region, string country, string description, double ceiling, int version, int tenureMonths, DeskDigestStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public bool Visible { get; set; } = visible;
    public int Revision { get; set; } = revision;
    public double Cost { get; set; } = cost;
    public string Region { get; set; } = region;
    public string Country { get; set; } = country;
    public string Description { get; set; } = description;
    public double Ceiling { get; set; } = ceiling;
    public int Version { get; set; } = version;
    public int TenureMonths { get; set; } = tenureMonths;
    public DeskDigestStatus Status { get; set; } = status;

    /// <summary>Starts a desk digest in status planned.</summary>
    public static DeskDigest NewDeskDigest(string code, string label)
    {
        return new DeskDigest(code, label, false, 0, 0.0, "", "", "", 0.0, 0, 0, DeskDigestStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case DeskDigestStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(DeskDigestStatus next)
    {
        switch (Status)
        {
            case DeskDigestStatus.Planned:
                switch (next)
                {
                    case DeskDigestStatus.Completed:
                    case DeskDigestStatus.Cancelled:
                        return true;
                }
                return false;
            case DeskDigestStatus.Scheduled:
                switch (next)
                {
                    case DeskDigestStatus.InProgress:
                    case DeskDigestStatus.Cancelled:
                        return true;
                }
                return false;
            case DeskDigestStatus.InProgress:
                return next == DeskDigestStatus.Cancelled;
            case DeskDigestStatus.Completed:
                return next == DeskDigestStatus.Cancelled;
        }
        return false;
    }

    public void MoveTo(DeskDigestStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"desk digest {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        Revision += 1;
    }

    public string DisplayLabel()
    {
        return $"{Code} / {Label}";
    }

    public double EffectiveExposure()
    {
        return Math.Round(Ceiling * (double)Version, 2);
    }

    public DeskDigest Copy()
    {
        return new DeskDigest(
            Code,
            Label,
            Visible,
            Revision,
            Cost,
            Region,
            Country,
            Description,
            Ceiling,
            Version,
            TenureMonths,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Region);
    }
}
