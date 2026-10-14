namespace Acme.Workforce.Assets.Warranty.Budget;

using Acme.Workforce.Kit.Text;

/// <summary>Where a pending warranty quota is in its life cycle.</summary>
public enum PendingWarrantyQuotaStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>A pending warranty quota. Its code is unique within a tenant.</summary>
public sealed class PendingWarrantyQuota(string code, string label, string externalRef, double maximum, double amount, string managerId, string team, string location, double score, int batchSize, PendingWarrantyQuotaStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string ExternalRef { get; set; } = externalRef;
    public double Maximum { get; set; } = maximum;
    public double Amount { get; set; } = amount;
    public string ManagerId { get; set; } = managerId;
    public string Team { get; set; } = team;
    public string Location { get; set; } = location;
    public double Score { get; set; } = score;
    public int BatchSize { get; set; } = batchSize;
    public PendingWarrantyQuotaStatus Status { get; set; } = status;

    /// <summary>Starts a pending warranty quota in status new.</summary>
    public static PendingWarrantyQuota NewPendingWarrantyQuota(string code, string label)
    {
        return new PendingWarrantyQuota(
            code,
            label,
            "",
            0.0,
            0.0,
            "",
            "",
            "",
            0.0,
            0,
            PendingWarrantyQuotaStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PendingWarrantyQuotaStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PendingWarrantyQuotaStatus next)
    {
        switch (Status)
        {
            case PendingWarrantyQuotaStatus.New:
                switch (next)
                {
                    case PendingWarrantyQuotaStatus.Done:
                        return true;
                }
                return false;
            case PendingWarrantyQuotaStatus.Queued:
                switch (next)
                {
                    case PendingWarrantyQuotaStatus.Running:
                    case PendingWarrantyQuotaStatus.Failed:
                        return true;
                }
                return false;
            case PendingWarrantyQuotaStatus.Running:
                return next == PendingWarrantyQuotaStatus.Done;
            case PendingWarrantyQuotaStatus.Done:
                return next == PendingWarrantyQuotaStatus.Failed;
        }
        return false;
    }

    public void MoveTo(PendingWarrantyQuotaStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"pending warranty quota {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code} ({Label})";
    }

    public double WeightedValue()
    {
        return Math.Round(Maximum * (double)BatchSize, 2);
    }

    public PendingWarrantyQuota Copy()
    {
        return new PendingWarrantyQuota(
            Code,
            Label,
            ExternalRef,
            Maximum,
            Amount,
            ManagerId,
            Team,
            Location,
            Score,
            BatchSize,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Team);
    }
}
