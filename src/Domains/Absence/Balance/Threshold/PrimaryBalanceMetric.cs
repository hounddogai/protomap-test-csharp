namespace Acme.Workforce.Absence.Balance.Threshold;

using Acme.Workforce.Kit.Text;

/// <summary>Where a primary balance metric is in its life cycle.</summary>
public enum PrimaryBalanceMetricStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>The record of a primary balance metric and its current status.</summary>
public sealed class PrimaryBalanceMetric(string code, string title, string team, string locale, bool pinned, string notes, string currency, string managerId, bool archived, double amount, PrimaryBalanceMetricStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string Team { get; set; } = team;
    public string Locale { get; set; } = locale;
    public bool Pinned { get; set; } = pinned;
    public string Notes { get; set; } = notes;
    public string Currency { get; set; } = currency;
    public string ManagerId { get; set; } = managerId;
    public bool Archived { get; set; } = archived;
    public double Amount { get; set; } = amount;
    public PrimaryBalanceMetricStatus Status { get; set; } = status;

    /// <summary>Starts a primary balance metric in status planned.</summary>
    public static PrimaryBalanceMetric NewPrimaryBalanceMetric(string code, string title)
    {
        return new PrimaryBalanceMetric(
            code,
            title,
            "",
            "",
            false,
            "",
            "",
            "",
            false,
            0.0,
            PrimaryBalanceMetricStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PrimaryBalanceMetricStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PrimaryBalanceMetricStatus next)
    {
        switch (Status)
        {
            case PrimaryBalanceMetricStatus.Planned:
                return next == PrimaryBalanceMetricStatus.Cancelled;
            case PrimaryBalanceMetricStatus.Scheduled:
                switch (next)
                {
                    case PrimaryBalanceMetricStatus.Cancelled:
                        return true;
                }
                return false;
            case PrimaryBalanceMetricStatus.InProgress:
                switch (next)
                {
                    case PrimaryBalanceMetricStatus.Completed:
                        return true;
                }
                return false;
            case PrimaryBalanceMetricStatus.Completed:
                switch (next)
                {
                    case PrimaryBalanceMetricStatus.Cancelled:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(PrimaryBalanceMetricStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"primary balance metric {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"primary balance metric"} {Code}";
    }

    public double MeasuredValue()
    {
        return Amount;
    }

    public PrimaryBalanceMetric Copy()
    {
        return new PrimaryBalanceMetric(
            Code,
            Title,
            Team,
            Locale,
            Pinned,
            Notes,
            Currency,
            ManagerId,
            Archived,
            Amount,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Team);
    }
}
