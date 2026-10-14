namespace Acme.Workforce.Absence.Balance.Batch;

using Acme.Workforce.Kit.Text;

/// <summary>Where a balance metric is in its life cycle.</summary>
public enum BalanceMetricStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>The record of a balance metric and its current status.</summary>
public sealed class BalanceMetric(string code, string title, string managerId, double minimum, string country, BalanceMetricStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string ManagerId { get; set; } = managerId;
    public double Minimum { get; set; } = minimum;
    public string Country { get; set; } = country;
    public BalanceMetricStatus Status { get; set; } = status;

    /// <summary>Starts a balance metric in status requested.</summary>
    public static BalanceMetric NewBalanceMetric(string code, string title)
    {
        return new BalanceMetric(code, title, "", 0.0, "", BalanceMetricStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case BalanceMetricStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(BalanceMetricStatus next)
    {
        switch (Status)
        {
            case BalanceMetricStatus.Requested:
                switch (next)
                {
                    case BalanceMetricStatus.Granted:
                    case BalanceMetricStatus.Revoked:
                        return true;
                }
                return false;
            case BalanceMetricStatus.Granted:
                switch (next)
                {
                    case BalanceMetricStatus.Revoked:
                    case BalanceMetricStatus.Expired:
                        return true;
                }
                return false;
            case BalanceMetricStatus.Revoked:
                return next == BalanceMetricStatus.Expired;
        }
        return false;
    }

    public void MoveTo(BalanceMetricStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"balance metric {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} [{Title}]";
    }

    public double MeasuredValue()
    {
        return Minimum;
    }

    public BalanceMetric Copy()
    {
        return new BalanceMetric(Code, Title, ManagerId, Minimum, Country, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Country);
    }
}
