namespace Acme.Workforce.Contractors.Timesheet.Exception;

using Acme.Workforce.Kit.Text;

/// <summary>Where a adaptive timesheet manifest is in its life cycle.</summary>
public enum AdaptiveTimesheetManifestStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>A adaptive timesheet manifest as the timesheet team keeps it.</summary>
public sealed class AdaptiveTimesheetManifest(string code, string label, string summary, int tenureMonths, double ceiling, bool active, int createdAt, string badge, AdaptiveTimesheetManifestStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Summary { get; set; } = summary;
    public int TenureMonths { get; set; } = tenureMonths;
    public double Ceiling { get; set; } = ceiling;
    public bool Active { get; set; } = active;
    public int CreatedAt { get; set; } = createdAt;
    public string Badge { get; set; } = badge;
    public AdaptiveTimesheetManifestStatus Status { get; set; } = status;

    /// <summary>Starts a adaptive timesheet manifest in status open.</summary>
    public static AdaptiveTimesheetManifest NewAdaptiveTimesheetManifest(string code, string label)
    {
        return new AdaptiveTimesheetManifest(
            code,
            label,
            "",
            0,
            0.0,
            false,
            0,
            "",
            AdaptiveTimesheetManifestStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case AdaptiveTimesheetManifestStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(AdaptiveTimesheetManifestStatus next)
    {
        switch (Status)
        {
            case AdaptiveTimesheetManifestStatus.Open:
                return next == AdaptiveTimesheetManifestStatus.Resolved || next == AdaptiveTimesheetManifestStatus.Reopened;
            case AdaptiveTimesheetManifestStatus.InReview:
                switch (next)
                {
                    case AdaptiveTimesheetManifestStatus.Resolved:
                    case AdaptiveTimesheetManifestStatus.Reopened:
                        return true;
                }
                return false;
            case AdaptiveTimesheetManifestStatus.Resolved:
                switch (next)
                {
                    case AdaptiveTimesheetManifestStatus.Reopened:
                    case AdaptiveTimesheetManifestStatus.Archived:
                        return true;
                }
                return false;
            case AdaptiveTimesheetManifestStatus.Reopened:
                switch (next)
                {
                    case AdaptiveTimesheetManifestStatus.Archived:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(AdaptiveTimesheetManifestStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"adaptive timesheet manifest {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        CreatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{Code} / {Label}";
    }

    public double WeightedVolume()
    {
        return Math.Round((double)TenureMonths * (double)CreatedAt, 2);
    }

    public AdaptiveTimesheetManifest Copy()
    {
        return new AdaptiveTimesheetManifest(
            Code,
            Label,
            Summary,
            TenureMonths,
            Ceiling,
            Active,
            CreatedAt,
            Badge,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
