namespace Acme.Workforce.Talent.SuccessionPlan.Score;

using Acme.Workforce.Kit.Text;

/// <summary>Where a strict succession plan score is in its life cycle.</summary>
public enum StrictSuccessionPlanScoreStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>A strict succession plan score as the succession plan team keeps it.</summary>
public sealed class StrictSuccessionPlanScore(string code, string name, double ceiling, string currency, string contactEmail, string ownerId, string label, double cost, int durationMinutes, int graceDays, StrictSuccessionPlanScoreStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public double Ceiling { get; set; } = ceiling;
    public string Currency { get; set; } = currency;
    public string ContactEmail { get; set; } = contactEmail;
    public string OwnerId { get; set; } = ownerId;
    public string Label { get; set; } = label;
    public double Cost { get; set; } = cost;
    public int DurationMinutes { get; set; } = durationMinutes;
    public int GraceDays { get; set; } = graceDays;
    public StrictSuccessionPlanScoreStatus Status { get; set; } = status;

    /// <summary>Starts a strict succession plan score in status proposed.</summary>
    public static StrictSuccessionPlanScore NewStrictSuccessionPlanScore(string code, string name)
    {
        return new StrictSuccessionPlanScore(
            code,
            name,
            0.0,
            "",
            "",
            "",
            "",
            0.0,
            0,
            0,
            StrictSuccessionPlanScoreStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case StrictSuccessionPlanScoreStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(StrictSuccessionPlanScoreStatus next)
    {
        switch (Status)
        {
            case StrictSuccessionPlanScoreStatus.Proposed:
                return next == StrictSuccessionPlanScoreStatus.Approved || next == StrictSuccessionPlanScoreStatus.Withdrawn;
            case StrictSuccessionPlanScoreStatus.Approved:
                return next == StrictSuccessionPlanScoreStatus.Rejected || next == StrictSuccessionPlanScoreStatus.Withdrawn;
            case StrictSuccessionPlanScoreStatus.Rejected:
                return next == StrictSuccessionPlanScoreStatus.Withdrawn;
        }
        return false;
    }

    public void MoveTo(StrictSuccessionPlanScoreStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"strict succession plan score {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{"strict succession plan score"} {Code}";
    }

    public double ProjectedVolume()
    {
        return Math.Round(Cost * Ceiling, 2);
    }

    public StrictSuccessionPlanScore Copy()
    {
        return new StrictSuccessionPlanScore(
            Code,
            Name,
            Ceiling,
            Currency,
            ContactEmail,
            OwnerId,
            Label,
            Cost,
            DurationMinutes,
            GraceDays,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
