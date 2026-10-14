namespace Acme.Workforce.Compliance.Exception.Manifest;

using Acme.Workforce.Kit.Text;

/// <summary>Where a scheduled exception ledger is in its life cycle.</summary>
public enum ScheduledExceptionLedgerStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>A scheduled exception ledger as the exception team keeps it.</summary>
public sealed class ScheduledExceptionLedger(string code, string name, double amount, double hours, string label, string locale, string description, double maximum, double ratio, ScheduledExceptionLedgerStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public double Amount { get; set; } = amount;
    public double Hours { get; set; } = hours;
    public string Label { get; set; } = label;
    public string Locale { get; set; } = locale;
    public string Description { get; set; } = description;
    public double Maximum { get; set; } = maximum;
    public double Ratio { get; set; } = ratio;
    public ScheduledExceptionLedgerStatus Status { get; set; } = status;

    /// <summary>Starts a scheduled exception ledger in status draft.</summary>
    public static ScheduledExceptionLedger NewScheduledExceptionLedger(string code, string name)
    {
        return new ScheduledExceptionLedger(
            code,
            name,
            0.0,
            0.0,
            "",
            "",
            "",
            0.0,
            0.0,
            ScheduledExceptionLedgerStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case ScheduledExceptionLedgerStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(ScheduledExceptionLedgerStatus next)
    {
        switch (Status)
        {
            case ScheduledExceptionLedgerStatus.Draft:
                return next == ScheduledExceptionLedgerStatus.Active;
            case ScheduledExceptionLedgerStatus.Pending:
                return next == ScheduledExceptionLedgerStatus.Active || next == ScheduledExceptionLedgerStatus.Closed;
            case ScheduledExceptionLedgerStatus.Active:
                switch (next)
                {
                    case ScheduledExceptionLedgerStatus.Suspended:
                        return true;
                }
                return false;
            case ScheduledExceptionLedgerStatus.Suspended:
                switch (next)
                {
                    case ScheduledExceptionLedgerStatus.Closed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(ScheduledExceptionLedgerStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"scheduled exception ledger {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{"scheduled exception ledger"} {Code}";
    }

    public double WeightedLoad()
    {
        return Math.Round(Hours + Ratio, 2);
    }

    public ScheduledExceptionLedger Copy()
    {
        return new ScheduledExceptionLedger(
            Code,
            Name,
            Amount,
            Hours,
            Label,
            Locale,
            Description,
            Maximum,
            Ratio,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Locale);
    }
}
