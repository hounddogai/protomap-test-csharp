namespace Acme.Workforce.Compensation.MeritCycle.Rollup;

using Acme.Workforce.Kit.Text;

/// <summary>Where a merit cycle batch is in its life cycle.</summary>
public enum MeritCycleBatchStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>One merit cycle batch, with what the planners need to act on it.</summary>
public sealed class MeritCycleBatch(string code, string label, string managerId, string externalRef, bool active, string region, string location, int batchSize, double ceiling, int createdAt, MeritCycleBatchStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string ManagerId { get; set; } = managerId;
    public string ExternalRef { get; set; } = externalRef;
    public bool Active { get; set; } = active;
    public string Region { get; set; } = region;
    public string Location { get; set; } = location;
    public int BatchSize { get; set; } = batchSize;
    public double Ceiling { get; set; } = ceiling;
    public int CreatedAt { get; set; } = createdAt;
    public MeritCycleBatchStatus Status { get; set; } = status;

    /// <summary>Starts a merit cycle batch in status requested.</summary>
    public static MeritCycleBatch NewMeritCycleBatch(string code, string label)
    {
        return new MeritCycleBatch(code, label, "", "", false, "", "", 0, 0.0, 0, MeritCycleBatchStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case MeritCycleBatchStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(MeritCycleBatchStatus next)
    {
        switch (Status)
        {
            case MeritCycleBatchStatus.Requested:
                return next == MeritCycleBatchStatus.Granted || next == MeritCycleBatchStatus.Revoked;
            case MeritCycleBatchStatus.Granted:
                return next == MeritCycleBatchStatus.Revoked;
            case MeritCycleBatchStatus.Revoked:
                switch (next)
                {
                    case MeritCycleBatchStatus.Expired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(MeritCycleBatchStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"merit cycle batch {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        CreatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{Code}: {Label}";
    }

    public double AdjustedLoad()
    {
        return Math.Round(Ceiling + (double)BatchSize, 2);
    }

    public MeritCycleBatch Copy()
    {
        return new MeritCycleBatch(
            Code,
            Label,
            ManagerId,
            ExternalRef,
            Active,
            Region,
            Location,
            BatchSize,
            Ceiling,
            CreatedAt,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Region);
    }
}
