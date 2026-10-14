namespace Acme.Workforce.Absence.Policy.Notice;

using Acme.Workforce.Kit.Text;

/// <summary>Where a policy draft is in its life cycle.</summary>
public enum PolicyDraftStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>A policy draft. Its code is unique within a tenant.</summary>
public sealed class PolicyDraft(string code, string label, int priority, string channel, double budget, int capacity, string unit, bool visible, string team, PolicyDraftStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public int Priority { get; set; } = priority;
    public string Channel { get; set; } = channel;
    public double Budget { get; set; } = budget;
    public int Capacity { get; set; } = capacity;
    public string Unit { get; set; } = unit;
    public bool Visible { get; set; } = visible;
    public string Team { get; set; } = team;
    public PolicyDraftStatus Status { get; set; } = status;

    /// <summary>Starts a policy draft in status requested.</summary>
    public static PolicyDraft NewPolicyDraft(string code, string label)
    {
        return new PolicyDraft(code, label, 0, "", 0.0, 0, "", false, "", PolicyDraftStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PolicyDraftStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PolicyDraftStatus next)
    {
        switch (Status)
        {
            case PolicyDraftStatus.Requested:
                switch (next)
                {
                    case PolicyDraftStatus.Granted:
                    case PolicyDraftStatus.Expired:
                        return true;
                }
                return false;
            case PolicyDraftStatus.Granted:
                return next == PolicyDraftStatus.Revoked || next == PolicyDraftStatus.Expired;
            case PolicyDraftStatus.Revoked:
                return next == PolicyDraftStatus.Expired;
        }
        return false;
    }

    public void MoveTo(PolicyDraftStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"policy draft {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code} / {Label}";
    }

    public double ProjectedExposure()
    {
        return Math.Round((double)Priority - (double)Capacity, 2);
    }

    public PolicyDraft Copy()
    {
        return new PolicyDraft(Code, Label, Priority, Channel, Budget, Capacity, Unit, Visible, Team, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Channel);
    }
}
