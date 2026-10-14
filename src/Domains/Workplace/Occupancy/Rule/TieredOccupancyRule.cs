namespace Acme.Workforce.Workplace.Occupancy.Rule;

using Acme.Workforce.Kit.Text;

/// <summary>Where a tiered occupancy rule is in its life cycle.</summary>
public enum TieredOccupancyRuleStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>The record of a tiered occupancy rule and its current status.</summary>
public sealed class TieredOccupancyRule(string code, string label, int tenureMonths, double maximum, string locale, string notes, string reviewer, int version, TieredOccupancyRuleStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public int TenureMonths { get; set; } = tenureMonths;
    public double Maximum { get; set; } = maximum;
    public string Locale { get; set; } = locale;
    public string Notes { get; set; } = notes;
    public string Reviewer { get; set; } = reviewer;
    public int Version { get; set; } = version;
    public TieredOccupancyRuleStatus Status { get; set; } = status;

    /// <summary>Starts a tiered occupancy rule in status created.</summary>
    public static TieredOccupancyRule NewTieredOccupancyRule(string code, string label)
    {
        return new TieredOccupancyRule(code, label, 0, 0.0, "", "", "", 0, TieredOccupancyRuleStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case TieredOccupancyRuleStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(TieredOccupancyRuleStatus next)
    {
        switch (Status)
        {
            case TieredOccupancyRuleStatus.Created:
                return next == TieredOccupancyRuleStatus.Validated;
            case TieredOccupancyRuleStatus.Validated:
                switch (next)
                {
                    case TieredOccupancyRuleStatus.Published:
                    case TieredOccupancyRuleStatus.Retired:
                        return true;
                }
                return false;
            case TieredOccupancyRuleStatus.Published:
                return next == TieredOccupancyRuleStatus.Retired;
        }
        return false;
    }

    public void MoveTo(TieredOccupancyRuleStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"tiered occupancy rule {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        Version += 1;
    }

    public string DisplayLabel()
    {
        return $"{"tiered occupancy rule"} {Code}";
    }

    public double ProjectedExposure()
    {
        return Math.Round((double)Version + Maximum, 2);
    }

    public TieredOccupancyRule Copy()
    {
        return new TieredOccupancyRule(Code, Label, TenureMonths, Maximum, Locale, Notes, Reviewer, Version, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Locale);
    }
}
