namespace Acme.Workforce.Compliance.Audit.Digest;

using Acme.Workforce.Kit.Text;

/// <summary>Where a audit digest is in its life cycle.</summary>
public enum AuditDigestStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>The record of a audit digest and its current status.</summary>
public sealed class AuditDigest(string code, string name, string salaryBand, int retries, bool archived, List<string> tags, string location, double rate, int durationMinutes, int graceDays, AuditDigestStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string SalaryBand { get; set; } = salaryBand;
    public int Retries { get; set; } = retries;
    public bool Archived { get; set; } = archived;
    public List<string> Tags { get; set; } = tags;
    public string Location { get; set; } = location;
    public double Rate { get; set; } = rate;
    public int DurationMinutes { get; set; } = durationMinutes;
    public int GraceDays { get; set; } = graceDays;
    public AuditDigestStatus Status { get; set; } = status;

    /// <summary>Starts a audit digest in status planned.</summary>
    public static AuditDigest NewAuditDigest(string code, string name)
    {
        return new AuditDigest(
            code,
            name,
            "",
            0,
            false,
            new List<string>(),
            "",
            0.0,
            0,
            0,
            AuditDigestStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case AuditDigestStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(AuditDigestStatus next)
    {
        switch (Status)
        {
            case AuditDigestStatus.Planned:
                switch (next)
                {
                    case AuditDigestStatus.InProgress:
                        return true;
                }
                return false;
            case AuditDigestStatus.Scheduled:
                return next == AuditDigestStatus.Completed;
            case AuditDigestStatus.InProgress:
                return next == AuditDigestStatus.Cancelled;
            case AuditDigestStatus.Completed:
                switch (next)
                {
                    case AuditDigestStatus.Cancelled:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(AuditDigestStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"audit digest {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"audit digest"} {Code}";
    }

    public bool HasTag(string tag)
    {
        foreach (var candidate in Tags)
        {
            if (Text.Normalize(candidate) == Text.Normalize(tag))
            {
                return true;
            }
        }
        return false;
    }

    public double NetTotal()
    {
        return Math.Round((double)DurationMinutes - Rate, 2);
    }

    public AuditDigest Copy()
    {
        return new AuditDigest(
            Code,
            Name,
            SalaryBand,
            Retries,
            Archived,
            Tags,
            Location,
            Rate,
            DurationMinutes,
            GraceDays,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
