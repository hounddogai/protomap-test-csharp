namespace Acme.Workforce.Contractors.Engagement.Notice;

using Acme.Workforce.Kit.Text;

/// <summary>Where a seasonal engagement rollup is in its life cycle.</summary>
public enum SeasonalEngagementRollupStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>A seasonal engagement rollup as the engagement team keeps it.</summary>
public sealed class SeasonalEngagementRollup(string code, string title, string jobCode, int retries, double cost, string department, int graceDays, int tenureMonths, int sequence, SeasonalEngagementRollupStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string JobCode { get; set; } = jobCode;
    public int Retries { get; set; } = retries;
    public double Cost { get; set; } = cost;
    public string Department { get; set; } = department;
    public int GraceDays { get; set; } = graceDays;
    public int TenureMonths { get; set; } = tenureMonths;
    public int Sequence { get; set; } = sequence;
    public SeasonalEngagementRollupStatus Status { get; set; } = status;

    /// <summary>Starts a seasonal engagement rollup in status draft.</summary>
    public static SeasonalEngagementRollup NewSeasonalEngagementRollup(string code, string title)
    {
        return new SeasonalEngagementRollup(
            code,
            title,
            "",
            0,
            0.0,
            "",
            0,
            0,
            0,
            SeasonalEngagementRollupStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case SeasonalEngagementRollupStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(SeasonalEngagementRollupStatus next)
    {
        switch (Status)
        {
            case SeasonalEngagementRollupStatus.Draft:
                switch (next)
                {
                    case SeasonalEngagementRollupStatus.Pending:
                    case SeasonalEngagementRollupStatus.Suspended:
                        return true;
                }
                return false;
            case SeasonalEngagementRollupStatus.Pending:
                switch (next)
                {
                    case SeasonalEngagementRollupStatus.Closed:
                        return true;
                }
                return false;
            case SeasonalEngagementRollupStatus.Active:
                switch (next)
                {
                    case SeasonalEngagementRollupStatus.Suspended:
                    case SeasonalEngagementRollupStatus.Closed:
                        return true;
                }
                return false;
            case SeasonalEngagementRollupStatus.Suspended:
                return next == SeasonalEngagementRollupStatus.Closed;
        }
        return false;
    }

    public void MoveTo(SeasonalEngagementRollupStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"seasonal engagement rollup {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} ({Title})";
    }

    public double NetValue()
    {
        return Math.Round((double)Retries * (double)GraceDays, 2);
    }

    public SeasonalEngagementRollup Copy()
    {
        return new SeasonalEngagementRollup(
            Code,
            Title,
            JobCode,
            Retries,
            Cost,
            Department,
            GraceDays,
            TenureMonths,
            Sequence,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Department);
    }
}
