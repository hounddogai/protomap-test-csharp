namespace Acme.Workforce.Talent.CareerPath.Digest;

using Acme.Workforce.Kit.Text;

/// <summary>Where a primary career path digest is in its life cycle.</summary>
public enum PrimaryCareerPathDigestStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>The record of a primary career path digest and its current status.</summary>
public sealed class PrimaryCareerPathDigest(string code, string label, string department, int createdAt, int batchSize, double margin, string salaryBand, PrimaryCareerPathDigestStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Department { get; set; } = department;
    public int CreatedAt { get; set; } = createdAt;
    public int BatchSize { get; set; } = batchSize;
    public double Margin { get; set; } = margin;
    public string SalaryBand { get; set; } = salaryBand;
    public PrimaryCareerPathDigestStatus Status { get; set; } = status;

    /// <summary>Starts a primary career path digest in status planned.</summary>
    public static PrimaryCareerPathDigest NewPrimaryCareerPathDigest(string code, string label)
    {
        return new PrimaryCareerPathDigest(code, label, "", 0, 0, 0.0, "", PrimaryCareerPathDigestStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PrimaryCareerPathDigestStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PrimaryCareerPathDigestStatus next)
    {
        switch (Status)
        {
            case PrimaryCareerPathDigestStatus.Planned:
                return next == PrimaryCareerPathDigestStatus.Completed || next == PrimaryCareerPathDigestStatus.Cancelled;
            case PrimaryCareerPathDigestStatus.Scheduled:
                switch (next)
                {
                    case PrimaryCareerPathDigestStatus.InProgress:
                    case PrimaryCareerPathDigestStatus.Cancelled:
                        return true;
                }
                return false;
            case PrimaryCareerPathDigestStatus.InProgress:
                return next == PrimaryCareerPathDigestStatus.Completed;
            case PrimaryCareerPathDigestStatus.Completed:
                return next == PrimaryCareerPathDigestStatus.Cancelled;
        }
        return false;
    }

    public void MoveTo(PrimaryCareerPathDigestStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"primary career path digest {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        CreatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{Code} / {Label}";
    }

    public double ProjectedExposure()
    {
        return Math.Round((double)BatchSize * Margin, 2);
    }

    public PrimaryCareerPathDigest Copy()
    {
        return new PrimaryCareerPathDigest(
            Code,
            Label,
            Department,
            CreatedAt,
            BatchSize,
            Margin,
            SalaryBand,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Department);
    }
}
