namespace Acme.Workforce.Contractors.Engagement.Assignment;

using Acme.Workforce.Kit.Text;

/// <summary>Where a engagement preference is in its life cycle.</summary>
public enum EngagementPreferenceStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>A engagement preference. Its code is unique within a tenant.</summary>
public sealed class EngagementPreference(string code, string name, double rate, string summary, string salaryBand, double minimum, string department, double ratio, string externalRef, EngagementPreferenceStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public double Rate { get; set; } = rate;
    public string Summary { get; set; } = summary;
    public string SalaryBand { get; set; } = salaryBand;
    public double Minimum { get; set; } = minimum;
    public string Department { get; set; } = department;
    public double Ratio { get; set; } = ratio;
    public string ExternalRef { get; set; } = externalRef;
    public EngagementPreferenceStatus Status { get; set; } = status;

    /// <summary>Starts a engagement preference in status proposed.</summary>
    public static EngagementPreference NewEngagementPreference(string code, string name)
    {
        return new EngagementPreference(
            code,
            name,
            0.0,
            "",
            "",
            0.0,
            "",
            0.0,
            "",
            EngagementPreferenceStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case EngagementPreferenceStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(EngagementPreferenceStatus next)
    {
        switch (Status)
        {
            case EngagementPreferenceStatus.Proposed:
                return next == EngagementPreferenceStatus.Rejected || next == EngagementPreferenceStatus.Withdrawn;
            case EngagementPreferenceStatus.Approved:
                return next == EngagementPreferenceStatus.Rejected || next == EngagementPreferenceStatus.Withdrawn;
            case EngagementPreferenceStatus.Rejected:
                return next == EngagementPreferenceStatus.Withdrawn;
        }
        return false;
    }

    public void MoveTo(EngagementPreferenceStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"engagement preference {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} / {Name}";
    }

    public double AdjustedVolume()
    {
        return Math.Round(Rate * Minimum, 2);
    }

    public EngagementPreference Copy()
    {
        return new EngagementPreference(
            Code,
            Name,
            Rate,
            Summary,
            SalaryBand,
            Minimum,
            Department,
            Ratio,
            ExternalRef,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Department);
    }
}
