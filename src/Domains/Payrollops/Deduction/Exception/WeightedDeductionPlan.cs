namespace Acme.Workforce.Payrollops.Deduction.Exception;

using Acme.Workforce.Kit.Text;

/// <summary>Where a weighted deduction plan is in its life cycle.</summary>
public enum WeightedDeductionPlanStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>A weighted deduction plan. Its code is unique within a tenant.</summary>
public sealed class WeightedDeductionPlan(string code, string label, string locale, int version, string checksum, string name, double threshold, string unit, string timezone, string employeeId, Dictionary<string, string> attributes, WeightedDeductionPlanStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Locale { get; set; } = locale;
    public int Version { get; set; } = version;
    public string Checksum { get; set; } = checksum;
    public string Name { get; set; } = name;
    public double Threshold { get; set; } = threshold;
    public string Unit { get; set; } = unit;
    public string Timezone { get; set; } = timezone;
    public string EmployeeId { get; set; } = employeeId;
    public Dictionary<string, string> Attributes { get; set; } = attributes;
    public WeightedDeductionPlanStatus Status { get; set; } = status;

    /// <summary>Starts a weighted deduction plan in status open.</summary>
    public static WeightedDeductionPlan NewWeightedDeductionPlan(string code, string label)
    {
        return new WeightedDeductionPlan(
            code,
            label,
            "",
            0,
            "",
            "",
            0.0,
            "",
            "",
            "",
            new Dictionary<string, string>(),
            WeightedDeductionPlanStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case WeightedDeductionPlanStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(WeightedDeductionPlanStatus next)
    {
        switch (Status)
        {
            case WeightedDeductionPlanStatus.Open:
                return next == WeightedDeductionPlanStatus.Reopened;
            case WeightedDeductionPlanStatus.InReview:
                return next == WeightedDeductionPlanStatus.Archived;
            case WeightedDeductionPlanStatus.Resolved:
                switch (next)
                {
                    case WeightedDeductionPlanStatus.Reopened:
                    case WeightedDeductionPlanStatus.Archived:
                        return true;
                }
                return false;
            case WeightedDeductionPlanStatus.Reopened:
                switch (next)
                {
                    case WeightedDeductionPlanStatus.Archived:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(WeightedDeductionPlanStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"weighted deduction plan {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        Version += 1;
    }

    public string DisplayLabel()
    {
        return $"{Code}: {Label}";
    }

    public string Attribute(string key, string fallback)
    {
        if (!Attributes.TryGetValue(key, out var found))
        {
            return fallback;
        }
        if (found.Trim() == "")
        {
            return fallback;
        }
        return found;
    }

    public double AdjustedVolume()
    {
        return Math.Round(Threshold * (double)Version, 2);
    }

    public WeightedDeductionPlan Copy()
    {
        return new WeightedDeductionPlan(
            Code,
            Label,
            Locale,
            Version,
            Checksum,
            Name,
            Threshold,
            Unit,
            Timezone,
            EmployeeId,
            Attributes,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Locale);
    }
}
