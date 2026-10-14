namespace Acme.Workforce.Compensation.PayBand.Batch;

using Acme.Workforce.Kit.Text;

/// <summary>Where a seasonal pay band audit is in its life cycle.</summary>
public enum SeasonalPayBandAuditStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>One seasonal pay band audit, with what the planners need to act on it.</summary>
public sealed class SeasonalPayBandAudit(string code, string title, double threshold, string reviewer, int revision, SeasonalPayBandAuditStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public double Threshold { get; set; } = threshold;
    public string Reviewer { get; set; } = reviewer;
    public int Revision { get; set; } = revision;
    public SeasonalPayBandAuditStatus Status { get; set; } = status;

    /// <summary>Starts a seasonal pay band audit in status requested.</summary>
    public static SeasonalPayBandAudit NewSeasonalPayBandAudit(string code, string title)
    {
        return new SeasonalPayBandAudit(code, title, 0.0, "", 0, SeasonalPayBandAuditStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case SeasonalPayBandAuditStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(SeasonalPayBandAuditStatus next)
    {
        switch (Status)
        {
            case SeasonalPayBandAuditStatus.Requested:
                return next == SeasonalPayBandAuditStatus.Expired;
            case SeasonalPayBandAuditStatus.Granted:
                return next == SeasonalPayBandAuditStatus.Revoked || next == SeasonalPayBandAuditStatus.Expired;
            case SeasonalPayBandAuditStatus.Revoked:
                switch (next)
                {
                    case SeasonalPayBandAuditStatus.Expired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(SeasonalPayBandAuditStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"seasonal pay band audit {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        Revision += 1;
    }

    public string Label()
    {
        return $"{"seasonal pay band audit"} {Code}";
    }

    public double EffectiveLoad()
    {
        return Math.Round(Threshold * (double)Revision, 2);
    }

    public SeasonalPayBandAudit Copy()
    {
        return new SeasonalPayBandAudit(Code, Title, Threshold, Reviewer, Revision, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
