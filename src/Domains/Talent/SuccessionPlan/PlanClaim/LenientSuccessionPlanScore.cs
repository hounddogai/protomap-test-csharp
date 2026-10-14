namespace Acme.Workforce.Talent.SuccessionPlan.PlanClaim;

using Acme.Workforce.Kit.Text;

/// <summary>Where a lenient succession plan score is in its life cycle.</summary>
public enum LenientSuccessionPlanScoreStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>One lenient succession plan score, with what the planners need to act on it.</summary>
public sealed class LenientSuccessionPlanScore(string code, string label, int endsAt, string country, int limit, int revision, LenientSuccessionPlanScoreStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public int EndsAt { get; set; } = endsAt;
    public string Country { get; set; } = country;
    public int Limit { get; set; } = limit;
    public int Revision { get; set; } = revision;
    public LenientSuccessionPlanScoreStatus Status { get; set; } = status;

    /// <summary>Starts a lenient succession plan score in status requested.</summary>
    public static LenientSuccessionPlanScore NewLenientSuccessionPlanScore(string code, string label)
    {
        return new LenientSuccessionPlanScore(code, label, 0, "", 0, 0, LenientSuccessionPlanScoreStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case LenientSuccessionPlanScoreStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(LenientSuccessionPlanScoreStatus next)
    {
        switch (Status)
        {
            case LenientSuccessionPlanScoreStatus.Requested:
                switch (next)
                {
                    case LenientSuccessionPlanScoreStatus.Granted:
                    case LenientSuccessionPlanScoreStatus.Revoked:
                        return true;
                }
                return false;
            case LenientSuccessionPlanScoreStatus.Granted:
                switch (next)
                {
                    case LenientSuccessionPlanScoreStatus.Revoked:
                    case LenientSuccessionPlanScoreStatus.Expired:
                        return true;
                }
                return false;
            case LenientSuccessionPlanScoreStatus.Revoked:
                return next == LenientSuccessionPlanScoreStatus.Expired;
        }
        return false;
    }

    public void MoveTo(LenientSuccessionPlanScoreStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"lenient succession plan score {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        EndsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        Revision += 1;
    }

    public string DisplayLabel()
    {
        return $"{Code} [{Label}]";
    }

    public double EffectiveLoad()
    {
        return Math.Round((double)EndsAt - (double)Revision, 2);
    }

    public LenientSuccessionPlanScore Copy()
    {
        return new LenientSuccessionPlanScore(Code, Label, EndsAt, Country, Limit, Revision, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Country);
    }
}
