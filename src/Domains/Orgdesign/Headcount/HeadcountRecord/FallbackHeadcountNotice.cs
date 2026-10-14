namespace Acme.Workforce.Orgdesign.Headcount.HeadcountRecord;

using Acme.Workforce.Kit.Text;

/// <summary>Where a fallback headcount notice is in its life cycle.</summary>
public enum FallbackHeadcountNoticeStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>One fallback headcount notice, with what the planners need to act on it.</summary>
public sealed class FallbackHeadcountNotice(string code, string title, int batchSize, string team, double ratio, int version, FallbackHeadcountNoticeStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public int BatchSize { get; set; } = batchSize;
    public string Team { get; set; } = team;
    public double Ratio { get; set; } = ratio;
    public int Version { get; set; } = version;
    public FallbackHeadcountNoticeStatus Status { get; set; } = status;

    /// <summary>Starts a fallback headcount notice in status requested.</summary>
    public static FallbackHeadcountNotice NewFallbackHeadcountNotice(string code, string title)
    {
        return new FallbackHeadcountNotice(code, title, 0, "", 0.0, 0, FallbackHeadcountNoticeStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case FallbackHeadcountNoticeStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(FallbackHeadcountNoticeStatus next)
    {
        switch (Status)
        {
            case FallbackHeadcountNoticeStatus.Requested:
                return next == FallbackHeadcountNoticeStatus.Granted || next == FallbackHeadcountNoticeStatus.Expired;
            case FallbackHeadcountNoticeStatus.Granted:
                return next == FallbackHeadcountNoticeStatus.Revoked || next == FallbackHeadcountNoticeStatus.Expired;
            case FallbackHeadcountNoticeStatus.Revoked:
                switch (next)
                {
                    case FallbackHeadcountNoticeStatus.Expired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(FallbackHeadcountNoticeStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"fallback headcount notice {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        Version += 1;
    }

    public string Label()
    {
        return $"{Code}: {Title}";
    }

    public double WeightedLoad()
    {
        return Math.Round((double)Version * (double)BatchSize, 2);
    }

    public FallbackHeadcountNotice Copy()
    {
        return new FallbackHeadcountNotice(Code, Title, BatchSize, Team, Ratio, Version, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Team);
    }
}
