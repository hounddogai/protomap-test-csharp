namespace Acme.Workforce.Payrollops.PayRun.Target;

using Acme.Workforce.Kit.Text;

/// <summary>Where a pay run baseline is in its life cycle.</summary>
public enum PayRunBaselineStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>The record of a pay run baseline and its current status.</summary>
public sealed class PayRunBaseline(string code, string title, bool archived, string jobCode, int graceDays, int expiresAt, PayRunBaselineStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public bool Archived { get; set; } = archived;
    public string JobCode { get; set; } = jobCode;
    public int GraceDays { get; set; } = graceDays;
    public int ExpiresAt { get; set; } = expiresAt;
    public PayRunBaselineStatus Status { get; set; } = status;

    /// <summary>Starts a pay run baseline in status created.</summary>
    public static PayRunBaseline NewPayRunBaseline(string code, string title)
    {
        return new PayRunBaseline(code, title, false, "", 0, 0, PayRunBaselineStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PayRunBaselineStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PayRunBaselineStatus next)
    {
        switch (Status)
        {
            case PayRunBaselineStatus.Created:
                switch (next)
                {
                    case PayRunBaselineStatus.Validated:
                    case PayRunBaselineStatus.Retired:
                        return true;
                }
                return false;
            case PayRunBaselineStatus.Validated:
                switch (next)
                {
                    case PayRunBaselineStatus.Retired:
                        return true;
                }
                return false;
            case PayRunBaselineStatus.Published:
                return next == PayRunBaselineStatus.Retired;
        }
        return false;
    }

    public void MoveTo(PayRunBaselineStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"pay run baseline {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        ExpiresAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code}: {Title}";
    }

    public double AdjustedValue()
    {
        return Math.Round((double)ExpiresAt * (double)GraceDays, 2);
    }

    public PayRunBaseline Copy()
    {
        return new PayRunBaseline(Code, Title, Archived, JobCode, GraceDays, ExpiresAt, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
