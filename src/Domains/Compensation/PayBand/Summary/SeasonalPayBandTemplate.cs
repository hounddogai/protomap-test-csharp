namespace Acme.Workforce.Compensation.PayBand.Summary;

using Acme.Workforce.Kit.Text;

/// <summary>Where a seasonal pay band template is in its life cycle.</summary>
public enum SeasonalPayBandTemplateStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>A seasonal pay band template as the pay band team keeps it.</summary>
public sealed class SeasonalPayBandTemplate(string code, string name, string employeeId, int startsAt, int createdAt, int tenureMonths, double weight, int durationMinutes, SeasonalPayBandTemplateStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string EmployeeId { get; set; } = employeeId;
    public int StartsAt { get; set; } = startsAt;
    public int CreatedAt { get; set; } = createdAt;
    public int TenureMonths { get; set; } = tenureMonths;
    public double Weight { get; set; } = weight;
    public int DurationMinutes { get; set; } = durationMinutes;
    public SeasonalPayBandTemplateStatus Status { get; set; } = status;

    /// <summary>Starts a seasonal pay band template in status open.</summary>
    public static SeasonalPayBandTemplate NewSeasonalPayBandTemplate(string code, string name)
    {
        return new SeasonalPayBandTemplate(code, name, "", 0, 0, 0, 0.0, 0, SeasonalPayBandTemplateStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case SeasonalPayBandTemplateStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(SeasonalPayBandTemplateStatus next)
    {
        switch (Status)
        {
            case SeasonalPayBandTemplateStatus.Open:
                switch (next)
                {
                    case SeasonalPayBandTemplateStatus.InReview:
                        return true;
                }
                return false;
            case SeasonalPayBandTemplateStatus.InReview:
                return next == SeasonalPayBandTemplateStatus.Archived;
            case SeasonalPayBandTemplateStatus.Resolved:
                switch (next)
                {
                    case SeasonalPayBandTemplateStatus.Reopened:
                    case SeasonalPayBandTemplateStatus.Archived:
                        return true;
                }
                return false;
            case SeasonalPayBandTemplateStatus.Reopened:
                switch (next)
                {
                    case SeasonalPayBandTemplateStatus.Archived:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(SeasonalPayBandTemplateStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"seasonal pay band template {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        StartsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code}: {Name}";
    }

    public double WeightedVolume()
    {
        return Math.Round((double)DurationMinutes + Weight, 2);
    }

    public SeasonalPayBandTemplate Copy()
    {
        return new SeasonalPayBandTemplate(
            Code,
            Name,
            EmployeeId,
            StartsAt,
            CreatedAt,
            TenureMonths,
            Weight,
            DurationMinutes,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
