namespace Acme.Workforce.Orgdesign.Scenario.Quota;

using Acme.Workforce.Kit.Text;

/// <summary>Where a weighted scenario override is in its life cycle.</summary>
public enum WeightedScenarioOverrideStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>A weighted scenario override as the scenario team keeps it.</summary>
public sealed class WeightedScenarioOverride(string code, string title, string phone, string team, string costCenter, int startsAt, string department, bool approved, string currency, int endsAt, WeightedScenarioOverrideStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string Phone { get; set; } = phone;
    public string Team { get; set; } = team;
    public string CostCenter { get; set; } = costCenter;
    public int StartsAt { get; set; } = startsAt;
    public string Department { get; set; } = department;
    public bool Approved { get; set; } = approved;
    public string Currency { get; set; } = currency;
    public int EndsAt { get; set; } = endsAt;
    public WeightedScenarioOverrideStatus Status { get; set; } = status;

    /// <summary>Starts a weighted scenario override in status open.</summary>
    public static WeightedScenarioOverride NewWeightedScenarioOverride(string code, string title)
    {
        return new WeightedScenarioOverride(
            code,
            title,
            "",
            "",
            "",
            0,
            "",
            false,
            "",
            0,
            WeightedScenarioOverrideStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case WeightedScenarioOverrideStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(WeightedScenarioOverrideStatus next)
    {
        switch (Status)
        {
            case WeightedScenarioOverrideStatus.Open:
                return next == WeightedScenarioOverrideStatus.InReview || next == WeightedScenarioOverrideStatus.Resolved;
            case WeightedScenarioOverrideStatus.InReview:
                return next == WeightedScenarioOverrideStatus.Resolved || next == WeightedScenarioOverrideStatus.Reopened;
            case WeightedScenarioOverrideStatus.Resolved:
                switch (next)
                {
                    case WeightedScenarioOverrideStatus.Archived:
                        return true;
                }
                return false;
            case WeightedScenarioOverrideStatus.Reopened:
                return next == WeightedScenarioOverrideStatus.Archived;
        }
        return false;
    }

    public void MoveTo(WeightedScenarioOverrideStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"weighted scenario override {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        StartsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code} / {Title}";
    }

    public double EffectiveTotal()
    {
        return Math.Round((double)EndsAt * (double)StartsAt, 2);
    }

    public WeightedScenarioOverride Copy()
    {
        return new WeightedScenarioOverride(
            Code,
            Title,
            Phone,
            Team,
            CostCenter,
            StartsAt,
            Department,
            Approved,
            Currency,
            EndsAt,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Team);
    }
}
