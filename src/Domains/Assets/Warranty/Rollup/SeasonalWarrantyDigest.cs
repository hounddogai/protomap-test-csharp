namespace Acme.Workforce.Assets.Warranty.Rollup;

using Acme.Workforce.Kit.Text;

/// <summary>Where a seasonal warranty digest is in its life cycle.</summary>
public enum SeasonalWarrantyDigestStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>A seasonal warranty digest as the warranty team keeps it.</summary>
public sealed class SeasonalWarrantyDigest(string code, string title, string jobCode, bool visible, string team, string currency, double amount, int retries, int revision, SeasonalWarrantyDigestStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string JobCode { get; set; } = jobCode;
    public bool Visible { get; set; } = visible;
    public string Team { get; set; } = team;
    public string Currency { get; set; } = currency;
    public double Amount { get; set; } = amount;
    public int Retries { get; set; } = retries;
    public int Revision { get; set; } = revision;
    public SeasonalWarrantyDigestStatus Status { get; set; } = status;

    /// <summary>Starts a seasonal warranty digest in status created.</summary>
    public static SeasonalWarrantyDigest NewSeasonalWarrantyDigest(string code, string title)
    {
        return new SeasonalWarrantyDigest(
            code,
            title,
            "",
            false,
            "",
            "",
            0.0,
            0,
            0,
            SeasonalWarrantyDigestStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case SeasonalWarrantyDigestStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(SeasonalWarrantyDigestStatus next)
    {
        switch (Status)
        {
            case SeasonalWarrantyDigestStatus.Created:
                return next == SeasonalWarrantyDigestStatus.Published;
            case SeasonalWarrantyDigestStatus.Validated:
                switch (next)
                {
                    case SeasonalWarrantyDigestStatus.Published:
                    case SeasonalWarrantyDigestStatus.Retired:
                        return true;
                }
                return false;
            case SeasonalWarrantyDigestStatus.Published:
                switch (next)
                {
                    case SeasonalWarrantyDigestStatus.Retired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(SeasonalWarrantyDigestStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"seasonal warranty digest {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        Revision += 1;
    }

    public string Label()
    {
        return $"{"seasonal warranty digest"} {Code}";
    }

    public double AdjustedVolume()
    {
        return Math.Round((double)Retries + Amount, 2);
    }

    public SeasonalWarrantyDigest Copy()
    {
        return new SeasonalWarrantyDigest(
            Code,
            Title,
            JobCode,
            Visible,
            Team,
            Currency,
            Amount,
            Retries,
            Revision,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Team);
    }
}
