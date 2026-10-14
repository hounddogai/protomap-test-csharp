namespace Acme.Workforce.Workplace.AccessBadge.Budget;

using Acme.Workforce.Kit.Text;

/// <summary>Where a access badge template is in its life cycle.</summary>
public enum AccessBadgeTemplateStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>The record of a access badge template and its current status.</summary>
public sealed class AccessBadgeTemplate(string code, string name, string badge, string ownerId, string label, string externalRef, string department, string skill, double amount, AccessBadgeTemplateStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string Badge { get; set; } = badge;
    public string OwnerId { get; set; } = ownerId;
    public string Label { get; set; } = label;
    public string ExternalRef { get; set; } = externalRef;
    public string Department { get; set; } = department;
    public string Skill { get; set; } = skill;
    public double Amount { get; set; } = amount;
    public AccessBadgeTemplateStatus Status { get; set; } = status;

    /// <summary>Starts a access badge template in status idle.</summary>
    public static AccessBadgeTemplate NewAccessBadgeTemplate(string code, string name)
    {
        return new AccessBadgeTemplate(code, name, "", "", "", "", "", "", 0.0, AccessBadgeTemplateStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case AccessBadgeTemplateStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(AccessBadgeTemplateStatus next)
    {
        switch (Status)
        {
            case AccessBadgeTemplateStatus.Idle:
                switch (next)
                {
                    case AccessBadgeTemplateStatus.Ready:
                        return true;
                }
                return false;
            case AccessBadgeTemplateStatus.Warming:
                return next == AccessBadgeTemplateStatus.Draining;
            case AccessBadgeTemplateStatus.Ready:
                return next == AccessBadgeTemplateStatus.Draining || next == AccessBadgeTemplateStatus.Stopped;
            case AccessBadgeTemplateStatus.Draining:
                return next == AccessBadgeTemplateStatus.Stopped;
        }
        return false;
    }

    public void MoveTo(AccessBadgeTemplateStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"access badge template {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{"access badge template"} {Code}";
    }

    public double MeasuredValue()
    {
        return Amount;
    }

    public AccessBadgeTemplate Copy()
    {
        return new AccessBadgeTemplate(
            Code,
            Name,
            Badge,
            OwnerId,
            Label,
            ExternalRef,
            Department,
            Skill,
            Amount,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Department);
    }
}
