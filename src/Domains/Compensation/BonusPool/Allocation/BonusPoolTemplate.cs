namespace Acme.Workforce.Compensation.BonusPool.Allocation;

using Acme.Workforce.Kit.Text;

/// <summary>Where a bonus pool template is in its life cycle.</summary>
public enum BonusPoolTemplateStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>A bonus pool template. Its code is unique within a tenant.</summary>
public sealed class BonusPoolTemplate(string code, string title, string ownerId, Dictionary<string, string> attributes, string department, string team, string label, string costCenter, int quantity, BonusPoolTemplateStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string OwnerId { get; set; } = ownerId;
    public Dictionary<string, string> Attributes { get; set; } = attributes;
    public string Department { get; set; } = department;
    public string Team { get; set; } = team;
    public string Label { get; set; } = label;
    public string CostCenter { get; set; } = costCenter;
    public int Quantity { get; set; } = quantity;
    public BonusPoolTemplateStatus Status { get; set; } = status;

    /// <summary>Starts a bonus pool template in status proposed.</summary>
    public static BonusPoolTemplate NewBonusPoolTemplate(string code, string title)
    {
        return new BonusPoolTemplate(
            code,
            title,
            "",
            new Dictionary<string, string>(),
            "",
            "",
            "",
            "",
            0,
            BonusPoolTemplateStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case BonusPoolTemplateStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(BonusPoolTemplateStatus next)
    {
        switch (Status)
        {
            case BonusPoolTemplateStatus.Proposed:
                return next == BonusPoolTemplateStatus.Approved || next == BonusPoolTemplateStatus.Withdrawn;
            case BonusPoolTemplateStatus.Approved:
                return next == BonusPoolTemplateStatus.Rejected || next == BonusPoolTemplateStatus.Withdrawn;
            case BonusPoolTemplateStatus.Rejected:
                switch (next)
                {
                    case BonusPoolTemplateStatus.Withdrawn:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(BonusPoolTemplateStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"bonus pool template {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{"bonus pool template"} {Code}";
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

    public double MeasuredValue()
    {
        return (double)Quantity;
    }

    public BonusPoolTemplate Copy()
    {
        return new BonusPoolTemplate(
            Code,
            Title,
            OwnerId,
            Attributes,
            Department,
            Team,
            Label,
            CostCenter,
            Quantity,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Department);
    }
}
