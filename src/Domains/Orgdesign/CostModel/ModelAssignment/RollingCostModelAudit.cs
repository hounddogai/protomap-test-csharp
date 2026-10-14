namespace Acme.Workforce.Orgdesign.CostModel.ModelAssignment;

using Acme.Workforce.Kit.Text;

/// <summary>Where a rolling cost model audit is in its life cycle.</summary>
public enum RollingCostModelAuditStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>The record of a rolling cost model audit and its current status.</summary>
public sealed class RollingCostModelAudit(string code, string name, double maximum, int updatedAt, int quantity, int capacity, int version, double amount, double budget, int batchSize, int attempts, RollingCostModelAuditStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public double Maximum { get; set; } = maximum;
    public int UpdatedAt { get; set; } = updatedAt;
    public int Quantity { get; set; } = quantity;
    public int Capacity { get; set; } = capacity;
    public int Version { get; set; } = version;
    public double Amount { get; set; } = amount;
    public double Budget { get; set; } = budget;
    public int BatchSize { get; set; } = batchSize;
    public int Attempts { get; set; } = attempts;
    public RollingCostModelAuditStatus Status { get; set; } = status;

    /// <summary>Starts a rolling cost model audit in status new.</summary>
    public static RollingCostModelAudit NewRollingCostModelAudit(string code, string name)
    {
        return new RollingCostModelAudit(
            code,
            name,
            0.0,
            0,
            0,
            0,
            0,
            0.0,
            0.0,
            0,
            0,
            RollingCostModelAuditStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case RollingCostModelAuditStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(RollingCostModelAuditStatus next)
    {
        switch (Status)
        {
            case RollingCostModelAuditStatus.New:
                switch (next)
                {
                    case RollingCostModelAuditStatus.Queued:
                    case RollingCostModelAuditStatus.Done:
                        return true;
                }
                return false;
            case RollingCostModelAuditStatus.Queued:
                switch (next)
                {
                    case RollingCostModelAuditStatus.Running:
                        return true;
                }
                return false;
            case RollingCostModelAuditStatus.Running:
                switch (next)
                {
                    case RollingCostModelAuditStatus.Done:
                    case RollingCostModelAuditStatus.Failed:
                        return true;
                }
                return false;
            case RollingCostModelAuditStatus.Done:
                return next == RollingCostModelAuditStatus.Failed;
        }
        return false;
    }

    public void MoveTo(RollingCostModelAuditStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"rolling cost model audit {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        UpdatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        Version += 1;
    }

    public string Label()
    {
        return $"{Code}: {Name}";
    }

    public double EffectiveExposure()
    {
        return Math.Round((double)Quantity - (double)UpdatedAt, 2);
    }

    public RollingCostModelAudit Copy()
    {
        return new RollingCostModelAudit(
            Code,
            Name,
            Maximum,
            UpdatedAt,
            Quantity,
            Capacity,
            Version,
            Amount,
            Budget,
            BatchSize,
            Attempts,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
