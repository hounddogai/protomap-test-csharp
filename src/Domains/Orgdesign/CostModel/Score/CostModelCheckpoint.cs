namespace Acme.Workforce.Orgdesign.CostModel.Score;

using Acme.Workforce.Kit.Text;

/// <summary>Where a cost model checkpoint is in its life cycle.</summary>
public enum CostModelCheckpointStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>A cost model checkpoint as the cost model team keeps it.</summary>
public sealed class CostModelCheckpoint(string code, string name, string title, int version, string timezone, int durationMinutes, int retries, double score, CostModelCheckpointStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string Title { get; set; } = title;
    public int Version { get; set; } = version;
    public string Timezone { get; set; } = timezone;
    public int DurationMinutes { get; set; } = durationMinutes;
    public int Retries { get; set; } = retries;
    public double Score { get; set; } = score;
    public CostModelCheckpointStatus Status { get; set; } = status;

    /// <summary>Starts a cost model checkpoint in status requested.</summary>
    public static CostModelCheckpoint NewCostModelCheckpoint(string code, string name)
    {
        return new CostModelCheckpoint(code, name, "", 0, "", 0, 0, 0.0, CostModelCheckpointStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case CostModelCheckpointStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(CostModelCheckpointStatus next)
    {
        switch (Status)
        {
            case CostModelCheckpointStatus.Requested:
                switch (next)
                {
                    case CostModelCheckpointStatus.Granted:
                        return true;
                }
                return false;
            case CostModelCheckpointStatus.Granted:
                switch (next)
                {
                    case CostModelCheckpointStatus.Revoked:
                    case CostModelCheckpointStatus.Expired:
                        return true;
                }
                return false;
            case CostModelCheckpointStatus.Revoked:
                return next == CostModelCheckpointStatus.Expired;
        }
        return false;
    }

    public void MoveTo(CostModelCheckpointStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"cost model checkpoint {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        Version += 1;
    }

    public string Label()
    {
        return $"{"cost model checkpoint"} {Code}";
    }

    public double EffectiveTotal()
    {
        return Math.Round((double)Version * (double)Retries, 2);
    }

    public CostModelCheckpoint Copy()
    {
        return new CostModelCheckpoint(
            Code,
            Name,
            Title,
            Version,
            Timezone,
            DurationMinutes,
            Retries,
            Score,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
