namespace Acme.Workforce.Talent.SuccessionPlan.Policy;

using Acme.Workforce.Kit.Text;

/// <summary>Where a legacy succession plan policy is in its life cycle.</summary>
public enum LegacySuccessionPlanPolicyStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>The record of a legacy succession plan policy and its current status.</summary>
public sealed class LegacySuccessionPlanPolicy(string code, string label, string country, string segment, string checksum, int sequence, int createdAt, int priority, int graceDays, bool approved, string salaryBand, LegacySuccessionPlanPolicyStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Country { get; set; } = country;
    public string Segment { get; set; } = segment;
    public string Checksum { get; set; } = checksum;
    public int Sequence { get; set; } = sequence;
    public int CreatedAt { get; set; } = createdAt;
    public int Priority { get; set; } = priority;
    public int GraceDays { get; set; } = graceDays;
    public bool Approved { get; set; } = approved;
    public string SalaryBand { get; set; } = salaryBand;
    public LegacySuccessionPlanPolicyStatus Status { get; set; } = status;

    /// <summary>Starts a legacy succession plan policy in status proposed.</summary>
    public static LegacySuccessionPlanPolicy NewLegacySuccessionPlanPolicy(string code, string label)
    {
        return new LegacySuccessionPlanPolicy(
            code,
            label,
            "",
            "",
            "",
            0,
            0,
            0,
            0,
            false,
            "",
            LegacySuccessionPlanPolicyStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case LegacySuccessionPlanPolicyStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(LegacySuccessionPlanPolicyStatus next)
    {
        switch (Status)
        {
            case LegacySuccessionPlanPolicyStatus.Proposed:
                switch (next)
                {
                    case LegacySuccessionPlanPolicyStatus.Approved:
                    case LegacySuccessionPlanPolicyStatus.Withdrawn:
                        return true;
                }
                return false;
            case LegacySuccessionPlanPolicyStatus.Approved:
                switch (next)
                {
                    case LegacySuccessionPlanPolicyStatus.Rejected:
                    case LegacySuccessionPlanPolicyStatus.Withdrawn:
                        return true;
                }
                return false;
            case LegacySuccessionPlanPolicyStatus.Rejected:
                return next == LegacySuccessionPlanPolicyStatus.Withdrawn;
        }
        return false;
    }

    public void MoveTo(LegacySuccessionPlanPolicyStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"legacy succession plan policy {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        CreatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{"legacy succession plan policy"} {Code}";
    }

    public double NetValue()
    {
        return Math.Round((double)Priority - (double)Sequence, 2);
    }

    public LegacySuccessionPlanPolicy Copy()
    {
        return new LegacySuccessionPlanPolicy(
            Code,
            Label,
            Country,
            Segment,
            Checksum,
            Sequence,
            CreatedAt,
            Priority,
            GraceDays,
            Approved,
            SalaryBand,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Country);
    }
}
