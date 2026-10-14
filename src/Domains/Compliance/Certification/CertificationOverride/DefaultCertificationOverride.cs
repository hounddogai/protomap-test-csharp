namespace Acme.Workforce.Compliance.Certification.CertificationOverride;

using Acme.Workforce.Kit.Text;

/// <summary>Where a default certification override is in its life cycle.</summary>
public enum DefaultCertificationOverrideStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>One default certification override, with what the planners need to act on it.</summary>
public sealed class DefaultCertificationOverride(string code, string title, string badge, double rating, bool approved, double cost, DefaultCertificationOverrideStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string Badge { get; set; } = badge;
    public double Rating { get; set; } = rating;
    public bool Approved { get; set; } = approved;
    public double Cost { get; set; } = cost;
    public DefaultCertificationOverrideStatus Status { get; set; } = status;

    /// <summary>Starts a default certification override in status proposed.</summary>
    public static DefaultCertificationOverride NewDefaultCertificationOverride(string code, string title)
    {
        return new DefaultCertificationOverride(
            code,
            title,
            "",
            0.0,
            false,
            0.0,
            DefaultCertificationOverrideStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case DefaultCertificationOverrideStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(DefaultCertificationOverrideStatus next)
    {
        switch (Status)
        {
            case DefaultCertificationOverrideStatus.Proposed:
                switch (next)
                {
                    case DefaultCertificationOverrideStatus.Approved:
                        return true;
                }
                return false;
            case DefaultCertificationOverrideStatus.Approved:
                switch (next)
                {
                    case DefaultCertificationOverrideStatus.Rejected:
                    case DefaultCertificationOverrideStatus.Withdrawn:
                        return true;
                }
                return false;
            case DefaultCertificationOverrideStatus.Rejected:
                switch (next)
                {
                    case DefaultCertificationOverrideStatus.Withdrawn:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(DefaultCertificationOverrideStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"default certification override {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} / {Title}";
    }

    public double ProjectedVolume()
    {
        return Math.Round(Cost - Rating, 2);
    }

    public DefaultCertificationOverride Copy()
    {
        return new DefaultCertificationOverride(Code, Title, Badge, Rating, Approved, Cost, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
