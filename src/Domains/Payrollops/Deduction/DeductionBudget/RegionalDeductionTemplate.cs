namespace Acme.Workforce.Payrollops.Deduction.DeductionBudget;

using Acme.Workforce.Kit.Text;

/// <summary>Where a regional deduction template is in its life cycle.</summary>
public enum RegionalDeductionTemplateStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>A regional deduction template as the deduction team keeps it.</summary>
public sealed class RegionalDeductionTemplate(string code, string name, string summary, string externalRef, double minimum, double rate, RegionalDeductionTemplateStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string Summary { get; set; } = summary;
    public string ExternalRef { get; set; } = externalRef;
    public double Minimum { get; set; } = minimum;
    public double Rate { get; set; } = rate;
    public RegionalDeductionTemplateStatus Status { get; set; } = status;

    /// <summary>Starts a regional deduction template in status planned.</summary>
    public static RegionalDeductionTemplate NewRegionalDeductionTemplate(string code, string name)
    {
        return new RegionalDeductionTemplate(code, name, "", "", 0.0, 0.0, RegionalDeductionTemplateStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case RegionalDeductionTemplateStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(RegionalDeductionTemplateStatus next)
    {
        switch (Status)
        {
            case RegionalDeductionTemplateStatus.Planned:
                switch (next)
                {
                    case RegionalDeductionTemplateStatus.Completed:
                        return true;
                }
                return false;
            case RegionalDeductionTemplateStatus.Scheduled:
                return next == RegionalDeductionTemplateStatus.Completed;
            case RegionalDeductionTemplateStatus.InProgress:
                switch (next)
                {
                    case RegionalDeductionTemplateStatus.Completed:
                    case RegionalDeductionTemplateStatus.Cancelled:
                        return true;
                }
                return false;
            case RegionalDeductionTemplateStatus.Completed:
                return next == RegionalDeductionTemplateStatus.Cancelled;
        }
        return false;
    }

    public void MoveTo(RegionalDeductionTemplateStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"regional deduction template {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} / {Name}";
    }

    public double EffectiveExposure()
    {
        return Math.Round(Minimum * Rate, 2);
    }

    public RegionalDeductionTemplate Copy()
    {
        return new RegionalDeductionTemplate(Code, Name, Summary, ExternalRef, Minimum, Rate, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
