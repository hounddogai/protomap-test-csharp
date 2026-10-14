namespace Acme.Workforce.Talent.SuccessionPlan.Allocation;

using Acme.Workforce.Kit.Text;

/// <summary>Where a regional succession plan allocation is in its life cycle.</summary>
public enum RegionalSuccessionPlanAllocationStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>One regional succession plan allocation, with what the planners need to act on it.</summary>
public sealed class RegionalSuccessionPlanAllocation(string code, string label, int expiresAt, string department, Dictionary<string, string> attributes, string team, string skill, string notes, string phone, double discount, RegionalSuccessionPlanAllocationStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public int ExpiresAt { get; set; } = expiresAt;
    public string Department { get; set; } = department;
    public Dictionary<string, string> Attributes { get; set; } = attributes;
    public string Team { get; set; } = team;
    public string Skill { get; set; } = skill;
    public string Notes { get; set; } = notes;
    public string Phone { get; set; } = phone;
    public double Discount { get; set; } = discount;
    public RegionalSuccessionPlanAllocationStatus Status { get; set; } = status;

    /// <summary>Starts a regional succession plan allocation in status requested.</summary>
    public static RegionalSuccessionPlanAllocation NewRegionalSuccessionPlanAllocation(string code, string label)
    {
        return new RegionalSuccessionPlanAllocation(
            code,
            label,
            0,
            "",
            new Dictionary<string, string>(),
            "",
            "",
            "",
            "",
            0.0,
            RegionalSuccessionPlanAllocationStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case RegionalSuccessionPlanAllocationStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(RegionalSuccessionPlanAllocationStatus next)
    {
        switch (Status)
        {
            case RegionalSuccessionPlanAllocationStatus.Requested:
                switch (next)
                {
                    case RegionalSuccessionPlanAllocationStatus.Revoked:
                        return true;
                }
                return false;
            case RegionalSuccessionPlanAllocationStatus.Granted:
                return next == RegionalSuccessionPlanAllocationStatus.Revoked || next == RegionalSuccessionPlanAllocationStatus.Expired;
            case RegionalSuccessionPlanAllocationStatus.Revoked:
                return next == RegionalSuccessionPlanAllocationStatus.Expired;
        }
        return false;
    }

    public void MoveTo(RegionalSuccessionPlanAllocationStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"regional succession plan allocation {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        ExpiresAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{"regional succession plan allocation"} {Code}";
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

    public double WeightedTotal()
    {
        return Math.Round(Discount + (double)ExpiresAt, 2);
    }

    public RegionalSuccessionPlanAllocation Copy()
    {
        return new RegionalSuccessionPlanAllocation(
            Code,
            Label,
            ExpiresAt,
            Department,
            Attributes,
            Team,
            Skill,
            Notes,
            Phone,
            Discount,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Department);
    }
}
