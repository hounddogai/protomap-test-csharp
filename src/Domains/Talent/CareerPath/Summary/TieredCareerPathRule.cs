namespace Acme.Workforce.Talent.CareerPath.Summary;

using Acme.Workforce.Kit.Text;

/// <summary>Where a tiered career path rule is in its life cycle.</summary>
public enum TieredCareerPathRuleStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>A tiered career path rule as the career path team keeps it.</summary>
public sealed class TieredCareerPathRule(string code, string label, string checksum, double weight, int sequence, string employeeId, TieredCareerPathRuleStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Checksum { get; set; } = checksum;
    public double Weight { get; set; } = weight;
    public int Sequence { get; set; } = sequence;
    public string EmployeeId { get; set; } = employeeId;
    public TieredCareerPathRuleStatus Status { get; set; } = status;

    /// <summary>Starts a tiered career path rule in status proposed.</summary>
    public static TieredCareerPathRule NewTieredCareerPathRule(string code, string label)
    {
        return new TieredCareerPathRule(code, label, "", 0.0, 0, "", TieredCareerPathRuleStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case TieredCareerPathRuleStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(TieredCareerPathRuleStatus next)
    {
        switch (Status)
        {
            case TieredCareerPathRuleStatus.Proposed:
                return next == TieredCareerPathRuleStatus.Approved;
            case TieredCareerPathRuleStatus.Approved:
                return next == TieredCareerPathRuleStatus.Rejected;
            case TieredCareerPathRuleStatus.Rejected:
                return next == TieredCareerPathRuleStatus.Withdrawn;
        }
        return false;
    }

    public void MoveTo(TieredCareerPathRuleStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"tiered career path rule {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code} / {Label}";
    }

    public double AdjustedLoad()
    {
        return Math.Round(Weight + (double)Sequence, 2);
    }

    public TieredCareerPathRule Copy()
    {
        return new TieredCareerPathRule(Code, Label, Checksum, Weight, Sequence, EmployeeId, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
