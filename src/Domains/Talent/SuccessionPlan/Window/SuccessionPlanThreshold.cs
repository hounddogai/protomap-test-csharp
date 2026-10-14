namespace Acme.Workforce.Talent.SuccessionPlan.Window;

using Acme.Workforce.Kit.Text;

/// <summary>Where a succession plan threshold is in its life cycle.</summary>
public enum SuccessionPlanThresholdStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>One succession plan threshold, with what the planners need to act on it.</summary>
public sealed class SuccessionPlanThreshold(string code, string title, string contactEmail, double fte, string costCenter, int capacity, string externalRef, string jobCode, string locale, double ceiling, string department, SuccessionPlanThresholdStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string ContactEmail { get; set; } = contactEmail;
    public double Fte { get; set; } = fte;
    public string CostCenter { get; set; } = costCenter;
    public int Capacity { get; set; } = capacity;
    public string ExternalRef { get; set; } = externalRef;
    public string JobCode { get; set; } = jobCode;
    public string Locale { get; set; } = locale;
    public double Ceiling { get; set; } = ceiling;
    public string Department { get; set; } = department;
    public SuccessionPlanThresholdStatus Status { get; set; } = status;

    /// <summary>Starts a succession plan threshold in status proposed.</summary>
    public static SuccessionPlanThreshold NewSuccessionPlanThreshold(string code, string title)
    {
        return new SuccessionPlanThreshold(
            code,
            title,
            "",
            0.0,
            "",
            0,
            "",
            "",
            "",
            0.0,
            "",
            SuccessionPlanThresholdStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case SuccessionPlanThresholdStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(SuccessionPlanThresholdStatus next)
    {
        switch (Status)
        {
            case SuccessionPlanThresholdStatus.Proposed:
                return next == SuccessionPlanThresholdStatus.Rejected || next == SuccessionPlanThresholdStatus.Withdrawn;
            case SuccessionPlanThresholdStatus.Approved:
                switch (next)
                {
                    case SuccessionPlanThresholdStatus.Rejected:
                    case SuccessionPlanThresholdStatus.Withdrawn:
                        return true;
                }
                return false;
            case SuccessionPlanThresholdStatus.Rejected:
                return next == SuccessionPlanThresholdStatus.Withdrawn;
        }
        return false;
    }

    public void MoveTo(SuccessionPlanThresholdStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"succession plan threshold {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} ({Title})";
    }

    public double EffectiveValue()
    {
        return Math.Round(Ceiling + (double)Capacity, 2);
    }

    public SuccessionPlanThreshold Copy()
    {
        return new SuccessionPlanThreshold(
            Code,
            Title,
            ContactEmail,
            Fte,
            CostCenter,
            Capacity,
            ExternalRef,
            JobCode,
            Locale,
            Ceiling,
            Department,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Locale);
    }
}
