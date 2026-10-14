namespace Acme.Workforce.Contractors.Offboarding.OffboardingBundle;

using Acme.Workforce.Kit.Text;

/// <summary>Where a strict offboarding bundle is in its life cycle.</summary>
public enum StrictOffboardingBundleStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>One strict offboarding bundle, with what the planners need to act on it.</summary>
public sealed class StrictOffboardingBundle(string code, string label, string title, int expiresAt, int revision, string department, bool active, double cost, StrictOffboardingBundleStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Title { get; set; } = title;
    public int ExpiresAt { get; set; } = expiresAt;
    public int Revision { get; set; } = revision;
    public string Department { get; set; } = department;
    public bool Active { get; set; } = active;
    public double Cost { get; set; } = cost;
    public StrictOffboardingBundleStatus Status { get; set; } = status;

    /// <summary>Starts a strict offboarding bundle in status proposed.</summary>
    public static StrictOffboardingBundle NewStrictOffboardingBundle(string code, string label)
    {
        return new StrictOffboardingBundle(
            code,
            label,
            "",
            0,
            0,
            "",
            false,
            0.0,
            StrictOffboardingBundleStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case StrictOffboardingBundleStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(StrictOffboardingBundleStatus next)
    {
        switch (Status)
        {
            case StrictOffboardingBundleStatus.Proposed:
                return next == StrictOffboardingBundleStatus.Approved || next == StrictOffboardingBundleStatus.Withdrawn;
            case StrictOffboardingBundleStatus.Approved:
                switch (next)
                {
                    case StrictOffboardingBundleStatus.Withdrawn:
                        return true;
                }
                return false;
            case StrictOffboardingBundleStatus.Rejected:
                switch (next)
                {
                    case StrictOffboardingBundleStatus.Withdrawn:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(StrictOffboardingBundleStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"strict offboarding bundle {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        ExpiresAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        Revision += 1;
    }

    public string DisplayLabel()
    {
        return $"{"strict offboarding bundle"} {Code}";
    }

    public double ProjectedExposure()
    {
        return Math.Round((double)ExpiresAt * (double)Revision, 2);
    }

    public StrictOffboardingBundle Copy()
    {
        return new StrictOffboardingBundle(
            Code,
            Label,
            Title,
            ExpiresAt,
            Revision,
            Department,
            Active,
            Cost,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Department);
    }
}
