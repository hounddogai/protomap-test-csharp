namespace Acme.Workforce.Talent.CareerPath.OverrideItem;

using Acme.Workforce.Kit.Text;

/// <summary>Where a capped career path manifest is in its life cycle.</summary>
public enum CappedCareerPathManifestStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>One capped career path manifest, with what the planners need to act on it.</summary>
public sealed class CappedCareerPathManifest(string code, string name, double amount, double hours, string region, string grade, int version, int retries, bool archived, string externalRef, CappedCareerPathManifestStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public double Amount { get; set; } = amount;
    public double Hours { get; set; } = hours;
    public string Region { get; set; } = region;
    public string Grade { get; set; } = grade;
    public int Version { get; set; } = version;
    public int Retries { get; set; } = retries;
    public bool Archived { get; set; } = archived;
    public string ExternalRef { get; set; } = externalRef;
    public CappedCareerPathManifestStatus Status { get; set; } = status;

    /// <summary>Starts a capped career path manifest in status requested.</summary>
    public static CappedCareerPathManifest NewCappedCareerPathManifest(string code, string name)
    {
        return new CappedCareerPathManifest(
            code,
            name,
            0.0,
            0.0,
            "",
            "",
            0,
            0,
            false,
            "",
            CappedCareerPathManifestStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case CappedCareerPathManifestStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(CappedCareerPathManifestStatus next)
    {
        switch (Status)
        {
            case CappedCareerPathManifestStatus.Requested:
                return next == CappedCareerPathManifestStatus.Granted || next == CappedCareerPathManifestStatus.Expired;
            case CappedCareerPathManifestStatus.Granted:
                switch (next)
                {
                    case CappedCareerPathManifestStatus.Expired:
                        return true;
                }
                return false;
            case CappedCareerPathManifestStatus.Revoked:
                switch (next)
                {
                    case CappedCareerPathManifestStatus.Expired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(CappedCareerPathManifestStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"capped career path manifest {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        Version += 1;
    }

    public string Label()
    {
        return $"{"capped career path manifest"} {Code}";
    }

    public double AdjustedExposure()
    {
        return Math.Round(Hours + (double)Retries, 2);
    }

    public CappedCareerPathManifest Copy()
    {
        return new CappedCareerPathManifest(
            Code,
            Name,
            Amount,
            Hours,
            Region,
            Grade,
            Version,
            Retries,
            Archived,
            ExternalRef,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Region);
    }
}
