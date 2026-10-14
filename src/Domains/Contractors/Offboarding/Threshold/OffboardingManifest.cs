namespace Acme.Workforce.Contractors.Offboarding.Threshold;

using Acme.Workforce.Kit.Text;

/// <summary>Where a offboarding manifest is in its life cycle.</summary>
public enum OffboardingManifestStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>A offboarding manifest as the offboarding team keeps it.</summary>
public sealed class OffboardingManifest(string code, string title, double maximum, string unit, int expiresAt, OffboardingManifestStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public double Maximum { get; set; } = maximum;
    public string Unit { get; set; } = unit;
    public int ExpiresAt { get; set; } = expiresAt;
    public OffboardingManifestStatus Status { get; set; } = status;

    /// <summary>Starts a offboarding manifest in status planned.</summary>
    public static OffboardingManifest NewOffboardingManifest(string code, string title)
    {
        return new OffboardingManifest(code, title, 0.0, "", 0, OffboardingManifestStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case OffboardingManifestStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(OffboardingManifestStatus next)
    {
        switch (Status)
        {
            case OffboardingManifestStatus.Planned:
                switch (next)
                {
                    case OffboardingManifestStatus.InProgress:
                    case OffboardingManifestStatus.Cancelled:
                        return true;
                }
                return false;
            case OffboardingManifestStatus.Scheduled:
                switch (next)
                {
                    case OffboardingManifestStatus.InProgress:
                    case OffboardingManifestStatus.Cancelled:
                        return true;
                }
                return false;
            case OffboardingManifestStatus.InProgress:
                switch (next)
                {
                    case OffboardingManifestStatus.Completed:
                    case OffboardingManifestStatus.Cancelled:
                        return true;
                }
                return false;
            case OffboardingManifestStatus.Completed:
                return next == OffboardingManifestStatus.Cancelled;
        }
        return false;
    }

    public void MoveTo(OffboardingManifestStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"offboarding manifest {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        ExpiresAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code} [{Title}]";
    }

    public double WeightedExposure()
    {
        return Math.Round(Maximum * (double)ExpiresAt, 2);
    }

    public OffboardingManifest Copy()
    {
        return new OffboardingManifest(Code, Title, Maximum, Unit, ExpiresAt, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Unit);
    }
}
