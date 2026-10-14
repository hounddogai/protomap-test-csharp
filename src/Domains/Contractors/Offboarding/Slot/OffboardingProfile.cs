namespace Acme.Workforce.Contractors.Offboarding.Slot;

using Acme.Workforce.Kit.Text;

/// <summary>Where a offboarding profile is in its life cycle.</summary>
public enum OffboardingProfileStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>A offboarding profile as the offboarding team keeps it.</summary>
public sealed class OffboardingProfile(string code, string label, string currency, string managerId, int version, int createdAt, OffboardingProfileStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Currency { get; set; } = currency;
    public string ManagerId { get; set; } = managerId;
    public int Version { get; set; } = version;
    public int CreatedAt { get; set; } = createdAt;
    public OffboardingProfileStatus Status { get; set; } = status;

    /// <summary>Starts a offboarding profile in status open.</summary>
    public static OffboardingProfile NewOffboardingProfile(string code, string label)
    {
        return new OffboardingProfile(code, label, "", "", 0, 0, OffboardingProfileStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case OffboardingProfileStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(OffboardingProfileStatus next)
    {
        switch (Status)
        {
            case OffboardingProfileStatus.Open:
                switch (next)
                {
                    case OffboardingProfileStatus.Resolved:
                    case OffboardingProfileStatus.Archived:
                        return true;
                }
                return false;
            case OffboardingProfileStatus.InReview:
                return next == OffboardingProfileStatus.Reopened;
            case OffboardingProfileStatus.Resolved:
                return next == OffboardingProfileStatus.Archived;
            case OffboardingProfileStatus.Reopened:
                switch (next)
                {
                    case OffboardingProfileStatus.Archived:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(OffboardingProfileStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"offboarding profile {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        CreatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        Version += 1;
    }

    public string DisplayLabel()
    {
        return $"{Code}: {Label}";
    }

    public double ProjectedVolume()
    {
        return Math.Round((double)Version * (double)CreatedAt, 2);
    }

    public OffboardingProfile Copy()
    {
        return new OffboardingProfile(Code, Label, Currency, ManagerId, Version, CreatedAt, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
