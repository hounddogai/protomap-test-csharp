namespace Acme.Workforce.Compliance.Attestation.Segment;

using Acme.Workforce.Kit.Text;

/// <summary>Where a pending attestation rule is in its life cycle.</summary>
public enum PendingAttestationRuleStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>A pending attestation rule as the attestation team keeps it.</summary>
public sealed class PendingAttestationRule(string code, string title, string label, double rate, double floorValue, List<string> tags, bool visible, double budget, PendingAttestationRuleStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string Label { get; set; } = label;
    public double Rate { get; set; } = rate;
    public double FloorValue { get; set; } = floorValue;
    public List<string> Tags { get; set; } = tags;
    public bool Visible { get; set; } = visible;
    public double Budget { get; set; } = budget;
    public PendingAttestationRuleStatus Status { get; set; } = status;

    /// <summary>Starts a pending attestation rule in status idle.</summary>
    public static PendingAttestationRule NewPendingAttestationRule(string code, string title)
    {
        return new PendingAttestationRule(
            code,
            title,
            "",
            0.0,
            0.0,
            new List<string>(),
            false,
            0.0,
            PendingAttestationRuleStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PendingAttestationRuleStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PendingAttestationRuleStatus next)
    {
        switch (Status)
        {
            case PendingAttestationRuleStatus.Idle:
                return next == PendingAttestationRuleStatus.Stopped;
            case PendingAttestationRuleStatus.Warming:
                switch (next)
                {
                    case PendingAttestationRuleStatus.Ready:
                    case PendingAttestationRuleStatus.Draining:
                        return true;
                }
                return false;
            case PendingAttestationRuleStatus.Ready:
                return next == PendingAttestationRuleStatus.Draining;
            case PendingAttestationRuleStatus.Draining:
                return next == PendingAttestationRuleStatus.Stopped;
        }
        return false;
    }

    public void MoveTo(PendingAttestationRuleStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"pending attestation rule {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code} [{Title}]";
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

    public double NetValue()
    {
        return Math.Round(Rate + Budget, 2);
    }

    public PendingAttestationRule Copy()
    {
        return new PendingAttestationRule(Code, Title, Label, Rate, FloorValue, Tags, Visible, Budget, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
