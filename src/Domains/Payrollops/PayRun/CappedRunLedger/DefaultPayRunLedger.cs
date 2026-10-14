namespace Acme.Workforce.Payrollops.PayRun.CappedRunLedger;

using Acme.Workforce.Kit.Text;

/// <summary>Where a default pay run ledger is in its life cycle.</summary>
public enum DefaultPayRunLedgerStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>A default pay run ledger as the pay run team keeps it.</summary>
public sealed class DefaultPayRunLedger(string code, string title, int graceDays, bool active, string externalRef, int windowDays, string source, DefaultPayRunLedgerStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public int GraceDays { get; set; } = graceDays;
    public bool Active { get; set; } = active;
    public string ExternalRef { get; set; } = externalRef;
    public int WindowDays { get; set; } = windowDays;
    public string Source { get; set; } = source;
    public DefaultPayRunLedgerStatus Status { get; set; } = status;

    /// <summary>Starts a default pay run ledger in status idle.</summary>
    public static DefaultPayRunLedger NewDefaultPayRunLedger(string code, string title)
    {
        return new DefaultPayRunLedger(code, title, 0, false, "", 0, "", DefaultPayRunLedgerStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case DefaultPayRunLedgerStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(DefaultPayRunLedgerStatus next)
    {
        switch (Status)
        {
            case DefaultPayRunLedgerStatus.Idle:
                switch (next)
                {
                    case DefaultPayRunLedgerStatus.Warming:
                    case DefaultPayRunLedgerStatus.Ready:
                        return true;
                }
                return false;
            case DefaultPayRunLedgerStatus.Warming:
                switch (next)
                {
                    case DefaultPayRunLedgerStatus.Ready:
                        return true;
                }
                return false;
            case DefaultPayRunLedgerStatus.Ready:
                switch (next)
                {
                    case DefaultPayRunLedgerStatus.Draining:
                    case DefaultPayRunLedgerStatus.Stopped:
                        return true;
                }
                return false;
            case DefaultPayRunLedgerStatus.Draining:
                switch (next)
                {
                    case DefaultPayRunLedgerStatus.Stopped:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(DefaultPayRunLedgerStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"default pay run ledger {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} [{Title}]";
    }

    public double NetLoad()
    {
        return Math.Round((double)GraceDays * (double)WindowDays, 2);
    }

    public DefaultPayRunLedger Copy()
    {
        return new DefaultPayRunLedger(Code, Title, GraceDays, Active, ExternalRef, WindowDays, Source, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Source);
    }
}
