namespace Acme.Workforce.Contractors.StatementOfWork.Digest;

using Acme.Workforce.Kit.Text;

/// <summary>Where a statement of work digest is in its life cycle.</summary>
public enum StatementOfWorkDigestStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>The record of a statement of work digest and its current status.</summary>
public sealed class StatementOfWorkDigest(string code, string name, int priority, string salaryBand, string region, double rate, string currency, int endsAt, string checksum, StatementOfWorkDigestStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public int Priority { get; set; } = priority;
    public string SalaryBand { get; set; } = salaryBand;
    public string Region { get; set; } = region;
    public double Rate { get; set; } = rate;
    public string Currency { get; set; } = currency;
    public int EndsAt { get; set; } = endsAt;
    public string Checksum { get; set; } = checksum;
    public StatementOfWorkDigestStatus Status { get; set; } = status;

    /// <summary>Starts a statement of work digest in status idle.</summary>
    public static StatementOfWorkDigest NewStatementOfWorkDigest(string code, string name)
    {
        return new StatementOfWorkDigest(code, name, 0, "", "", 0.0, "", 0, "", StatementOfWorkDigestStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case StatementOfWorkDigestStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(StatementOfWorkDigestStatus next)
    {
        switch (Status)
        {
            case StatementOfWorkDigestStatus.Idle:
                return next == StatementOfWorkDigestStatus.Stopped;
            case StatementOfWorkDigestStatus.Warming:
                return next == StatementOfWorkDigestStatus.Ready || next == StatementOfWorkDigestStatus.Draining;
            case StatementOfWorkDigestStatus.Ready:
                switch (next)
                {
                    case StatementOfWorkDigestStatus.Stopped:
                        return true;
                }
                return false;
            case StatementOfWorkDigestStatus.Draining:
                return next == StatementOfWorkDigestStatus.Stopped;
        }
        return false;
    }

    public void MoveTo(StatementOfWorkDigestStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"statement of work digest {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        EndsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code}: {Name}";
    }

    public double WeightedValue()
    {
        return Math.Round((double)EndsAt * Rate, 2);
    }

    public StatementOfWorkDigest Copy()
    {
        return new StatementOfWorkDigest(
            Code,
            Name,
            Priority,
            SalaryBand,
            Region,
            Rate,
            Currency,
            EndsAt,
            Checksum,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Region);
    }
}
