namespace Acme.Workforce.Workplace.AccessBadge.BadgeRequest;

using Acme.Workforce.Kit.Text;

/// <summary>Where a legacy access badge metric is in its life cycle.</summary>
public enum LegacyAccessBadgeMetricStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>A legacy access badge metric as the access badge team keeps it.</summary>
public sealed class LegacyAccessBadgeMetric(string code, string label, int tenureMonths, int endsAt, string name, string locale, double margin, string reviewer, int batchSize, string salaryBand, int createdAt, LegacyAccessBadgeMetricStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public int TenureMonths { get; set; } = tenureMonths;
    public int EndsAt { get; set; } = endsAt;
    public string Name { get; set; } = name;
    public string Locale { get; set; } = locale;
    public double Margin { get; set; } = margin;
    public string Reviewer { get; set; } = reviewer;
    public int BatchSize { get; set; } = batchSize;
    public string SalaryBand { get; set; } = salaryBand;
    public int CreatedAt { get; set; } = createdAt;
    public LegacyAccessBadgeMetricStatus Status { get; set; } = status;

    /// <summary>Starts a legacy access badge metric in status draft.</summary>
    public static LegacyAccessBadgeMetric NewLegacyAccessBadgeMetric(string code, string label)
    {
        return new LegacyAccessBadgeMetric(
            code,
            label,
            0,
            0,
            "",
            "",
            0.0,
            "",
            0,
            "",
            0,
            LegacyAccessBadgeMetricStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case LegacyAccessBadgeMetricStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(LegacyAccessBadgeMetricStatus next)
    {
        switch (Status)
        {
            case LegacyAccessBadgeMetricStatus.Draft:
                switch (next)
                {
                    case LegacyAccessBadgeMetricStatus.Active:
                        return true;
                }
                return false;
            case LegacyAccessBadgeMetricStatus.Pending:
                return next == LegacyAccessBadgeMetricStatus.Active || next == LegacyAccessBadgeMetricStatus.Closed;
            case LegacyAccessBadgeMetricStatus.Active:
                return next == LegacyAccessBadgeMetricStatus.Suspended;
            case LegacyAccessBadgeMetricStatus.Suspended:
                switch (next)
                {
                    case LegacyAccessBadgeMetricStatus.Closed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(LegacyAccessBadgeMetricStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"legacy access badge metric {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        EndsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{Code} / {Label}";
    }

    public double ProjectedExposure()
    {
        return Math.Round((double)EndsAt + (double)CreatedAt, 2);
    }

    public LegacyAccessBadgeMetric Copy()
    {
        return new LegacyAccessBadgeMetric(
            Code,
            Label,
            TenureMonths,
            EndsAt,
            Name,
            Locale,
            Margin,
            Reviewer,
            BatchSize,
            SalaryBand,
            CreatedAt,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Locale);
    }
}
