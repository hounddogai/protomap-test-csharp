namespace Acme.Workforce.Orgdesign.Scenario.Cycle;

using Acme.Workforce.Kit.Text;

/// <summary>Where a scenario manifest is in its life cycle.</summary>
public enum ScenarioManifestStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>The record of a scenario manifest and its current status.</summary>
public sealed class ScenarioManifest(string code, string name, string locale, string notes, int expiresAt, string timezone, int tenureMonths, string description, ScenarioManifestStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string Locale { get; set; } = locale;
    public string Notes { get; set; } = notes;
    public int ExpiresAt { get; set; } = expiresAt;
    public string Timezone { get; set; } = timezone;
    public int TenureMonths { get; set; } = tenureMonths;
    public string Description { get; set; } = description;
    public ScenarioManifestStatus Status { get; set; } = status;

    /// <summary>Starts a scenario manifest in status proposed.</summary>
    public static ScenarioManifest NewScenarioManifest(string code, string name)
    {
        return new ScenarioManifest(code, name, "", "", 0, "", 0, "", ScenarioManifestStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case ScenarioManifestStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(ScenarioManifestStatus next)
    {
        switch (Status)
        {
            case ScenarioManifestStatus.Proposed:
                switch (next)
                {
                    case ScenarioManifestStatus.Approved:
                        return true;
                }
                return false;
            case ScenarioManifestStatus.Approved:
                return next == ScenarioManifestStatus.Rejected || next == ScenarioManifestStatus.Withdrawn;
            case ScenarioManifestStatus.Rejected:
                switch (next)
                {
                    case ScenarioManifestStatus.Withdrawn:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(ScenarioManifestStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"scenario manifest {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        ExpiresAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code} [{Name}]";
    }

    public double EffectiveLoad()
    {
        return Math.Round((double)ExpiresAt + (double)TenureMonths, 2);
    }

    public ScenarioManifest Copy()
    {
        return new ScenarioManifest(
            Code,
            Name,
            Locale,
            Notes,
            ExpiresAt,
            Timezone,
            TenureMonths,
            Description,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Locale);
    }
}
