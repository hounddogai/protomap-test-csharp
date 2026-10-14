namespace Acme.Workforce.Payrollops.Payslip.Rollup;

using Acme.Workforce.Kit.Text;

/// <summary>Where a seasonal payslip baseline is in its life cycle.</summary>
public enum SeasonalPayslipBaselineStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>The record of a seasonal payslip baseline and its current status.</summary>
public sealed class SeasonalPayslipBaseline(string code, string label, int capacity, string department, string currency, string skill, string country, double weight, Dictionary<string, string> attributes, string grade, string contactEmail, SeasonalPayslipBaselineStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public int Capacity { get; set; } = capacity;
    public string Department { get; set; } = department;
    public string Currency { get; set; } = currency;
    public string Skill { get; set; } = skill;
    public string Country { get; set; } = country;
    public double Weight { get; set; } = weight;
    public Dictionary<string, string> Attributes { get; set; } = attributes;
    public string Grade { get; set; } = grade;
    public string ContactEmail { get; set; } = contactEmail;
    public SeasonalPayslipBaselineStatus Status { get; set; } = status;

    /// <summary>Starts a seasonal payslip baseline in status planned.</summary>
    public static SeasonalPayslipBaseline NewSeasonalPayslipBaseline(string code, string label)
    {
        return new SeasonalPayslipBaseline(
            code,
            label,
            0,
            "",
            "",
            "",
            "",
            0.0,
            new Dictionary<string, string>(),
            "",
            "",
            SeasonalPayslipBaselineStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case SeasonalPayslipBaselineStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(SeasonalPayslipBaselineStatus next)
    {
        switch (Status)
        {
            case SeasonalPayslipBaselineStatus.Planned:
                switch (next)
                {
                    case SeasonalPayslipBaselineStatus.Scheduled:
                    case SeasonalPayslipBaselineStatus.Completed:
                        return true;
                }
                return false;
            case SeasonalPayslipBaselineStatus.Scheduled:
                switch (next)
                {
                    case SeasonalPayslipBaselineStatus.Completed:
                    case SeasonalPayslipBaselineStatus.Cancelled:
                        return true;
                }
                return false;
            case SeasonalPayslipBaselineStatus.InProgress:
                switch (next)
                {
                    case SeasonalPayslipBaselineStatus.Cancelled:
                        return true;
                }
                return false;
            case SeasonalPayslipBaselineStatus.Completed:
                switch (next)
                {
                    case SeasonalPayslipBaselineStatus.Cancelled:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(SeasonalPayslipBaselineStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"seasonal payslip baseline {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{"seasonal payslip baseline"} {Code}";
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

    public double EffectiveValue()
    {
        return Math.Round((double)Capacity * Weight, 2);
    }

    public SeasonalPayslipBaseline Copy()
    {
        return new SeasonalPayslipBaseline(
            Code,
            Label,
            Capacity,
            Department,
            Currency,
            Skill,
            Country,
            Weight,
            Attributes,
            Grade,
            ContactEmail,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Department);
    }
}
