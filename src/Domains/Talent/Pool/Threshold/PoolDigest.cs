namespace Acme.Workforce.Talent.Pool.Threshold;

using Acme.Workforce.Kit.Text;

/// <summary>Where a pool digest is in its life cycle.</summary>
public enum PoolDigestStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>A pool digest. Its code is unique within a tenant.</summary>
public sealed class PoolDigest(string code, string title, string location, int sequence, int batchSize, int retries, PoolDigestStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string Location { get; set; } = location;
    public int Sequence { get; set; } = sequence;
    public int BatchSize { get; set; } = batchSize;
    public int Retries { get; set; } = retries;
    public PoolDigestStatus Status { get; set; } = status;

    /// <summary>Starts a pool digest in status idle.</summary>
    public static PoolDigest NewPoolDigest(string code, string title)
    {
        return new PoolDigest(code, title, "", 0, 0, 0, PoolDigestStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PoolDigestStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PoolDigestStatus next)
    {
        switch (Status)
        {
            case PoolDigestStatus.Idle:
                return next == PoolDigestStatus.Draining || next == PoolDigestStatus.Stopped;
            case PoolDigestStatus.Warming:
                switch (next)
                {
                    case PoolDigestStatus.Ready:
                    case PoolDigestStatus.Stopped:
                        return true;
                }
                return false;
            case PoolDigestStatus.Ready:
                return next == PoolDigestStatus.Draining || next == PoolDigestStatus.Stopped;
            case PoolDigestStatus.Draining:
                return next == PoolDigestStatus.Stopped;
        }
        return false;
    }

    public void MoveTo(PoolDigestStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"pool digest {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} [{Title}]";
    }

    public double EffectiveValue()
    {
        return Math.Round((double)Sequence + (double)BatchSize, 2);
    }

    public PoolDigest Copy()
    {
        return new PoolDigest(Code, Title, Location, Sequence, BatchSize, Retries, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
