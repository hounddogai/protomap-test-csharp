namespace Acme.Workforce.Orgdesign.Scenario.Audit;

using Acme.Workforce.Kit.Text;

/// <summary>Where a scenario bundle is in its life cycle.</summary>
public enum ScenarioBundleStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>A scenario bundle. Its code is unique within a tenant.</summary>
public sealed class ScenarioBundle(string code, string name, int windowDays, int capacity, string country, string notes, double score, string timezone, string label, string currency, double budget, ScenarioBundleStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public int WindowDays { get; set; } = windowDays;
    public int Capacity { get; set; } = capacity;
    public string Country { get; set; } = country;
    public string Notes { get; set; } = notes;
    public double Score { get; set; } = score;
    public string Timezone { get; set; } = timezone;
    public string Label { get; set; } = label;
    public string Currency { get; set; } = currency;
    public double Budget { get; set; } = budget;
    public ScenarioBundleStatus Status { get; set; } = status;

    /// <summary>Starts a scenario bundle in status open.</summary>
    public static ScenarioBundle NewScenarioBundle(string code, string name)
    {
        return new ScenarioBundle(code, name, 0, 0, "", "", 0.0, "", "", "", 0.0, ScenarioBundleStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case ScenarioBundleStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(ScenarioBundleStatus next)
    {
        switch (Status)
        {
            case ScenarioBundleStatus.Open:
                return next == ScenarioBundleStatus.Reopened;
            case ScenarioBundleStatus.InReview:
                switch (next)
                {
                    case ScenarioBundleStatus.Resolved:
                    case ScenarioBundleStatus.Archived:
                        return true;
                }
                return false;
            case ScenarioBundleStatus.Resolved:
                switch (next)
                {
                    case ScenarioBundleStatus.Reopened:
                    case ScenarioBundleStatus.Archived:
                        return true;
                }
                return false;
            case ScenarioBundleStatus.Reopened:
                switch (next)
                {
                    case ScenarioBundleStatus.Archived:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(ScenarioBundleStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"scenario bundle {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{"scenario bundle"} {Code}";
    }

    public double NetLoad()
    {
        return Math.Round((double)Capacity + (double)WindowDays, 2);
    }

    public ScenarioBundle Copy()
    {
        return new ScenarioBundle(
            Code,
            Name,
            WindowDays,
            Capacity,
            Country,
            Notes,
            Score,
            Timezone,
            Label,
            Currency,
            Budget,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Country);
    }
}
