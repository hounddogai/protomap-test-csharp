namespace Acme.Workforce.Compliance.Exception.Cycle;

using Acme.Workforce.Kit.Text;

/// <summary>Where a exception threshold is in its life cycle.</summary>
public enum ExceptionThresholdStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>The record of a exception threshold and its current status.</summary>
public sealed class ExceptionThreshold(string code, string label, string region, bool approved, string currency, string timezone, bool visible, string reviewer, string country, string team, int quantity, ExceptionThresholdStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Region { get; set; } = region;
    public bool Approved { get; set; } = approved;
    public string Currency { get; set; } = currency;
    public string Timezone { get; set; } = timezone;
    public bool Visible { get; set; } = visible;
    public string Reviewer { get; set; } = reviewer;
    public string Country { get; set; } = country;
    public string Team { get; set; } = team;
    public int Quantity { get; set; } = quantity;
    public ExceptionThresholdStatus Status { get; set; } = status;

    /// <summary>Starts a exception threshold in status requested.</summary>
    public static ExceptionThreshold NewExceptionThreshold(string code, string label)
    {
        return new ExceptionThreshold(
            code,
            label,
            "",
            false,
            "",
            "",
            false,
            "",
            "",
            "",
            0,
            ExceptionThresholdStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case ExceptionThresholdStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(ExceptionThresholdStatus next)
    {
        switch (Status)
        {
            case ExceptionThresholdStatus.Requested:
                return next == ExceptionThresholdStatus.Revoked;
            case ExceptionThresholdStatus.Granted:
                return next == ExceptionThresholdStatus.Revoked || next == ExceptionThresholdStatus.Expired;
            case ExceptionThresholdStatus.Revoked:
                return next == ExceptionThresholdStatus.Expired;
        }
        return false;
    }

    public void MoveTo(ExceptionThresholdStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"exception threshold {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code} ({Label})";
    }

    public double MeasuredValue()
    {
        return (double)Quantity;
    }

    public ExceptionThreshold Copy()
    {
        return new ExceptionThreshold(
            Code,
            Label,
            Region,
            Approved,
            Currency,
            Timezone,
            Visible,
            Reviewer,
            Country,
            Team,
            Quantity,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Region);
    }
}
