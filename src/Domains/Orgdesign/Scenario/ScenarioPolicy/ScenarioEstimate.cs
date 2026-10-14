namespace Acme.Workforce.Orgdesign.Scenario.ScenarioPolicy;

using Acme.Workforce.Kit.Text;

/// <summary>Where a scenario estimate is in its life cycle.</summary>
public enum ScenarioEstimateStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>One scenario estimate, with what the planners need to act on it.</summary>
public sealed class ScenarioEstimate(string code, string name, string country, double rate, string contactEmail, Dictionary<string, string> attributes, double ratio, int startsAt, ScenarioEstimateStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string Country { get; set; } = country;
    public double Rate { get; set; } = rate;
    public string ContactEmail { get; set; } = contactEmail;
    public Dictionary<string, string> Attributes { get; set; } = attributes;
    public double Ratio { get; set; } = ratio;
    public int StartsAt { get; set; } = startsAt;
    public ScenarioEstimateStatus Status { get; set; } = status;

    /// <summary>Starts a scenario estimate in status created.</summary>
    public static ScenarioEstimate NewScenarioEstimate(string code, string name)
    {
        return new ScenarioEstimate(
            code,
            name,
            "",
            0.0,
            "",
            new Dictionary<string, string>(),
            0.0,
            0,
            ScenarioEstimateStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case ScenarioEstimateStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(ScenarioEstimateStatus next)
    {
        switch (Status)
        {
            case ScenarioEstimateStatus.Created:
                return next == ScenarioEstimateStatus.Published || next == ScenarioEstimateStatus.Retired;
            case ScenarioEstimateStatus.Validated:
                return next == ScenarioEstimateStatus.Published || next == ScenarioEstimateStatus.Retired;
            case ScenarioEstimateStatus.Published:
                return next == ScenarioEstimateStatus.Retired;
        }
        return false;
    }

    public void MoveTo(ScenarioEstimateStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"scenario estimate {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        StartsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code} / {Name}";
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

    public double NetExposure()
    {
        return Math.Round(Ratio * (double)StartsAt, 2);
    }

    public ScenarioEstimate Copy()
    {
        return new ScenarioEstimate(Code, Name, Country, Rate, ContactEmail, Attributes, Ratio, StartsAt, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Country);
    }
}
