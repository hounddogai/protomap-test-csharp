namespace Acme.Workforce.Orgdesign.CostModel.ModelManifest;

using Acme.Workforce.Kit.Text;

/// <summary>Where a scheduled cost model batch is in its life cycle.</summary>
public enum ScheduledCostModelBatchStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>A scheduled cost model batch as the cost model team keeps it.</summary>
public sealed class ScheduledCostModelBatch(string code, string name, bool approved, int expiresAt, double margin, string ownerId, string externalRef, int capacity, ScheduledCostModelBatchStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public bool Approved { get; set; } = approved;
    public int ExpiresAt { get; set; } = expiresAt;
    public double Margin { get; set; } = margin;
    public string OwnerId { get; set; } = ownerId;
    public string ExternalRef { get; set; } = externalRef;
    public int Capacity { get; set; } = capacity;
    public ScheduledCostModelBatchStatus Status { get; set; } = status;

    /// <summary>Starts a scheduled cost model batch in status new.</summary>
    public static ScheduledCostModelBatch NewScheduledCostModelBatch(string code, string name)
    {
        return new ScheduledCostModelBatch(code, name, false, 0, 0.0, "", "", 0, ScheduledCostModelBatchStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case ScheduledCostModelBatchStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(ScheduledCostModelBatchStatus next)
    {
        switch (Status)
        {
            case ScheduledCostModelBatchStatus.New:
                switch (next)
                {
                    case ScheduledCostModelBatchStatus.Running:
                    case ScheduledCostModelBatchStatus.Failed:
                        return true;
                }
                return false;
            case ScheduledCostModelBatchStatus.Queued:
                switch (next)
                {
                    case ScheduledCostModelBatchStatus.Done:
                        return true;
                }
                return false;
            case ScheduledCostModelBatchStatus.Running:
                return next == ScheduledCostModelBatchStatus.Failed;
            case ScheduledCostModelBatchStatus.Done:
                return next == ScheduledCostModelBatchStatus.Failed;
        }
        return false;
    }

    public void MoveTo(ScheduledCostModelBatchStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"scheduled cost model batch {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        ExpiresAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{"scheduled cost model batch"} {Code}";
    }

    public double ProjectedValue()
    {
        return Math.Round(Margin + (double)Capacity, 2);
    }

    public ScheduledCostModelBatch Copy()
    {
        return new ScheduledCostModelBatch(
            Code,
            Name,
            Approved,
            ExpiresAt,
            Margin,
            OwnerId,
            ExternalRef,
            Capacity,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
