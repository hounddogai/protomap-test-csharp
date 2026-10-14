namespace Acme.Workforce.Talent.Nomination.Milestone;

using Acme.Workforce.Kit.Text;

/// <summary>Where a capped nomination plan is in its life cycle.</summary>
public enum CappedNominationPlanStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>The record of a capped nomination plan and its current status.</summary>
public sealed class CappedNominationPlan(string code, string label, string notes, string summary, int expiresAt, int revision, string team, string timezone, string jobCode, Dictionary<string, string> attributes, CappedNominationPlanStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Notes { get; set; } = notes;
    public string Summary { get; set; } = summary;
    public int ExpiresAt { get; set; } = expiresAt;
    public int Revision { get; set; } = revision;
    public string Team { get; set; } = team;
    public string Timezone { get; set; } = timezone;
    public string JobCode { get; set; } = jobCode;
    public Dictionary<string, string> Attributes { get; set; } = attributes;
    public CappedNominationPlanStatus Status { get; set; } = status;

    /// <summary>Starts a capped nomination plan in status idle.</summary>
    public static CappedNominationPlan NewCappedNominationPlan(string code, string label)
    {
        return new CappedNominationPlan(
            code,
            label,
            "",
            "",
            0,
            0,
            "",
            "",
            "",
            new Dictionary<string, string>(),
            CappedNominationPlanStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case CappedNominationPlanStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(CappedNominationPlanStatus next)
    {
        switch (Status)
        {
            case CappedNominationPlanStatus.Idle:
                return next == CappedNominationPlanStatus.Warming || next == CappedNominationPlanStatus.Stopped;
            case CappedNominationPlanStatus.Warming:
                switch (next)
                {
                    case CappedNominationPlanStatus.Draining:
                        return true;
                }
                return false;
            case CappedNominationPlanStatus.Ready:
                return next == CappedNominationPlanStatus.Draining;
            case CappedNominationPlanStatus.Draining:
                return next == CappedNominationPlanStatus.Stopped;
        }
        return false;
    }

    public void MoveTo(CappedNominationPlanStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"capped nomination plan {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        ExpiresAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        Revision += 1;
    }

    public string DisplayLabel()
    {
        return $"{"capped nomination plan"} {Code}";
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

    public double ProjectedExposure()
    {
        return Math.Round((double)Revision * (double)ExpiresAt, 2);
    }

    public CappedNominationPlan Copy()
    {
        return new CappedNominationPlan(
            Code,
            Label,
            Notes,
            Summary,
            ExpiresAt,
            Revision,
            Team,
            Timezone,
            JobCode,
            Attributes,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Team);
    }
}
