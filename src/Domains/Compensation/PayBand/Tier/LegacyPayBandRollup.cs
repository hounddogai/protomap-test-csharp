namespace Acme.Workforce.Compensation.PayBand.Tier;

using Acme.Workforce.Kit.Text;

/// <summary>Where a legacy pay band rollup is in its life cycle.</summary>
public enum LegacyPayBandRollupStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>A legacy pay band rollup as the pay band team keeps it.</summary>
public sealed class LegacyPayBandRollup(string code, string name, int graceDays, double minimum, double discount, int startsAt, string externalRef, Dictionary<string, string> attributes, int revision, int limit, bool visible, LegacyPayBandRollupStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public int GraceDays { get; set; } = graceDays;
    public double Minimum { get; set; } = minimum;
    public double Discount { get; set; } = discount;
    public int StartsAt { get; set; } = startsAt;
    public string ExternalRef { get; set; } = externalRef;
    public Dictionary<string, string> Attributes { get; set; } = attributes;
    public int Revision { get; set; } = revision;
    public int Limit { get; set; } = limit;
    public bool Visible { get; set; } = visible;
    public LegacyPayBandRollupStatus Status { get; set; } = status;

    /// <summary>Starts a legacy pay band rollup in status draft.</summary>
    public static LegacyPayBandRollup NewLegacyPayBandRollup(string code, string name)
    {
        return new LegacyPayBandRollup(
            code,
            name,
            0,
            0.0,
            0.0,
            0,
            "",
            new Dictionary<string, string>(),
            0,
            0,
            false,
            LegacyPayBandRollupStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case LegacyPayBandRollupStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(LegacyPayBandRollupStatus next)
    {
        switch (Status)
        {
            case LegacyPayBandRollupStatus.Draft:
                return next == LegacyPayBandRollupStatus.Active;
            case LegacyPayBandRollupStatus.Pending:
                switch (next)
                {
                    case LegacyPayBandRollupStatus.Active:
                    case LegacyPayBandRollupStatus.Closed:
                        return true;
                }
                return false;
            case LegacyPayBandRollupStatus.Active:
                switch (next)
                {
                    case LegacyPayBandRollupStatus.Suspended:
                    case LegacyPayBandRollupStatus.Closed:
                        return true;
                }
                return false;
            case LegacyPayBandRollupStatus.Suspended:
                switch (next)
                {
                    case LegacyPayBandRollupStatus.Closed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(LegacyPayBandRollupStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"legacy pay band rollup {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        StartsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        Revision += 1;
    }

    public string Label()
    {
        return $"{Code} / {Name}";
    }

    public string Attribute(string key, string fallback)
    {
        if (!Attributes.TryGetValue(key, out var found))
        {
            return fallback;
        }
        if (found.Trim() == "")
        {
            return fallback;
        }
        return found;
    }

    public double AdjustedExposure()
    {
        return Math.Round((double)Revision * (double)StartsAt, 2);
    }

    public LegacyPayBandRollup Copy()
    {
        return new LegacyPayBandRollup(
            Code,
            Name,
            GraceDays,
            Minimum,
            Discount,
            StartsAt,
            ExternalRef,
            Attributes,
            Revision,
            Limit,
            Visible,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
