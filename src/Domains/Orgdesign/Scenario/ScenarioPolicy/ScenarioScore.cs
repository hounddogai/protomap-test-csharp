namespace Acme.Workforce.Orgdesign.Scenario.ScenarioPolicy;

using Acme.Workforce.Kit.Text;

/// <summary>Where a scenario score is in its life cycle.</summary>
public enum ScenarioScoreStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>The record of a scenario score and its current status.</summary>
public sealed class ScenarioScore(string code, string name, string jobCode, double rate, double amount, double budget, int updatedAt, string notes, string channel, string region, ScenarioScoreStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string JobCode { get; set; } = jobCode;
    public double Rate { get; set; } = rate;
    public double Amount { get; set; } = amount;
    public double Budget { get; set; } = budget;
    public int UpdatedAt { get; set; } = updatedAt;
    public string Notes { get; set; } = notes;
    public string Channel { get; set; } = channel;
    public string Region { get; set; } = region;
    public ScenarioScoreStatus Status { get; set; } = status;

    /// <summary>Starts a scenario score in status proposed.</summary>
    public static ScenarioScore NewScenarioScore(string code, string name)
    {
        return new ScenarioScore(code, name, "", 0.0, 0.0, 0.0, 0, "", "", "", ScenarioScoreStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case ScenarioScoreStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(ScenarioScoreStatus next)
    {
        switch (Status)
        {
            case ScenarioScoreStatus.Proposed:
                switch (next)
                {
                    case ScenarioScoreStatus.Approved:
                    case ScenarioScoreStatus.Withdrawn:
                        return true;
                }
                return false;
            case ScenarioScoreStatus.Approved:
                return next == ScenarioScoreStatus.Rejected || next == ScenarioScoreStatus.Withdrawn;
            case ScenarioScoreStatus.Rejected:
                switch (next)
                {
                    case ScenarioScoreStatus.Withdrawn:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(ScenarioScoreStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"scenario score {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        UpdatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code} / {Name}";
    }

    public double NetExposure()
    {
        return Math.Round(Rate - Amount, 2);
    }

    public ScenarioScore Copy()
    {
        return new ScenarioScore(
            Code,
            Name,
            JobCode,
            Rate,
            Amount,
            Budget,
            UpdatedAt,
            Notes,
            Channel,
            Region,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Channel);
    }
}
