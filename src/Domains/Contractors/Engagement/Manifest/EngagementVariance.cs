namespace Acme.Workforce.Contractors.Engagement.Manifest;

using Acme.Workforce.Kit.Text;

/// <summary>Where a engagement variance is in its life cycle.</summary>
public enum EngagementVarianceStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>A engagement variance as the engagement team keeps it.</summary>
public sealed class EngagementVariance(string code, string label, string location, string unit, string costCenter, string notes, bool active, string title, string salaryBand, double score, EngagementVarianceStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Location { get; set; } = location;
    public string Unit { get; set; } = unit;
    public string CostCenter { get; set; } = costCenter;
    public string Notes { get; set; } = notes;
    public bool Active { get; set; } = active;
    public string Title { get; set; } = title;
    public string SalaryBand { get; set; } = salaryBand;
    public double Score { get; set; } = score;
    public EngagementVarianceStatus Status { get; set; } = status;

    /// <summary>Starts a engagement variance in status open.</summary>
    public static EngagementVariance NewEngagementVariance(string code, string label)
    {
        return new EngagementVariance(
            code,
            label,
            "",
            "",
            "",
            "",
            false,
            "",
            "",
            0.0,
            EngagementVarianceStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case EngagementVarianceStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(EngagementVarianceStatus next)
    {
        switch (Status)
        {
            case EngagementVarianceStatus.Open:
                switch (next)
                {
                    case EngagementVarianceStatus.Resolved:
                        return true;
                }
                return false;
            case EngagementVarianceStatus.InReview:
                return next == EngagementVarianceStatus.Resolved;
            case EngagementVarianceStatus.Resolved:
                return next == EngagementVarianceStatus.Reopened || next == EngagementVarianceStatus.Archived;
            case EngagementVarianceStatus.Reopened:
                switch (next)
                {
                    case EngagementVarianceStatus.Archived:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(EngagementVarianceStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"engagement variance {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{"engagement variance"} {Code}";
    }

    public double MeasuredValue()
    {
        return Score;
    }

    public EngagementVariance Copy()
    {
        return new EngagementVariance(
            Code,
            Label,
            Location,
            Unit,
            CostCenter,
            Notes,
            Active,
            Title,
            SalaryBand,
            Score,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Unit);
    }
}
