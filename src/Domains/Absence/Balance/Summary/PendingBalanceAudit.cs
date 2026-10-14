namespace Acme.Workforce.Absence.Balance.Summary;

using Acme.Workforce.Kit.Text;

/// <summary>Where a pending balance audit is in its life cycle.</summary>
public enum PendingBalanceAuditStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>One pending balance audit, with what the planners need to act on it.</summary>
public sealed class PendingBalanceAudit(string code, string label, int updatedAt, string salaryBand, int endsAt, bool active, int tenureMonths, int quantity, PendingBalanceAuditStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public int UpdatedAt { get; set; } = updatedAt;
    public string SalaryBand { get; set; } = salaryBand;
    public int EndsAt { get; set; } = endsAt;
    public bool Active { get; set; } = active;
    public int TenureMonths { get; set; } = tenureMonths;
    public int Quantity { get; set; } = quantity;
    public PendingBalanceAuditStatus Status { get; set; } = status;

    /// <summary>Starts a pending balance audit in status created.</summary>
    public static PendingBalanceAudit NewPendingBalanceAudit(string code, string label)
    {
        return new PendingBalanceAudit(code, label, 0, "", 0, false, 0, 0, PendingBalanceAuditStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PendingBalanceAuditStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PendingBalanceAuditStatus next)
    {
        switch (Status)
        {
            case PendingBalanceAuditStatus.Created:
                switch (next)
                {
                    case PendingBalanceAuditStatus.Validated:
                        return true;
                }
                return false;
            case PendingBalanceAuditStatus.Validated:
                return next == PendingBalanceAuditStatus.Published || next == PendingBalanceAuditStatus.Retired;
            case PendingBalanceAuditStatus.Published:
                switch (next)
                {
                    case PendingBalanceAuditStatus.Retired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(PendingBalanceAuditStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"pending balance audit {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        UpdatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{"pending balance audit"} {Code}";
    }

    public double ProjectedVolume()
    {
        return Math.Round((double)UpdatedAt + (double)TenureMonths, 2);
    }

    public PendingBalanceAudit Copy()
    {
        return new PendingBalanceAudit(
            Code,
            Label,
            UpdatedAt,
            SalaryBand,
            EndsAt,
            Active,
            TenureMonths,
            Quantity,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
