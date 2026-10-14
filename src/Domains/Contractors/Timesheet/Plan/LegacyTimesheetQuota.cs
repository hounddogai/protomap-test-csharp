namespace Acme.Workforce.Contractors.Timesheet.Plan;

using Acme.Workforce.Kit.Text;

/// <summary>Where a legacy timesheet quota is in its life cycle.</summary>
public enum LegacyTimesheetQuotaStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>One legacy timesheet quota, with what the planners need to act on it.</summary>
public sealed class LegacyTimesheetQuota(string code, string title, string label, int attempts, int capacity, double rating, string source, double cost, int updatedAt, LegacyTimesheetQuotaStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string Label { get; set; } = label;
    public int Attempts { get; set; } = attempts;
    public int Capacity { get; set; } = capacity;
    public double Rating { get; set; } = rating;
    public string Source { get; set; } = source;
    public double Cost { get; set; } = cost;
    public int UpdatedAt { get; set; } = updatedAt;
    public LegacyTimesheetQuotaStatus Status { get; set; } = status;

    /// <summary>Starts a legacy timesheet quota in status created.</summary>
    public static LegacyTimesheetQuota NewLegacyTimesheetQuota(string code, string title)
    {
        return new LegacyTimesheetQuota(code, title, "", 0, 0, 0.0, "", 0.0, 0, LegacyTimesheetQuotaStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case LegacyTimesheetQuotaStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(LegacyTimesheetQuotaStatus next)
    {
        switch (Status)
        {
            case LegacyTimesheetQuotaStatus.Created:
                switch (next)
                {
                    case LegacyTimesheetQuotaStatus.Validated:
                        return true;
                }
                return false;
            case LegacyTimesheetQuotaStatus.Validated:
                return next == LegacyTimesheetQuotaStatus.Retired;
            case LegacyTimesheetQuotaStatus.Published:
                switch (next)
                {
                    case LegacyTimesheetQuotaStatus.Retired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(LegacyTimesheetQuotaStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"legacy timesheet quota {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        UpdatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{Code}: {Title}";
    }

    public double EffectiveVolume()
    {
        return Math.Round(Cost + Rating, 2);
    }

    public LegacyTimesheetQuota Copy()
    {
        return new LegacyTimesheetQuota(
            Code,
            Title,
            Label,
            Attempts,
            Capacity,
            Rating,
            Source,
            Cost,
            UpdatedAt,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Source);
    }
}
