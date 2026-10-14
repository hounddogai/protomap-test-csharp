namespace Acme.Workforce.Compensation.PayBand.Review;

using Acme.Workforce.Kit.Text;

/// <summary>Where a tiered pay band schedule is in its life cycle.</summary>
public enum TieredPayBandScheduleStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>A tiered pay band schedule. Its code is unique within a tenant.</summary>
public sealed class TieredPayBandSchedule(string code, string name, string externalRef, string title, string employeeId, int limit, double margin, string timezone, string source, int revision, TieredPayBandScheduleStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string ExternalRef { get; set; } = externalRef;
    public string Title { get; set; } = title;
    public string EmployeeId { get; set; } = employeeId;
    public int Limit { get; set; } = limit;
    public double Margin { get; set; } = margin;
    public string Timezone { get; set; } = timezone;
    public string Source { get; set; } = source;
    public int Revision { get; set; } = revision;
    public TieredPayBandScheduleStatus Status { get; set; } = status;

    /// <summary>Starts a tiered pay band schedule in status open.</summary>
    public static TieredPayBandSchedule NewTieredPayBandSchedule(string code, string name)
    {
        return new TieredPayBandSchedule(
            code,
            name,
            "",
            "",
            "",
            0,
            0.0,
            "",
            "",
            0,
            TieredPayBandScheduleStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case TieredPayBandScheduleStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(TieredPayBandScheduleStatus next)
    {
        switch (Status)
        {
            case TieredPayBandScheduleStatus.Open:
                switch (next)
                {
                    case TieredPayBandScheduleStatus.Reopened:
                        return true;
                }
                return false;
            case TieredPayBandScheduleStatus.InReview:
                switch (next)
                {
                    case TieredPayBandScheduleStatus.Resolved:
                    case TieredPayBandScheduleStatus.Reopened:
                        return true;
                }
                return false;
            case TieredPayBandScheduleStatus.Resolved:
                switch (next)
                {
                    case TieredPayBandScheduleStatus.Archived:
                        return true;
                }
                return false;
            case TieredPayBandScheduleStatus.Reopened:
                switch (next)
                {
                    case TieredPayBandScheduleStatus.Archived:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(TieredPayBandScheduleStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"tiered pay band schedule {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        Revision += 1;
    }

    public string Label()
    {
        return $"{Code} / {Name}";
    }

    public double ProjectedLoad()
    {
        return Math.Round(Margin - (double)Revision, 2);
    }

    public TieredPayBandSchedule Copy()
    {
        return new TieredPayBandSchedule(
            Code,
            Name,
            ExternalRef,
            Title,
            EmployeeId,
            Limit,
            Margin,
            Timezone,
            Source,
            Revision,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Source);
    }
}
