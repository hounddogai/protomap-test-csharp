namespace Acme.Workforce.Workplace.AccessBadge.Audit;

using Acme.Workforce.Kit.Text;

/// <summary>Where a access badge audit is in its life cycle.</summary>
public enum AccessBadgeAuditStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>A access badge audit as the access badge team keeps it.</summary>
public sealed class AccessBadgeAudit(string code, string name, string unit, int quantity, string skill, string employeeId, double weight, int retries, int createdAt, Dictionary<string, string> attributes, double amount, AccessBadgeAuditStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string Unit { get; set; } = unit;
    public int Quantity { get; set; } = quantity;
    public string Skill { get; set; } = skill;
    public string EmployeeId { get; set; } = employeeId;
    public double Weight { get; set; } = weight;
    public int Retries { get; set; } = retries;
    public int CreatedAt { get; set; } = createdAt;
    public Dictionary<string, string> Attributes { get; set; } = attributes;
    public double Amount { get; set; } = amount;
    public AccessBadgeAuditStatus Status { get; set; } = status;

    /// <summary>Starts a access badge audit in status proposed.</summary>
    public static AccessBadgeAudit NewAccessBadgeAudit(string code, string name)
    {
        return new AccessBadgeAudit(
            code,
            name,
            "",
            0,
            "",
            "",
            0.0,
            0,
            0,
            new Dictionary<string, string>(),
            0.0,
            AccessBadgeAuditStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case AccessBadgeAuditStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(AccessBadgeAuditStatus next)
    {
        switch (Status)
        {
            case AccessBadgeAuditStatus.Proposed:
                return next == AccessBadgeAuditStatus.Rejected;
            case AccessBadgeAuditStatus.Approved:
                return next == AccessBadgeAuditStatus.Rejected || next == AccessBadgeAuditStatus.Withdrawn;
            case AccessBadgeAuditStatus.Rejected:
                return next == AccessBadgeAuditStatus.Withdrawn;
        }
        return false;
    }

    public void MoveTo(AccessBadgeAuditStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"access badge audit {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        CreatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code} / {Name}";
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

    public double EffectiveTotal()
    {
        return Math.Round(Weight * (double)Quantity, 2);
    }

    public AccessBadgeAudit Copy()
    {
        return new AccessBadgeAudit(
            Code,
            Name,
            Unit,
            Quantity,
            Skill,
            EmployeeId,
            Weight,
            Retries,
            CreatedAt,
            Attributes,
            Amount,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Unit);
    }
}
