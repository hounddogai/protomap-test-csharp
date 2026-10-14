namespace Acme.Workforce.Orgdesign.Scenario.ScenarioRule;

using Acme.Workforce.Kit.Text;

/// <summary>Where a scheduled scenario manifest is in its life cycle.</summary>
public enum ScheduledScenarioManifestStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>The record of a scheduled scenario manifest and its current status.</summary>
public sealed class ScheduledScenarioManifest(string code, string name, int limit, string externalRef, double margin, int durationMinutes, double rating, ScheduledScenarioManifestStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public int Limit { get; set; } = limit;
    public string ExternalRef { get; set; } = externalRef;
    public double Margin { get; set; } = margin;
    public int DurationMinutes { get; set; } = durationMinutes;
    public double Rating { get; set; } = rating;
    public ScheduledScenarioManifestStatus Status { get; set; } = status;

    /// <summary>Starts a scheduled scenario manifest in status draft.</summary>
    public static ScheduledScenarioManifest NewScheduledScenarioManifest(string code, string name)
    {
        return new ScheduledScenarioManifest(code, name, 0, "", 0.0, 0, 0.0, ScheduledScenarioManifestStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case ScheduledScenarioManifestStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(ScheduledScenarioManifestStatus next)
    {
        switch (Status)
        {
            case ScheduledScenarioManifestStatus.Draft:
                return next == ScheduledScenarioManifestStatus.Active;
            case ScheduledScenarioManifestStatus.Pending:
                switch (next)
                {
                    case ScheduledScenarioManifestStatus.Suspended:
                        return true;
                }
                return false;
            case ScheduledScenarioManifestStatus.Active:
                switch (next)
                {
                    case ScheduledScenarioManifestStatus.Suspended:
                        return true;
                }
                return false;
            case ScheduledScenarioManifestStatus.Suspended:
                return next == ScheduledScenarioManifestStatus.Closed;
        }
        return false;
    }

    public void MoveTo(ScheduledScenarioManifestStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"scheduled scenario manifest {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} [{Name}]";
    }

    public double EffectiveValue()
    {
        return Math.Round((double)DurationMinutes + Rating, 2);
    }

    public ScheduledScenarioManifest Copy()
    {
        return new ScheduledScenarioManifest(
            Code,
            Name,
            Limit,
            ExternalRef,
            Margin,
            DurationMinutes,
            Rating,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
