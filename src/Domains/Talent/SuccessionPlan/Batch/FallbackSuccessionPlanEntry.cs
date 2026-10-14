namespace Acme.Workforce.Talent.SuccessionPlan.Batch;

using Acme.Workforce.Kit.Text;

/// <summary>Where a fallback succession plan entry is in its life cycle.</summary>
public enum FallbackSuccessionPlanEntryStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>The record of a fallback succession plan entry and its current status.</summary>
public sealed class FallbackSuccessionPlanEntry(string code, string label, string locale, int durationMinutes, string notes, string employeeId, int batchSize, double floorValue, FallbackSuccessionPlanEntryStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Locale { get; set; } = locale;
    public int DurationMinutes { get; set; } = durationMinutes;
    public string Notes { get; set; } = notes;
    public string EmployeeId { get; set; } = employeeId;
    public int BatchSize { get; set; } = batchSize;
    public double FloorValue { get; set; } = floorValue;
    public FallbackSuccessionPlanEntryStatus Status { get; set; } = status;

    /// <summary>Starts a fallback succession plan entry in status new.</summary>
    public static FallbackSuccessionPlanEntry NewFallbackSuccessionPlanEntry(string code, string label)
    {
        return new FallbackSuccessionPlanEntry(
            code,
            label,
            "",
            0,
            "",
            "",
            0,
            0.0,
            FallbackSuccessionPlanEntryStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case FallbackSuccessionPlanEntryStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(FallbackSuccessionPlanEntryStatus next)
    {
        switch (Status)
        {
            case FallbackSuccessionPlanEntryStatus.New:
                return next == FallbackSuccessionPlanEntryStatus.Failed;
            case FallbackSuccessionPlanEntryStatus.Queued:
                return next == FallbackSuccessionPlanEntryStatus.Running || next == FallbackSuccessionPlanEntryStatus.Failed;
            case FallbackSuccessionPlanEntryStatus.Running:
                return next == FallbackSuccessionPlanEntryStatus.Failed;
            case FallbackSuccessionPlanEntryStatus.Done:
                switch (next)
                {
                    case FallbackSuccessionPlanEntryStatus.Failed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(FallbackSuccessionPlanEntryStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"fallback succession plan entry {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{"fallback succession plan entry"} {Code}";
    }

    public double WeightedVolume()
    {
        return Math.Round((double)DurationMinutes - FloorValue, 2);
    }

    public FallbackSuccessionPlanEntry Copy()
    {
        return new FallbackSuccessionPlanEntry(
            Code,
            Label,
            Locale,
            DurationMinutes,
            Notes,
            EmployeeId,
            BatchSize,
            FloorValue,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Locale);
    }
}
