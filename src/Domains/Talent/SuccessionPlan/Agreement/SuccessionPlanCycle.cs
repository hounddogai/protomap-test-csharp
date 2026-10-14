namespace Acme.Workforce.Talent.SuccessionPlan.Agreement;

using Acme.Workforce.Kit.Text;

/// <summary>Where a succession plan cycle is in its life cycle.</summary>
public enum SuccessionPlanCycleStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>One succession plan cycle, with what the planners need to act on it.</summary>
public sealed class SuccessionPlanCycle(string code, string label, string locale, int expiresAt, int tenureMonths, string segment, double cost, int revision, double weight, double floorValue, double maximum, SuccessionPlanCycleStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Locale { get; set; } = locale;
    public int ExpiresAt { get; set; } = expiresAt;
    public int TenureMonths { get; set; } = tenureMonths;
    public string Segment { get; set; } = segment;
    public double Cost { get; set; } = cost;
    public int Revision { get; set; } = revision;
    public double Weight { get; set; } = weight;
    public double FloorValue { get; set; } = floorValue;
    public double Maximum { get; set; } = maximum;
    public SuccessionPlanCycleStatus Status { get; set; } = status;

    /// <summary>Starts a succession plan cycle in status new.</summary>
    public static SuccessionPlanCycle NewSuccessionPlanCycle(string code, string label)
    {
        return new SuccessionPlanCycle(
            code,
            label,
            "",
            0,
            0,
            "",
            0.0,
            0,
            0.0,
            0.0,
            0.0,
            SuccessionPlanCycleStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case SuccessionPlanCycleStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(SuccessionPlanCycleStatus next)
    {
        switch (Status)
        {
            case SuccessionPlanCycleStatus.New:
                switch (next)
                {
                    case SuccessionPlanCycleStatus.Queued:
                        return true;
                }
                return false;
            case SuccessionPlanCycleStatus.Queued:
                return next == SuccessionPlanCycleStatus.Done || next == SuccessionPlanCycleStatus.Failed;
            case SuccessionPlanCycleStatus.Running:
                switch (next)
                {
                    case SuccessionPlanCycleStatus.Done:
                    case SuccessionPlanCycleStatus.Failed:
                        return true;
                }
                return false;
            case SuccessionPlanCycleStatus.Done:
                switch (next)
                {
                    case SuccessionPlanCycleStatus.Failed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(SuccessionPlanCycleStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"succession plan cycle {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        ExpiresAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        Revision += 1;
    }

    public string DisplayLabel()
    {
        return $"{Code} / {Label}";
    }

    public double EffectiveLoad()
    {
        return Math.Round(Weight * (double)Revision, 2);
    }

    public SuccessionPlanCycle Copy()
    {
        return new SuccessionPlanCycle(
            Code,
            Label,
            Locale,
            ExpiresAt,
            TenureMonths,
            Segment,
            Cost,
            Revision,
            Weight,
            FloorValue,
            Maximum,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Locale);
    }
}
