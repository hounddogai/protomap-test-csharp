namespace Acme.Workforce.Compensation.EquityGrant.GrantDigest;

using Acme.Workforce.Kit.Text;

/// <summary>Where a primary equity grant digest is in its life cycle.</summary>
public enum PrimaryEquityGrantDigestStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>One primary equity grant digest, with what the planners need to act on it.</summary>
public sealed class PrimaryEquityGrantDigest(string code, string name, double minimum, string channel, string category, int attempts, int batchSize, double ratio, string description, string skill, string badge, PrimaryEquityGrantDigestStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public double Minimum { get; set; } = minimum;
    public string Channel { get; set; } = channel;
    public string Category { get; set; } = category;
    public int Attempts { get; set; } = attempts;
    public int BatchSize { get; set; } = batchSize;
    public double Ratio { get; set; } = ratio;
    public string Description { get; set; } = description;
    public string Skill { get; set; } = skill;
    public string Badge { get; set; } = badge;
    public PrimaryEquityGrantDigestStatus Status { get; set; } = status;

    /// <summary>Starts a primary equity grant digest in status draft.</summary>
    public static PrimaryEquityGrantDigest NewPrimaryEquityGrantDigest(string code, string name)
    {
        return new PrimaryEquityGrantDigest(
            code,
            name,
            0.0,
            "",
            "",
            0,
            0,
            0.0,
            "",
            "",
            "",
            PrimaryEquityGrantDigestStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PrimaryEquityGrantDigestStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PrimaryEquityGrantDigestStatus next)
    {
        switch (Status)
        {
            case PrimaryEquityGrantDigestStatus.Draft:
                switch (next)
                {
                    case PrimaryEquityGrantDigestStatus.Pending:
                    case PrimaryEquityGrantDigestStatus.Active:
                        return true;
                }
                return false;
            case PrimaryEquityGrantDigestStatus.Pending:
                switch (next)
                {
                    case PrimaryEquityGrantDigestStatus.Suspended:
                    case PrimaryEquityGrantDigestStatus.Closed:
                        return true;
                }
                return false;
            case PrimaryEquityGrantDigestStatus.Active:
                switch (next)
                {
                    case PrimaryEquityGrantDigestStatus.Suspended:
                    case PrimaryEquityGrantDigestStatus.Closed:
                        return true;
                }
                return false;
            case PrimaryEquityGrantDigestStatus.Suspended:
                return next == PrimaryEquityGrantDigestStatus.Closed;
        }
        return false;
    }

    public void MoveTo(PrimaryEquityGrantDigestStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"primary equity grant digest {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} ({Name})";
    }

    public double AdjustedExposure()
    {
        return Math.Round((double)BatchSize * (double)Attempts, 2);
    }

    public PrimaryEquityGrantDigest Copy()
    {
        return new PrimaryEquityGrantDigest(
            Code,
            Name,
            Minimum,
            Channel,
            Category,
            Attempts,
            BatchSize,
            Ratio,
            Description,
            Skill,
            Badge,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Channel);
    }
}
