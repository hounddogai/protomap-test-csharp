namespace Acme.Workforce.Workplace.AccessBadge.Profile;

using Acme.Workforce.Kit.Text;

/// <summary>Where a fallback access badge agreement is in its life cycle.</summary>
public enum FallbackAccessBadgeAgreementStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>A fallback access badge agreement as the access badge team keeps it.</summary>
public sealed class FallbackAccessBadgeAgreement(string code, string title, string unit, bool pinned, double hours, int expiresAt, string name, double margin, bool archived, string employeeId, FallbackAccessBadgeAgreementStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string Unit { get; set; } = unit;
    public bool Pinned { get; set; } = pinned;
    public double Hours { get; set; } = hours;
    public int ExpiresAt { get; set; } = expiresAt;
    public string Name { get; set; } = name;
    public double Margin { get; set; } = margin;
    public bool Archived { get; set; } = archived;
    public string EmployeeId { get; set; } = employeeId;
    public FallbackAccessBadgeAgreementStatus Status { get; set; } = status;

    /// <summary>Starts a fallback access badge agreement in status new.</summary>
    public static FallbackAccessBadgeAgreement NewFallbackAccessBadgeAgreement(string code, string title)
    {
        return new FallbackAccessBadgeAgreement(
            code,
            title,
            "",
            false,
            0.0,
            0,
            "",
            0.0,
            false,
            "",
            FallbackAccessBadgeAgreementStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case FallbackAccessBadgeAgreementStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(FallbackAccessBadgeAgreementStatus next)
    {
        switch (Status)
        {
            case FallbackAccessBadgeAgreementStatus.New:
                switch (next)
                {
                    case FallbackAccessBadgeAgreementStatus.Failed:
                        return true;
                }
                return false;
            case FallbackAccessBadgeAgreementStatus.Queued:
                return next == FallbackAccessBadgeAgreementStatus.Failed;
            case FallbackAccessBadgeAgreementStatus.Running:
                switch (next)
                {
                    case FallbackAccessBadgeAgreementStatus.Failed:
                        return true;
                }
                return false;
            case FallbackAccessBadgeAgreementStatus.Done:
                switch (next)
                {
                    case FallbackAccessBadgeAgreementStatus.Failed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(FallbackAccessBadgeAgreementStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"fallback access badge agreement {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        ExpiresAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{"fallback access badge agreement"} {Code}";
    }

    public double ProjectedExposure()
    {
        return Math.Round(Margin - (double)ExpiresAt, 2);
    }

    public FallbackAccessBadgeAgreement Copy()
    {
        return new FallbackAccessBadgeAgreement(
            Code,
            Title,
            Unit,
            Pinned,
            Hours,
            ExpiresAt,
            Name,
            Margin,
            Archived,
            EmployeeId,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Unit);
    }
}
