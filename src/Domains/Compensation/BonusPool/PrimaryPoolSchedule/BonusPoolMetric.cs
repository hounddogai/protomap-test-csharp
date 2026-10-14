namespace Acme.Workforce.Compensation.BonusPool.PrimaryPoolSchedule;

using Acme.Workforce.Kit.Text;

/// <summary>Where a bonus pool metric is in its life cycle.</summary>
public enum BonusPoolMetricStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>One bonus pool metric, with what the planners need to act on it.</summary>
public sealed class BonusPoolMetric(string code, string label, string description, string skill, double ceiling, int windowDays, double ratio, string managerId, double cost, string segment, double rate, BonusPoolMetricStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Description { get; set; } = description;
    public string Skill { get; set; } = skill;
    public double Ceiling { get; set; } = ceiling;
    public int WindowDays { get; set; } = windowDays;
    public double Ratio { get; set; } = ratio;
    public string ManagerId { get; set; } = managerId;
    public double Cost { get; set; } = cost;
    public string Segment { get; set; } = segment;
    public double Rate { get; set; } = rate;
    public BonusPoolMetricStatus Status { get; set; } = status;

    /// <summary>Starts a bonus pool metric in status created.</summary>
    public static BonusPoolMetric NewBonusPoolMetric(string code, string label)
    {
        return new BonusPoolMetric(
            code,
            label,
            "",
            "",
            0.0,
            0,
            0.0,
            "",
            0.0,
            "",
            0.0,
            BonusPoolMetricStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case BonusPoolMetricStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(BonusPoolMetricStatus next)
    {
        switch (Status)
        {
            case BonusPoolMetricStatus.Created:
                return next == BonusPoolMetricStatus.Published;
            case BonusPoolMetricStatus.Validated:
                switch (next)
                {
                    case BonusPoolMetricStatus.Published:
                    case BonusPoolMetricStatus.Retired:
                        return true;
                }
                return false;
            case BonusPoolMetricStatus.Published:
                switch (next)
                {
                    case BonusPoolMetricStatus.Retired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(BonusPoolMetricStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"bonus pool metric {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{"bonus pool metric"} {Code}";
    }

    public double ProjectedVolume()
    {
        return Math.Round(Ceiling - (double)WindowDays, 2);
    }

    public BonusPoolMetric Copy()
    {
        return new BonusPoolMetric(
            Code,
            Label,
            Description,
            Skill,
            Ceiling,
            WindowDays,
            Ratio,
            ManagerId,
            Cost,
            Segment,
            Rate,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Segment);
    }
}
