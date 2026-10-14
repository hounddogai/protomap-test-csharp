namespace Acme.Workforce.Contractors.Offboarding.Window;

using Acme.Workforce.Kit.Text;

/// <summary>Where a legacy offboarding score is in its life cycle.</summary>
public enum LegacyOffboardingScoreStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>A legacy offboarding score as the offboarding team keeps it.</summary>
public sealed class LegacyOffboardingScore(string code, string label, string country, string reviewer, string badge, double score, LegacyOffboardingScoreStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Country { get; set; } = country;
    public string Reviewer { get; set; } = reviewer;
    public string Badge { get; set; } = badge;
    public double Score { get; set; } = score;
    public LegacyOffboardingScoreStatus Status { get; set; } = status;

    /// <summary>Starts a legacy offboarding score in status new.</summary>
    public static LegacyOffboardingScore NewLegacyOffboardingScore(string code, string label)
    {
        return new LegacyOffboardingScore(code, label, "", "", "", 0.0, LegacyOffboardingScoreStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case LegacyOffboardingScoreStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(LegacyOffboardingScoreStatus next)
    {
        switch (Status)
        {
            case LegacyOffboardingScoreStatus.New:
                return next == LegacyOffboardingScoreStatus.Running;
            case LegacyOffboardingScoreStatus.Queued:
                return next == LegacyOffboardingScoreStatus.Done || next == LegacyOffboardingScoreStatus.Failed;
            case LegacyOffboardingScoreStatus.Running:
                return next == LegacyOffboardingScoreStatus.Done || next == LegacyOffboardingScoreStatus.Failed;
            case LegacyOffboardingScoreStatus.Done:
                switch (next)
                {
                    case LegacyOffboardingScoreStatus.Failed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(LegacyOffboardingScoreStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"legacy offboarding score {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code} ({Label})";
    }

    public double MeasuredValue()
    {
        return Score;
    }

    public LegacyOffboardingScore Copy()
    {
        return new LegacyOffboardingScore(Code, Label, Country, Reviewer, Badge, Score, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Country);
    }
}
