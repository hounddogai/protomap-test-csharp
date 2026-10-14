namespace Acme.Workforce.Compliance.PolicyAck.Quota;

using Acme.Workforce.Kit.Text;

/// <summary>Where a default policy ack override is in its life cycle.</summary>
public enum DefaultPolicyAckOverrideStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>The record of a default policy ack override and its current status.</summary>
public sealed class DefaultPolicyAckOverride(string code, string label, double minimum, string summary, double margin, double fte, double discount, int endsAt, DefaultPolicyAckOverrideStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public double Minimum { get; set; } = minimum;
    public string Summary { get; set; } = summary;
    public double Margin { get; set; } = margin;
    public double Fte { get; set; } = fte;
    public double Discount { get; set; } = discount;
    public int EndsAt { get; set; } = endsAt;
    public DefaultPolicyAckOverrideStatus Status { get; set; } = status;

    /// <summary>Starts a default policy ack override in status proposed.</summary>
    public static DefaultPolicyAckOverride NewDefaultPolicyAckOverride(string code, string label)
    {
        return new DefaultPolicyAckOverride(
            code,
            label,
            0.0,
            "",
            0.0,
            0.0,
            0.0,
            0,
            DefaultPolicyAckOverrideStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case DefaultPolicyAckOverrideStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(DefaultPolicyAckOverrideStatus next)
    {
        switch (Status)
        {
            case DefaultPolicyAckOverrideStatus.Proposed:
                return next == DefaultPolicyAckOverrideStatus.Rejected || next == DefaultPolicyAckOverrideStatus.Withdrawn;
            case DefaultPolicyAckOverrideStatus.Approved:
                return next == DefaultPolicyAckOverrideStatus.Withdrawn;
            case DefaultPolicyAckOverrideStatus.Rejected:
                switch (next)
                {
                    case DefaultPolicyAckOverrideStatus.Withdrawn:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(DefaultPolicyAckOverrideStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"default policy ack override {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        EndsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{Code} [{Label}]";
    }

    public double WeightedVolume()
    {
        return Math.Round((double)EndsAt - Discount, 2);
    }

    public DefaultPolicyAckOverride Copy()
    {
        return new DefaultPolicyAckOverride(Code, Label, Minimum, Summary, Margin, Fte, Discount, EndsAt, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
