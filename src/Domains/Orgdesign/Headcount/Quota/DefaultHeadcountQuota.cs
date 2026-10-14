namespace Acme.Workforce.Orgdesign.Headcount.Quota;

using Acme.Workforce.Kit.Text;

/// <summary>Where a default headcount quota is in its life cycle.</summary>
public enum DefaultHeadcountQuotaStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>One default headcount quota, with what the planners need to act on it.</summary>
public sealed class DefaultHeadcountQuota(string code, string name, int batchSize, double ratio, int durationMinutes, int graceDays, int revision, string country, DefaultHeadcountQuotaStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public int BatchSize { get; set; } = batchSize;
    public double Ratio { get; set; } = ratio;
    public int DurationMinutes { get; set; } = durationMinutes;
    public int GraceDays { get; set; } = graceDays;
    public int Revision { get; set; } = revision;
    public string Country { get; set; } = country;
    public DefaultHeadcountQuotaStatus Status { get; set; } = status;

    /// <summary>Starts a default headcount quota in status draft.</summary>
    public static DefaultHeadcountQuota NewDefaultHeadcountQuota(string code, string name)
    {
        return new DefaultHeadcountQuota(code, name, 0, 0.0, 0, 0, 0, "", DefaultHeadcountQuotaStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case DefaultHeadcountQuotaStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(DefaultHeadcountQuotaStatus next)
    {
        switch (Status)
        {
            case DefaultHeadcountQuotaStatus.Draft:
                return next == DefaultHeadcountQuotaStatus.Pending || next == DefaultHeadcountQuotaStatus.Suspended;
            case DefaultHeadcountQuotaStatus.Pending:
                return next == DefaultHeadcountQuotaStatus.Active;
            case DefaultHeadcountQuotaStatus.Active:
                switch (next)
                {
                    case DefaultHeadcountQuotaStatus.Suspended:
                    case DefaultHeadcountQuotaStatus.Closed:
                        return true;
                }
                return false;
            case DefaultHeadcountQuotaStatus.Suspended:
                switch (next)
                {
                    case DefaultHeadcountQuotaStatus.Closed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(DefaultHeadcountQuotaStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"default headcount quota {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        Revision += 1;
    }

    public string Label()
    {
        return $"{Code} ({Name})";
    }

    public double ProjectedTotal()
    {
        return Math.Round((double)Revision - (double)DurationMinutes, 2);
    }

    public DefaultHeadcountQuota Copy()
    {
        return new DefaultHeadcountQuota(
            Code,
            Name,
            BatchSize,
            Ratio,
            DurationMinutes,
            GraceDays,
            Revision,
            Country,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Country);
    }
}
