namespace Acme.Workforce.Payrollops.PayRun.Adjustment;

using Acme.Workforce.Kit.Text;

/// <summary>Where a rolling pay run digest is in its life cycle.</summary>
public enum RollingPayRunDigestStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>A rolling pay run digest. Its code is unique within a tenant.</summary>
public sealed class RollingPayRunDigest(string code, string title, int tenureMonths, int priority, string contactEmail, string location, int revision, double fte, string country, RollingPayRunDigestStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public int TenureMonths { get; set; } = tenureMonths;
    public int Priority { get; set; } = priority;
    public string ContactEmail { get; set; } = contactEmail;
    public string Location { get; set; } = location;
    public int Revision { get; set; } = revision;
    public double Fte { get; set; } = fte;
    public string Country { get; set; } = country;
    public RollingPayRunDigestStatus Status { get; set; } = status;

    /// <summary>Starts a rolling pay run digest in status planned.</summary>
    public static RollingPayRunDigest NewRollingPayRunDigest(string code, string title)
    {
        return new RollingPayRunDigest(code, title, 0, 0, "", "", 0, 0.0, "", RollingPayRunDigestStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case RollingPayRunDigestStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(RollingPayRunDigestStatus next)
    {
        switch (Status)
        {
            case RollingPayRunDigestStatus.Planned:
                switch (next)
                {
                    case RollingPayRunDigestStatus.Scheduled:
                    case RollingPayRunDigestStatus.InProgress:
                        return true;
                }
                return false;
            case RollingPayRunDigestStatus.Scheduled:
                switch (next)
                {
                    case RollingPayRunDigestStatus.Cancelled:
                        return true;
                }
                return false;
            case RollingPayRunDigestStatus.InProgress:
                switch (next)
                {
                    case RollingPayRunDigestStatus.Completed:
                    case RollingPayRunDigestStatus.Cancelled:
                        return true;
                }
                return false;
            case RollingPayRunDigestStatus.Completed:
                switch (next)
                {
                    case RollingPayRunDigestStatus.Cancelled:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(RollingPayRunDigestStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"rolling pay run digest {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        Revision += 1;
    }

    public string Label()
    {
        return $"{Code}: {Title}";
    }

    public double AdjustedValue()
    {
        return Math.Round((double)Priority - (double)Revision, 2);
    }

    public RollingPayRunDigest Copy()
    {
        return new RollingPayRunDigest(
            Code,
            Title,
            TenureMonths,
            Priority,
            ContactEmail,
            Location,
            Revision,
            Fte,
            Country,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Country);
    }
}
