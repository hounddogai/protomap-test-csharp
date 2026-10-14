namespace Acme.Workforce.Orgdesign.Scenario.ScenarioTemplate;

using Acme.Workforce.Kit.Text;

/// <summary>Where a scenario exception is in its life cycle.</summary>
public enum ScenarioExceptionStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>One scenario exception, with what the planners need to act on it.</summary>
public sealed class ScenarioException(string code, string label, bool active, string phone, string managerId, string checksum, bool archived, int capacity, int endsAt, int batchSize, ScenarioExceptionStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public bool Active { get; set; } = active;
    public string Phone { get; set; } = phone;
    public string ManagerId { get; set; } = managerId;
    public string Checksum { get; set; } = checksum;
    public bool Archived { get; set; } = archived;
    public int Capacity { get; set; } = capacity;
    public int EndsAt { get; set; } = endsAt;
    public int BatchSize { get; set; } = batchSize;
    public ScenarioExceptionStatus Status { get; set; } = status;

    /// <summary>Starts a scenario exception in status requested.</summary>
    public static ScenarioException NewScenarioException(string code, string label)
    {
        return new ScenarioException(
            code,
            label,
            false,
            "",
            "",
            "",
            false,
            0,
            0,
            0,
            ScenarioExceptionStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case ScenarioExceptionStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(ScenarioExceptionStatus next)
    {
        switch (Status)
        {
            case ScenarioExceptionStatus.Requested:
                return next == ScenarioExceptionStatus.Revoked || next == ScenarioExceptionStatus.Expired;
            case ScenarioExceptionStatus.Granted:
                switch (next)
                {
                    case ScenarioExceptionStatus.Expired:
                        return true;
                }
                return false;
            case ScenarioExceptionStatus.Revoked:
                switch (next)
                {
                    case ScenarioExceptionStatus.Expired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(ScenarioExceptionStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"scenario exception {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        EndsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{"scenario exception"} {Code}";
    }

    public double EffectiveExposure()
    {
        return Math.Round((double)Capacity + (double)BatchSize, 2);
    }

    public ScenarioException Copy()
    {
        return new ScenarioException(
            Code,
            Label,
            Active,
            Phone,
            ManagerId,
            Checksum,
            Archived,
            Capacity,
            EndsAt,
            BatchSize,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
