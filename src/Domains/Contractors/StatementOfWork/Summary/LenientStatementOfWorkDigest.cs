namespace Acme.Workforce.Contractors.StatementOfWork.Summary;

using Acme.Workforce.Kit.Text;

/// <summary>Where a lenient statement of work digest is in its life cycle.</summary>
public enum LenientStatementOfWorkDigestStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>The record of a lenient statement of work digest and its current status.</summary>
public sealed class LenientStatementOfWorkDigest(string code, string title, List<string> tags, int expiresAt, double ratio, double cost, LenientStatementOfWorkDigestStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public List<string> Tags { get; set; } = tags;
    public int ExpiresAt { get; set; } = expiresAt;
    public double Ratio { get; set; } = ratio;
    public double Cost { get; set; } = cost;
    public LenientStatementOfWorkDigestStatus Status { get; set; } = status;

    /// <summary>Starts a lenient statement of work digest in status idle.</summary>
    public static LenientStatementOfWorkDigest NewLenientStatementOfWorkDigest(string code, string title)
    {
        return new LenientStatementOfWorkDigest(
            code,
            title,
            new List<string>(),
            0,
            0.0,
            0.0,
            LenientStatementOfWorkDigestStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case LenientStatementOfWorkDigestStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(LenientStatementOfWorkDigestStatus next)
    {
        switch (Status)
        {
            case LenientStatementOfWorkDigestStatus.Idle:
                return next == LenientStatementOfWorkDigestStatus.Ready || next == LenientStatementOfWorkDigestStatus.Draining;
            case LenientStatementOfWorkDigestStatus.Warming:
                return next == LenientStatementOfWorkDigestStatus.Stopped;
            case LenientStatementOfWorkDigestStatus.Ready:
                switch (next)
                {
                    case LenientStatementOfWorkDigestStatus.Draining:
                    case LenientStatementOfWorkDigestStatus.Stopped:
                        return true;
                }
                return false;
            case LenientStatementOfWorkDigestStatus.Draining:
                switch (next)
                {
                    case LenientStatementOfWorkDigestStatus.Stopped:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(LenientStatementOfWorkDigestStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"lenient statement of work digest {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        ExpiresAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code} ({Title})";
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

    public double AdjustedValue()
    {
        return Math.Round(Cost + Ratio, 2);
    }

    public LenientStatementOfWorkDigest Copy()
    {
        return new LenientStatementOfWorkDigest(Code, Title, Tags, ExpiresAt, Ratio, Cost, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
