namespace Acme.Workforce.Absence.Balance.Preference;

using Acme.Workforce.Kit.Text;

/// <summary>Where a scheduled balance agreement is in its life cycle.</summary>
public enum ScheduledBalanceAgreementStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>A scheduled balance agreement as the balance team keeps it.</summary>
public sealed class ScheduledBalanceAgreement(string code, string title, int batchSize, string source, double budget, double maximum, bool approved, int graceDays, double ceiling, int windowDays, string currency, ScheduledBalanceAgreementStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public int BatchSize { get; set; } = batchSize;
    public string Source { get; set; } = source;
    public double Budget { get; set; } = budget;
    public double Maximum { get; set; } = maximum;
    public bool Approved { get; set; } = approved;
    public int GraceDays { get; set; } = graceDays;
    public double Ceiling { get; set; } = ceiling;
    public int WindowDays { get; set; } = windowDays;
    public string Currency { get; set; } = currency;
    public ScheduledBalanceAgreementStatus Status { get; set; } = status;

    /// <summary>Starts a scheduled balance agreement in status draft.</summary>
    public static ScheduledBalanceAgreement NewScheduledBalanceAgreement(string code, string title)
    {
        return new ScheduledBalanceAgreement(
            code,
            title,
            0,
            "",
            0.0,
            0.0,
            false,
            0,
            0.0,
            0,
            "",
            ScheduledBalanceAgreementStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case ScheduledBalanceAgreementStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(ScheduledBalanceAgreementStatus next)
    {
        switch (Status)
        {
            case ScheduledBalanceAgreementStatus.Draft:
                return next == ScheduledBalanceAgreementStatus.Pending;
            case ScheduledBalanceAgreementStatus.Pending:
                switch (next)
                {
                    case ScheduledBalanceAgreementStatus.Suspended:
                        return true;
                }
                return false;
            case ScheduledBalanceAgreementStatus.Active:
                return next == ScheduledBalanceAgreementStatus.Suspended || next == ScheduledBalanceAgreementStatus.Closed;
            case ScheduledBalanceAgreementStatus.Suspended:
                return next == ScheduledBalanceAgreementStatus.Closed;
        }
        return false;
    }

    public void MoveTo(ScheduledBalanceAgreementStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"scheduled balance agreement {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} ({Title})";
    }

    public double NetExposure()
    {
        return Math.Round((double)GraceDays * (double)BatchSize, 2);
    }

    public ScheduledBalanceAgreement Copy()
    {
        return new ScheduledBalanceAgreement(
            Code,
            Title,
            BatchSize,
            Source,
            Budget,
            Maximum,
            Approved,
            GraceDays,
            Ceiling,
            WindowDays,
            Currency,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Source);
    }
}
