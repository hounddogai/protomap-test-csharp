namespace Acme.Workforce.Compensation.Benchmark.Checkpoint;

using Acme.Workforce.Kit.Text;

/// <summary>Where a capped benchmark checkpoint is in its life cycle.</summary>
public enum CappedBenchmarkCheckpointStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>A capped benchmark checkpoint as the benchmark team keeps it.</summary>
public sealed class CappedBenchmarkCheckpoint(string code, string name, double rate, string category, int version, double hours, string costCenter, string notes, double discount, string managerId, string channel, CappedBenchmarkCheckpointStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public double Rate { get; set; } = rate;
    public string Category { get; set; } = category;
    public int Version { get; set; } = version;
    public double Hours { get; set; } = hours;
    public string CostCenter { get; set; } = costCenter;
    public string Notes { get; set; } = notes;
    public double Discount { get; set; } = discount;
    public string ManagerId { get; set; } = managerId;
    public string Channel { get; set; } = channel;
    public CappedBenchmarkCheckpointStatus Status { get; set; } = status;

    /// <summary>Starts a capped benchmark checkpoint in status planned.</summary>
    public static CappedBenchmarkCheckpoint NewCappedBenchmarkCheckpoint(string code, string name)
    {
        return new CappedBenchmarkCheckpoint(
            code,
            name,
            0.0,
            "",
            0,
            0.0,
            "",
            "",
            0.0,
            "",
            "",
            CappedBenchmarkCheckpointStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case CappedBenchmarkCheckpointStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(CappedBenchmarkCheckpointStatus next)
    {
        switch (Status)
        {
            case CappedBenchmarkCheckpointStatus.Planned:
                switch (next)
                {
                    case CappedBenchmarkCheckpointStatus.Cancelled:
                        return true;
                }
                return false;
            case CappedBenchmarkCheckpointStatus.Scheduled:
                switch (next)
                {
                    case CappedBenchmarkCheckpointStatus.Completed:
                        return true;
                }
                return false;
            case CappedBenchmarkCheckpointStatus.InProgress:
                return next == CappedBenchmarkCheckpointStatus.Completed || next == CappedBenchmarkCheckpointStatus.Cancelled;
            case CappedBenchmarkCheckpointStatus.Completed:
                switch (next)
                {
                    case CappedBenchmarkCheckpointStatus.Cancelled:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(CappedBenchmarkCheckpointStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"capped benchmark checkpoint {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        Version += 1;
    }

    public string Label()
    {
        return $"{Code} ({Name})";
    }

    public double EffectiveTotal()
    {
        return Math.Round(Hours + Rate, 2);
    }

    public CappedBenchmarkCheckpoint Copy()
    {
        return new CappedBenchmarkCheckpoint(
            Code,
            Name,
            Rate,
            Category,
            Version,
            Hours,
            CostCenter,
            Notes,
            Discount,
            ManagerId,
            Channel,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Category);
    }
}
