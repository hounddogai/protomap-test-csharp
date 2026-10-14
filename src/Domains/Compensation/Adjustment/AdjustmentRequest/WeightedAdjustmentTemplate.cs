namespace Acme.Workforce.Compensation.Adjustment.AdjustmentRequest;

using Acme.Workforce.Kit.Text;

/// <summary>Where a weighted adjustment template is in its life cycle.</summary>
public enum WeightedAdjustmentTemplateStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>One weighted adjustment template, with what the planners need to act on it.</summary>
public sealed class WeightedAdjustmentTemplate(string code, string label, string department, int version, double score, string source, string managerId, string salaryBand, WeightedAdjustmentTemplateStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Department { get; set; } = department;
    public int Version { get; set; } = version;
    public double Score { get; set; } = score;
    public string Source { get; set; } = source;
    public string ManagerId { get; set; } = managerId;
    public string SalaryBand { get; set; } = salaryBand;
    public WeightedAdjustmentTemplateStatus Status { get; set; } = status;

    /// <summary>Starts a weighted adjustment template in status open.</summary>
    public static WeightedAdjustmentTemplate NewWeightedAdjustmentTemplate(string code, string label)
    {
        return new WeightedAdjustmentTemplate(
            code,
            label,
            "",
            0,
            0.0,
            "",
            "",
            "",
            WeightedAdjustmentTemplateStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case WeightedAdjustmentTemplateStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(WeightedAdjustmentTemplateStatus next)
    {
        switch (Status)
        {
            case WeightedAdjustmentTemplateStatus.Open:
                switch (next)
                {
                    case WeightedAdjustmentTemplateStatus.InReview:
                    case WeightedAdjustmentTemplateStatus.Archived:
                        return true;
                }
                return false;
            case WeightedAdjustmentTemplateStatus.InReview:
                return next == WeightedAdjustmentTemplateStatus.Resolved || next == WeightedAdjustmentTemplateStatus.Archived;
            case WeightedAdjustmentTemplateStatus.Resolved:
                return next == WeightedAdjustmentTemplateStatus.Reopened;
            case WeightedAdjustmentTemplateStatus.Reopened:
                return next == WeightedAdjustmentTemplateStatus.Archived;
        }
        return false;
    }

    public void MoveTo(WeightedAdjustmentTemplateStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"weighted adjustment template {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        Version += 1;
    }

    public string DisplayLabel()
    {
        return $"{"weighted adjustment template"} {Code}";
    }

    public double AdjustedValue()
    {
        return Math.Round(Score - (double)Version, 2);
    }

    public WeightedAdjustmentTemplate Copy()
    {
        return new WeightedAdjustmentTemplate(
            Code,
            Label,
            Department,
            Version,
            Score,
            Source,
            ManagerId,
            SalaryBand,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Department);
    }
}
