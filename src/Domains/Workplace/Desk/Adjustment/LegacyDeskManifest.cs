namespace Acme.Workforce.Workplace.Desk.Adjustment;

using Acme.Workforce.Kit.Text;

/// <summary>Where a legacy desk manifest is in its life cycle.</summary>
public enum LegacyDeskManifestStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>A legacy desk manifest as the desk team keeps it.</summary>
public sealed class LegacyDeskManifest(string code, string label, bool pinned, string segment, string country, int expiresAt, double ratio, double hours, int windowDays, string grade, LegacyDeskManifestStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public bool Pinned { get; set; } = pinned;
    public string Segment { get; set; } = segment;
    public string Country { get; set; } = country;
    public int ExpiresAt { get; set; } = expiresAt;
    public double Ratio { get; set; } = ratio;
    public double Hours { get; set; } = hours;
    public int WindowDays { get; set; } = windowDays;
    public string Grade { get; set; } = grade;
    public LegacyDeskManifestStatus Status { get; set; } = status;

    /// <summary>Starts a legacy desk manifest in status planned.</summary>
    public static LegacyDeskManifest NewLegacyDeskManifest(string code, string label)
    {
        return new LegacyDeskManifest(
            code,
            label,
            false,
            "",
            "",
            0,
            0.0,
            0.0,
            0,
            "",
            LegacyDeskManifestStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case LegacyDeskManifestStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(LegacyDeskManifestStatus next)
    {
        switch (Status)
        {
            case LegacyDeskManifestStatus.Planned:
                switch (next)
                {
                    case LegacyDeskManifestStatus.InProgress:
                    case LegacyDeskManifestStatus.Completed:
                        return true;
                }
                return false;
            case LegacyDeskManifestStatus.Scheduled:
                switch (next)
                {
                    case LegacyDeskManifestStatus.InProgress:
                        return true;
                }
                return false;
            case LegacyDeskManifestStatus.InProgress:
                switch (next)
                {
                    case LegacyDeskManifestStatus.Completed:
                        return true;
                }
                return false;
            case LegacyDeskManifestStatus.Completed:
                return next == LegacyDeskManifestStatus.Cancelled;
        }
        return false;
    }

    public void MoveTo(LegacyDeskManifestStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"legacy desk manifest {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        ExpiresAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{Code} ({Label})";
    }

    public double WeightedVolume()
    {
        return Math.Round(Hours * Ratio, 2);
    }

    public LegacyDeskManifest Copy()
    {
        return new LegacyDeskManifest(
            Code,
            Label,
            Pinned,
            Segment,
            Country,
            ExpiresAt,
            Ratio,
            Hours,
            WindowDays,
            Grade,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Segment);
    }
}
