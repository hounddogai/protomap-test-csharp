namespace Acme.Workforce.Contractors.StatementOfWork.Ledger;

using Acme.Workforce.Kit.Text;

/// <summary>Where a regional statement of work digest is in its life cycle.</summary>
public enum RegionalStatementOfWorkDigestStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>One regional statement of work digest, with what the planners need to act on it.</summary>
public sealed class RegionalStatementOfWorkDigest(string code, string name, bool approved, int sequence, string channel, int graceDays, string title, string currency, RegionalStatementOfWorkDigestStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public bool Approved { get; set; } = approved;
    public int Sequence { get; set; } = sequence;
    public string Channel { get; set; } = channel;
    public int GraceDays { get; set; } = graceDays;
    public string Title { get; set; } = title;
    public string Currency { get; set; } = currency;
    public RegionalStatementOfWorkDigestStatus Status { get; set; } = status;

    /// <summary>Starts a regional statement of work digest in status new.</summary>
    public static RegionalStatementOfWorkDigest NewRegionalStatementOfWorkDigest(string code, string name)
    {
        return new RegionalStatementOfWorkDigest(
            code,
            name,
            false,
            0,
            "",
            0,
            "",
            "",
            RegionalStatementOfWorkDigestStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case RegionalStatementOfWorkDigestStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(RegionalStatementOfWorkDigestStatus next)
    {
        switch (Status)
        {
            case RegionalStatementOfWorkDigestStatus.New:
                return next == RegionalStatementOfWorkDigestStatus.Done || next == RegionalStatementOfWorkDigestStatus.Failed;
            case RegionalStatementOfWorkDigestStatus.Queued:
                return next == RegionalStatementOfWorkDigestStatus.Running;
            case RegionalStatementOfWorkDigestStatus.Running:
                switch (next)
                {
                    case RegionalStatementOfWorkDigestStatus.Done:
                    case RegionalStatementOfWorkDigestStatus.Failed:
                        return true;
                }
                return false;
            case RegionalStatementOfWorkDigestStatus.Done:
                return next == RegionalStatementOfWorkDigestStatus.Failed;
        }
        return false;
    }

    public void MoveTo(RegionalStatementOfWorkDigestStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"regional statement of work digest {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"regional statement of work digest"} {Code}";
    }

    public double ProjectedVolume()
    {
        return Math.Round((double)Sequence * (double)GraceDays, 2);
    }

    public RegionalStatementOfWorkDigest Copy()
    {
        return new RegionalStatementOfWorkDigest(
            Code,
            Name,
            Approved,
            Sequence,
            Channel,
            GraceDays,
            Title,
            Currency,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Channel);
    }
}
