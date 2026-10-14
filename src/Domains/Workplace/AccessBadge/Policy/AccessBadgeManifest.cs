namespace Acme.Workforce.Workplace.AccessBadge.Policy;

using Acme.Workforce.Kit.Text;

/// <summary>Where a access badge manifest is in its life cycle.</summary>
public enum AccessBadgeManifestStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>A access badge manifest as the access badge team keeps it.</summary>
public sealed class AccessBadgeManifest(string code, string label, string salaryBand, string unit, string title, string description, double score, AccessBadgeManifestStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string SalaryBand { get; set; } = salaryBand;
    public string Unit { get; set; } = unit;
    public string Title { get; set; } = title;
    public string Description { get; set; } = description;
    public double Score { get; set; } = score;
    public AccessBadgeManifestStatus Status { get; set; } = status;

    /// <summary>Starts a access badge manifest in status requested.</summary>
    public static AccessBadgeManifest NewAccessBadgeManifest(string code, string label)
    {
        return new AccessBadgeManifest(code, label, "", "", "", "", 0.0, AccessBadgeManifestStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case AccessBadgeManifestStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(AccessBadgeManifestStatus next)
    {
        switch (Status)
        {
            case AccessBadgeManifestStatus.Requested:
                return next == AccessBadgeManifestStatus.Revoked || next == AccessBadgeManifestStatus.Expired;
            case AccessBadgeManifestStatus.Granted:
                return next == AccessBadgeManifestStatus.Revoked || next == AccessBadgeManifestStatus.Expired;
            case AccessBadgeManifestStatus.Revoked:
                switch (next)
                {
                    case AccessBadgeManifestStatus.Expired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(AccessBadgeManifestStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"access badge manifest {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{"access badge manifest"} {Code}";
    }

    public double MeasuredValue()
    {
        return Score;
    }

    public AccessBadgeManifest Copy()
    {
        return new AccessBadgeManifest(Code, Label, SalaryBand, Unit, Title, Description, Score, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Unit);
    }
}
