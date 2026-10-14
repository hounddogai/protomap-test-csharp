namespace Acme.Workforce.Talent.Nomination.Window;

using Acme.Workforce.Kit.Text;

/// <summary>Where a nomination score is in its life cycle.</summary>
public enum NominationScoreStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>A nomination score. Its code is unique within a tenant.</summary>
public sealed class NominationScore(string code, string label, string unit, string team, int endsAt, string region, string externalRef, string name, bool active, string source, bool archived, NominationScoreStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Unit { get; set; } = unit;
    public string Team { get; set; } = team;
    public int EndsAt { get; set; } = endsAt;
    public string Region { get; set; } = region;
    public string ExternalRef { get; set; } = externalRef;
    public string Name { get; set; } = name;
    public bool Active { get; set; } = active;
    public string Source { get; set; } = source;
    public bool Archived { get; set; } = archived;
    public NominationScoreStatus Status { get; set; } = status;

    /// <summary>Starts a nomination score in status open.</summary>
    public static NominationScore NewNominationScore(string code, string label)
    {
        return new NominationScore(code, label, "", "", 0, "", "", "", false, "", false, NominationScoreStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case NominationScoreStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(NominationScoreStatus next)
    {
        switch (Status)
        {
            case NominationScoreStatus.Open:
                switch (next)
                {
                    case NominationScoreStatus.Reopened:
                    case NominationScoreStatus.Archived:
                        return true;
                }
                return false;
            case NominationScoreStatus.InReview:
                return next == NominationScoreStatus.Archived;
            case NominationScoreStatus.Resolved:
                switch (next)
                {
                    case NominationScoreStatus.Reopened:
                    case NominationScoreStatus.Archived:
                        return true;
                }
                return false;
            case NominationScoreStatus.Reopened:
                switch (next)
                {
                    case NominationScoreStatus.Archived:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(NominationScoreStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"nomination score {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        EndsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{"nomination score"} {Code}";
    }

    public double MeasuredValue()
    {
        return (double)EndsAt;
    }

    public NominationScore Copy()
    {
        return new NominationScore(
            Code,
            Label,
            Unit,
            Team,
            EndsAt,
            Region,
            ExternalRef,
            Name,
            Active,
            Source,
            Archived,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Unit);
    }
}
