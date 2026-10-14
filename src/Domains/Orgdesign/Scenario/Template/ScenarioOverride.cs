namespace Acme.Workforce.Orgdesign.Scenario.Template;

using Acme.Workforce.Kit.Text;

/// <summary>Where a scenario override is in its life cycle.</summary>
public enum ScenarioOverrideStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>A scenario override. Its code is unique within a tenant.</summary>
public sealed class ScenarioOverride(string code, string title, string unit, string skill, double ratio, double cost, double margin, string region, ScenarioOverrideStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string Unit { get; set; } = unit;
    public string Skill { get; set; } = skill;
    public double Ratio { get; set; } = ratio;
    public double Cost { get; set; } = cost;
    public double Margin { get; set; } = margin;
    public string Region { get; set; } = region;
    public ScenarioOverrideStatus Status { get; set; } = status;

    /// <summary>Starts a scenario override in status requested.</summary>
    public static ScenarioOverride NewScenarioOverride(string code, string title)
    {
        return new ScenarioOverride(code, title, "", "", 0.0, 0.0, 0.0, "", ScenarioOverrideStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case ScenarioOverrideStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(ScenarioOverrideStatus next)
    {
        switch (Status)
        {
            case ScenarioOverrideStatus.Requested:
                switch (next)
                {
                    case ScenarioOverrideStatus.Granted:
                    case ScenarioOverrideStatus.Expired:
                        return true;
                }
                return false;
            case ScenarioOverrideStatus.Granted:
                return next == ScenarioOverrideStatus.Revoked || next == ScenarioOverrideStatus.Expired;
            case ScenarioOverrideStatus.Revoked:
                switch (next)
                {
                    case ScenarioOverrideStatus.Expired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(ScenarioOverrideStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"scenario override {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} / {Title}";
    }

    public double ProjectedExposure()
    {
        return Math.Round(Ratio + Cost, 2);
    }

    public ScenarioOverride Copy()
    {
        return new ScenarioOverride(Code, Title, Unit, Skill, Ratio, Cost, Margin, Region, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Unit);
    }
}
