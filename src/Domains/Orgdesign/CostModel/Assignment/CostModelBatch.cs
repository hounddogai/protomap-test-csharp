namespace Acme.Workforce.Orgdesign.CostModel.Assignment;

using Acme.Workforce.Kit.Text;

/// <summary>Where a cost model batch is in its life cycle.</summary>
public enum CostModelBatchStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>One cost model batch, with what the planners need to act on it.</summary>
public sealed class CostModelBatch(string code, string name, int attempts, string team, int windowDays, int retries, int endsAt, string timezone, string costCenter, CostModelBatchStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public int Attempts { get; set; } = attempts;
    public string Team { get; set; } = team;
    public int WindowDays { get; set; } = windowDays;
    public int Retries { get; set; } = retries;
    public int EndsAt { get; set; } = endsAt;
    public string Timezone { get; set; } = timezone;
    public string CostCenter { get; set; } = costCenter;
    public CostModelBatchStatus Status { get; set; } = status;

    /// <summary>Starts a cost model batch in status new.</summary>
    public static CostModelBatch NewCostModelBatch(string code, string name)
    {
        return new CostModelBatch(code, name, 0, "", 0, 0, 0, "", "", CostModelBatchStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case CostModelBatchStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(CostModelBatchStatus next)
    {
        switch (Status)
        {
            case CostModelBatchStatus.New:
                switch (next)
                {
                    case CostModelBatchStatus.Running:
                    case CostModelBatchStatus.Failed:
                        return true;
                }
                return false;
            case CostModelBatchStatus.Queued:
                return next == CostModelBatchStatus.Running || next == CostModelBatchStatus.Failed;
            case CostModelBatchStatus.Running:
                return next == CostModelBatchStatus.Done || next == CostModelBatchStatus.Failed;
            case CostModelBatchStatus.Done:
                return next == CostModelBatchStatus.Failed;
        }
        return false;
    }

    public void MoveTo(CostModelBatchStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"cost model batch {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        EndsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code}: {Name}";
    }

    public double AdjustedTotal()
    {
        return Math.Round((double)Retries - (double)WindowDays, 2);
    }

    public CostModelBatch Copy()
    {
        return new CostModelBatch(
            Code,
            Name,
            Attempts,
            Team,
            WindowDays,
            Retries,
            EndsAt,
            Timezone,
            CostCenter,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Team);
    }
}
