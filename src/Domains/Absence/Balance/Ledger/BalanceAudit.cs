namespace Acme.Workforce.Absence.Balance.Ledger;

using Acme.Workforce.Kit.Text;

/// <summary>Where a balance audit is in its life cycle.</summary>
public enum BalanceAuditStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>A balance audit as the balance team keeps it.</summary>
public sealed class BalanceAudit(string code, string title, int graceDays, double weight, string notes, BalanceAuditStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public int GraceDays { get; set; } = graceDays;
    public double Weight { get; set; } = weight;
    public string Notes { get; set; } = notes;
    public BalanceAuditStatus Status { get; set; } = status;

    /// <summary>Starts a balance audit in status new.</summary>
    public static BalanceAudit NewBalanceAudit(string code, string title)
    {
        return new BalanceAudit(code, title, 0, 0.0, "", BalanceAuditStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case BalanceAuditStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(BalanceAuditStatus next)
    {
        switch (Status)
        {
            case BalanceAuditStatus.New:
                return next == BalanceAuditStatus.Queued || next == BalanceAuditStatus.Failed;
            case BalanceAuditStatus.Queued:
                switch (next)
                {
                    case BalanceAuditStatus.Done:
                        return true;
                }
                return false;
            case BalanceAuditStatus.Running:
                switch (next)
                {
                    case BalanceAuditStatus.Done:
                    case BalanceAuditStatus.Failed:
                        return true;
                }
                return false;
            case BalanceAuditStatus.Done:
                return next == BalanceAuditStatus.Failed;
        }
        return false;
    }

    public void MoveTo(BalanceAuditStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"balance audit {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code}: {Title}";
    }

    public double NetTotal()
    {
        return Math.Round(Weight + (double)GraceDays, 2);
    }

    public BalanceAudit Copy()
    {
        return new BalanceAudit(Code, Title, GraceDays, Weight, Notes, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
