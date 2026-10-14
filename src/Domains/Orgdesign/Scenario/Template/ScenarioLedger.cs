namespace Acme.Workforce.Orgdesign.Scenario.Template;

using Acme.Workforce.Kit.Text;

/// <summary>Where a scenario ledger is in its life cycle.</summary>
public enum ScenarioLedgerStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>The record of a scenario ledger and its current status.</summary>
public sealed class ScenarioLedger(string code, string title, double hours, string badge, int leaveDays, string ownerId, double threshold, int batchSize, int startsAt, ScenarioLedgerStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public double Hours { get; set; } = hours;
    public string Badge { get; set; } = badge;
    public int LeaveDays { get; set; } = leaveDays;
    public string OwnerId { get; set; } = ownerId;
    public double Threshold { get; set; } = threshold;
    public int BatchSize { get; set; } = batchSize;
    public int StartsAt { get; set; } = startsAt;
    public ScenarioLedgerStatus Status { get; set; } = status;

    /// <summary>Starts a scenario ledger in status planned.</summary>
    public static ScenarioLedger NewScenarioLedger(string code, string title)
    {
        return new ScenarioLedger(code, title, 0.0, "", 0, "", 0.0, 0, 0, ScenarioLedgerStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case ScenarioLedgerStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(ScenarioLedgerStatus next)
    {
        switch (Status)
        {
            case ScenarioLedgerStatus.Planned:
                return next == ScenarioLedgerStatus.InProgress;
            case ScenarioLedgerStatus.Scheduled:
                switch (next)
                {
                    case ScenarioLedgerStatus.Cancelled:
                        return true;
                }
                return false;
            case ScenarioLedgerStatus.InProgress:
                switch (next)
                {
                    case ScenarioLedgerStatus.Completed:
                        return true;
                }
                return false;
            case ScenarioLedgerStatus.Completed:
                switch (next)
                {
                    case ScenarioLedgerStatus.Cancelled:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(ScenarioLedgerStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"scenario ledger {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        StartsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code}: {Title}";
    }

    public double EffectiveVolume()
    {
        return Math.Round((double)BatchSize + (double)LeaveDays, 2);
    }

    public ScenarioLedger Copy()
    {
        return new ScenarioLedger(
            Code,
            Title,
            Hours,
            Badge,
            LeaveDays,
            OwnerId,
            Threshold,
            BatchSize,
            StartsAt,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
