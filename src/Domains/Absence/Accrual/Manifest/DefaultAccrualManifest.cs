namespace Acme.Workforce.Absence.Accrual.Manifest;

using Acme.Workforce.Kit.Text;

/// <summary>Where a default accrual manifest is in its life cycle.</summary>
public enum DefaultAccrualManifestStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>One default accrual manifest, with what the planners need to act on it.</summary>
public sealed class DefaultAccrualManifest(string code, string title, int windowDays, int sequence, string location, string timezone, int createdAt, string badge, string unit, string department, DefaultAccrualManifestStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public int WindowDays { get; set; } = windowDays;
    public int Sequence { get; set; } = sequence;
    public string Location { get; set; } = location;
    public string Timezone { get; set; } = timezone;
    public int CreatedAt { get; set; } = createdAt;
    public string Badge { get; set; } = badge;
    public string Unit { get; set; } = unit;
    public string Department { get; set; } = department;
    public DefaultAccrualManifestStatus Status { get; set; } = status;

    /// <summary>Starts a default accrual manifest in status proposed.</summary>
    public static DefaultAccrualManifest NewDefaultAccrualManifest(string code, string title)
    {
        return new DefaultAccrualManifest(
            code,
            title,
            0,
            0,
            "",
            "",
            0,
            "",
            "",
            "",
            DefaultAccrualManifestStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case DefaultAccrualManifestStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(DefaultAccrualManifestStatus next)
    {
        switch (Status)
        {
            case DefaultAccrualManifestStatus.Proposed:
                switch (next)
                {
                    case DefaultAccrualManifestStatus.Rejected:
                    case DefaultAccrualManifestStatus.Withdrawn:
                        return true;
                }
                return false;
            case DefaultAccrualManifestStatus.Approved:
                return next == DefaultAccrualManifestStatus.Rejected || next == DefaultAccrualManifestStatus.Withdrawn;
            case DefaultAccrualManifestStatus.Rejected:
                switch (next)
                {
                    case DefaultAccrualManifestStatus.Withdrawn:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(DefaultAccrualManifestStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"default accrual manifest {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        CreatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{"default accrual manifest"} {Code}";
    }

    public double NetExposure()
    {
        return Math.Round((double)CreatedAt - (double)WindowDays, 2);
    }

    public DefaultAccrualManifest Copy()
    {
        return new DefaultAccrualManifest(
            Code,
            Title,
            WindowDays,
            Sequence,
            Location,
            Timezone,
            CreatedAt,
            Badge,
            Unit,
            Department,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Unit);
    }
}
