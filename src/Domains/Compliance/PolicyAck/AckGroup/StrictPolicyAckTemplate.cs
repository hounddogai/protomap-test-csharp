namespace Acme.Workforce.Compliance.PolicyAck.AckGroup;

using Acme.Workforce.Kit.Text;

/// <summary>Where a strict policy ack template is in its life cycle.</summary>
public enum StrictPolicyAckTemplateStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>The record of a strict policy ack template and its current status.</summary>
public sealed class StrictPolicyAckTemplate(string code, string label, double rating, string unit, double ratio, string name, bool visible, List<string> tags, string managerId, double cost, StrictPolicyAckTemplateStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public double Rating { get; set; } = rating;
    public string Unit { get; set; } = unit;
    public double Ratio { get; set; } = ratio;
    public string Name { get; set; } = name;
    public bool Visible { get; set; } = visible;
    public List<string> Tags { get; set; } = tags;
    public string ManagerId { get; set; } = managerId;
    public double Cost { get; set; } = cost;
    public StrictPolicyAckTemplateStatus Status { get; set; } = status;

    /// <summary>Starts a strict policy ack template in status proposed.</summary>
    public static StrictPolicyAckTemplate NewStrictPolicyAckTemplate(string code, string label)
    {
        return new StrictPolicyAckTemplate(
            code,
            label,
            0.0,
            "",
            0.0,
            "",
            false,
            new List<string>(),
            "",
            0.0,
            StrictPolicyAckTemplateStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case StrictPolicyAckTemplateStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(StrictPolicyAckTemplateStatus next)
    {
        switch (Status)
        {
            case StrictPolicyAckTemplateStatus.Proposed:
                switch (next)
                {
                    case StrictPolicyAckTemplateStatus.Rejected:
                        return true;
                }
                return false;
            case StrictPolicyAckTemplateStatus.Approved:
                return next == StrictPolicyAckTemplateStatus.Rejected || next == StrictPolicyAckTemplateStatus.Withdrawn;
            case StrictPolicyAckTemplateStatus.Rejected:
                switch (next)
                {
                    case StrictPolicyAckTemplateStatus.Withdrawn:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(StrictPolicyAckTemplateStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"strict policy ack template {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code}: {Label}";
    }

    public bool HasTag(string tag)
    {
        foreach (var candidate in Tags)
        {
            if (Text.Normalize(candidate) == Text.Normalize(tag))
            {
                return true;
            }
        }
        return false;
    }

    public double EffectiveValue()
    {
        return Math.Round(Cost * Ratio, 2);
    }

    public StrictPolicyAckTemplate Copy()
    {
        return new StrictPolicyAckTemplate(
            Code,
            Label,
            Rating,
            Unit,
            Ratio,
            Name,
            Visible,
            Tags,
            ManagerId,
            Cost,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Unit);
    }
}
