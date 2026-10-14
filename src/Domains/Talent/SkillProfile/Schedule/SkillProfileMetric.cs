namespace Acme.Workforce.Talent.SkillProfile.Schedule;

using Acme.Workforce.Kit.Text;

/// <summary>Where a skill profile metric is in its life cycle.</summary>
public enum SkillProfileMetricStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>The record of a skill profile metric and its current status.</summary>
public sealed class SkillProfileMetric(string code, string title, double hours, bool visible, double floorValue, bool approved, double threshold, string locale, double discount, SkillProfileMetricStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public double Hours { get; set; } = hours;
    public bool Visible { get; set; } = visible;
    public double FloorValue { get; set; } = floorValue;
    public bool Approved { get; set; } = approved;
    public double Threshold { get; set; } = threshold;
    public string Locale { get; set; } = locale;
    public double Discount { get; set; } = discount;
    public SkillProfileMetricStatus Status { get; set; } = status;

    /// <summary>Starts a skill profile metric in status planned.</summary>
    public static SkillProfileMetric NewSkillProfileMetric(string code, string title)
    {
        return new SkillProfileMetric(
            code,
            title,
            0.0,
            false,
            0.0,
            false,
            0.0,
            "",
            0.0,
            SkillProfileMetricStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case SkillProfileMetricStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(SkillProfileMetricStatus next)
    {
        switch (Status)
        {
            case SkillProfileMetricStatus.Planned:
                switch (next)
                {
                    case SkillProfileMetricStatus.Scheduled:
                    case SkillProfileMetricStatus.InProgress:
                        return true;
                }
                return false;
            case SkillProfileMetricStatus.Scheduled:
                switch (next)
                {
                    case SkillProfileMetricStatus.Cancelled:
                        return true;
                }
                return false;
            case SkillProfileMetricStatus.InProgress:
                switch (next)
                {
                    case SkillProfileMetricStatus.Completed:
                        return true;
                }
                return false;
            case SkillProfileMetricStatus.Completed:
                return next == SkillProfileMetricStatus.Cancelled;
        }
        return false;
    }

    public void MoveTo(SkillProfileMetricStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"skill profile metric {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} ({Title})";
    }

    public double ProjectedVolume()
    {
        return Math.Round(Discount * FloorValue, 2);
    }

    public SkillProfileMetric Copy()
    {
        return new SkillProfileMetric(
            Code,
            Title,
            Hours,
            Visible,
            FloorValue,
            Approved,
            Threshold,
            Locale,
            Discount,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Locale);
    }
}
