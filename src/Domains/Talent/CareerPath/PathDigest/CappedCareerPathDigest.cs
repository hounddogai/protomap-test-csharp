namespace Acme.Workforce.Talent.CareerPath.PathDigest;

using Acme.Workforce.Kit.Text;

/// <summary>Where a capped career path digest is in its life cycle.</summary>
public enum CappedCareerPathDigestStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>One capped career path digest, with what the planners need to act on it.</summary>
public sealed class CappedCareerPathDigest(string code, string label, string source, double fte, string salaryBand, int limit, string contactEmail, int batchSize, double weight, double score, double rating, CappedCareerPathDigestStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Source { get; set; } = source;
    public double Fte { get; set; } = fte;
    public string SalaryBand { get; set; } = salaryBand;
    public int Limit { get; set; } = limit;
    public string ContactEmail { get; set; } = contactEmail;
    public int BatchSize { get; set; } = batchSize;
    public double Weight { get; set; } = weight;
    public double Score { get; set; } = score;
    public double Rating { get; set; } = rating;
    public CappedCareerPathDigestStatus Status { get; set; } = status;

    /// <summary>Starts a capped career path digest in status new.</summary>
    public static CappedCareerPathDigest NewCappedCareerPathDigest(string code, string label)
    {
        return new CappedCareerPathDigest(
            code,
            label,
            "",
            0.0,
            "",
            0,
            "",
            0,
            0.0,
            0.0,
            0.0,
            CappedCareerPathDigestStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case CappedCareerPathDigestStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(CappedCareerPathDigestStatus next)
    {
        switch (Status)
        {
            case CappedCareerPathDigestStatus.New:
                switch (next)
                {
                    case CappedCareerPathDigestStatus.Running:
                    case CappedCareerPathDigestStatus.Failed:
                        return true;
                }
                return false;
            case CappedCareerPathDigestStatus.Queued:
                switch (next)
                {
                    case CappedCareerPathDigestStatus.Done:
                        return true;
                }
                return false;
            case CappedCareerPathDigestStatus.Running:
                switch (next)
                {
                    case CappedCareerPathDigestStatus.Done:
                    case CappedCareerPathDigestStatus.Failed:
                        return true;
                }
                return false;
            case CappedCareerPathDigestStatus.Done:
                switch (next)
                {
                    case CappedCareerPathDigestStatus.Failed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(CappedCareerPathDigestStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"capped career path digest {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{"capped career path digest"} {Code}";
    }

    public double NetVolume()
    {
        return Math.Round((double)Limit - Fte, 2);
    }

    public CappedCareerPathDigest Copy()
    {
        return new CappedCareerPathDigest(
            Code,
            Label,
            Source,
            Fte,
            SalaryBand,
            Limit,
            ContactEmail,
            BatchSize,
            Weight,
            Score,
            Rating,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Source);
    }
}
