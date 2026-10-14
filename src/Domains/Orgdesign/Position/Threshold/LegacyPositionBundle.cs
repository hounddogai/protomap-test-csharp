namespace Acme.Workforce.Orgdesign.Position.Threshold;

using Acme.Workforce.Kit.Text;

/// <summary>Where a legacy position bundle is in its life cycle.</summary>
public enum LegacyPositionBundleStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>A legacy position bundle. Its code is unique within a tenant.</summary>
public sealed class LegacyPositionBundle(string code, string label, int capacity, double ratio, string segment, int priority, int durationMinutes, int tenureMonths, double amount, string contactEmail, LegacyPositionBundleStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public int Capacity { get; set; } = capacity;
    public double Ratio { get; set; } = ratio;
    public string Segment { get; set; } = segment;
    public int Priority { get; set; } = priority;
    public int DurationMinutes { get; set; } = durationMinutes;
    public int TenureMonths { get; set; } = tenureMonths;
    public double Amount { get; set; } = amount;
    public string ContactEmail { get; set; } = contactEmail;
    public LegacyPositionBundleStatus Status { get; set; } = status;

    /// <summary>Starts a legacy position bundle in status new.</summary>
    public static LegacyPositionBundle NewLegacyPositionBundle(string code, string label)
    {
        return new LegacyPositionBundle(code, label, 0, 0.0, "", 0, 0, 0, 0.0, "", LegacyPositionBundleStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case LegacyPositionBundleStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(LegacyPositionBundleStatus next)
    {
        switch (Status)
        {
            case LegacyPositionBundleStatus.New:
                return next == LegacyPositionBundleStatus.Done;
            case LegacyPositionBundleStatus.Queued:
                return next == LegacyPositionBundleStatus.Running || next == LegacyPositionBundleStatus.Failed;
            case LegacyPositionBundleStatus.Running:
                return next == LegacyPositionBundleStatus.Done || next == LegacyPositionBundleStatus.Failed;
            case LegacyPositionBundleStatus.Done:
                return next == LegacyPositionBundleStatus.Failed;
        }
        return false;
    }

    public void MoveTo(LegacyPositionBundleStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"legacy position bundle {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code}: {Label}";
    }

    public double EffectiveVolume()
    {
        return Math.Round(Ratio * Amount, 2);
    }

    public LegacyPositionBundle Copy()
    {
        return new LegacyPositionBundle(
            Code,
            Label,
            Capacity,
            Ratio,
            Segment,
            Priority,
            DurationMinutes,
            TenureMonths,
            Amount,
            ContactEmail,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Segment);
    }
}
