namespace Acme.Workforce.Payrollops.TaxForm.Adjustment;

using Acme.Workforce.Kit.Text;

/// <summary>Where a regional tax form adjustment is in its life cycle.</summary>
public enum RegionalTaxFormAdjustmentStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>One regional tax form adjustment, with what the planners need to act on it.</summary>
public sealed class RegionalTaxFormAdjustment(string code, string title, int startsAt, int durationMinutes, string grade, double budget, int batchSize, RegionalTaxFormAdjustmentStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public int StartsAt { get; set; } = startsAt;
    public int DurationMinutes { get; set; } = durationMinutes;
    public string Grade { get; set; } = grade;
    public double Budget { get; set; } = budget;
    public int BatchSize { get; set; } = batchSize;
    public RegionalTaxFormAdjustmentStatus Status { get; set; } = status;

    /// <summary>Starts a regional tax form adjustment in status idle.</summary>
    public static RegionalTaxFormAdjustment NewRegionalTaxFormAdjustment(string code, string title)
    {
        return new RegionalTaxFormAdjustment(code, title, 0, 0, "", 0.0, 0, RegionalTaxFormAdjustmentStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case RegionalTaxFormAdjustmentStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(RegionalTaxFormAdjustmentStatus next)
    {
        switch (Status)
        {
            case RegionalTaxFormAdjustmentStatus.Idle:
                switch (next)
                {
                    case RegionalTaxFormAdjustmentStatus.Ready:
                        return true;
                }
                return false;
            case RegionalTaxFormAdjustmentStatus.Warming:
                return next == RegionalTaxFormAdjustmentStatus.Draining;
            case RegionalTaxFormAdjustmentStatus.Ready:
                return next == RegionalTaxFormAdjustmentStatus.Draining || next == RegionalTaxFormAdjustmentStatus.Stopped;
            case RegionalTaxFormAdjustmentStatus.Draining:
                return next == RegionalTaxFormAdjustmentStatus.Stopped;
        }
        return false;
    }

    public void MoveTo(RegionalTaxFormAdjustmentStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"regional tax form adjustment {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        StartsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code} / {Title}";
    }

    public double AdjustedValue()
    {
        return Math.Round((double)DurationMinutes - (double)BatchSize, 2);
    }

    public RegionalTaxFormAdjustment Copy()
    {
        return new RegionalTaxFormAdjustment(
            Code,
            Title,
            StartsAt,
            DurationMinutes,
            Grade,
            Budget,
            BatchSize,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
