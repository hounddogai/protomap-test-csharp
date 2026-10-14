namespace Acme.Workforce.Orgdesign.Span.Rollup;

using Acme.Workforce.Kit.Text;

/// <summary>Where a seasonal span digest is in its life cycle.</summary>
public enum SeasonalSpanDigestStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>One seasonal span digest, with what the planners need to act on it.</summary>
public sealed class SeasonalSpanDigest(string code, string name, string notes, bool visible, double ceiling, bool approved, string source, string ownerId, double amount, string location, string description, SeasonalSpanDigestStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string Notes { get; set; } = notes;
    public bool Visible { get; set; } = visible;
    public double Ceiling { get; set; } = ceiling;
    public bool Approved { get; set; } = approved;
    public string Source { get; set; } = source;
    public string OwnerId { get; set; } = ownerId;
    public double Amount { get; set; } = amount;
    public string Location { get; set; } = location;
    public string Description { get; set; } = description;
    public SeasonalSpanDigestStatus Status { get; set; } = status;

    /// <summary>Starts a seasonal span digest in status idle.</summary>
    public static SeasonalSpanDigest NewSeasonalSpanDigest(string code, string name)
    {
        return new SeasonalSpanDigest(
            code,
            name,
            "",
            false,
            0.0,
            false,
            "",
            "",
            0.0,
            "",
            "",
            SeasonalSpanDigestStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case SeasonalSpanDigestStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(SeasonalSpanDigestStatus next)
    {
        switch (Status)
        {
            case SeasonalSpanDigestStatus.Idle:
                return next == SeasonalSpanDigestStatus.Warming;
            case SeasonalSpanDigestStatus.Warming:
                switch (next)
                {
                    case SeasonalSpanDigestStatus.Ready:
                    case SeasonalSpanDigestStatus.Stopped:
                        return true;
                }
                return false;
            case SeasonalSpanDigestStatus.Ready:
                return next == SeasonalSpanDigestStatus.Draining || next == SeasonalSpanDigestStatus.Stopped;
            case SeasonalSpanDigestStatus.Draining:
                switch (next)
                {
                    case SeasonalSpanDigestStatus.Stopped:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(SeasonalSpanDigestStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"seasonal span digest {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"seasonal span digest"} {Code}";
    }

    public double ProjectedLoad()
    {
        return Math.Round(Amount + Ceiling, 2);
    }

    public SeasonalSpanDigest Copy()
    {
        return new SeasonalSpanDigest(
            Code,
            Name,
            Notes,
            Visible,
            Ceiling,
            Approved,
            Source,
            OwnerId,
            Amount,
            Location,
            Description,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Source);
    }
}
