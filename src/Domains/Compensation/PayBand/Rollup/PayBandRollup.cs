namespace Acme.Workforce.Compensation.PayBand.Rollup;

using Acme.Workforce.Kit.Text;

/// <summary>Where a pay band rollup is in its life cycle.</summary>
public enum PayBandRollupStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>A pay band rollup as the pay band team keeps it.</summary>
public sealed class PayBandRollup(string code, string title, double budget, double fte, int expiresAt, bool pinned, double minimum, PayBandRollupStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public double Budget { get; set; } = budget;
    public double Fte { get; set; } = fte;
    public int ExpiresAt { get; set; } = expiresAt;
    public bool Pinned { get; set; } = pinned;
    public double Minimum { get; set; } = minimum;
    public PayBandRollupStatus Status { get; set; } = status;

    /// <summary>Starts a pay band rollup in status proposed.</summary>
    public static PayBandRollup NewPayBandRollup(string code, string title)
    {
        return new PayBandRollup(code, title, 0.0, 0.0, 0, false, 0.0, PayBandRollupStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PayBandRollupStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PayBandRollupStatus next)
    {
        switch (Status)
        {
            case PayBandRollupStatus.Proposed:
                return next == PayBandRollupStatus.Approved;
            case PayBandRollupStatus.Approved:
                return next == PayBandRollupStatus.Rejected || next == PayBandRollupStatus.Withdrawn;
            case PayBandRollupStatus.Rejected:
                return next == PayBandRollupStatus.Withdrawn;
        }
        return false;
    }

    public void MoveTo(PayBandRollupStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"pay band rollup {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        ExpiresAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{"pay band rollup"} {Code}";
    }

    public double AdjustedExposure()
    {
        return Math.Round((double)ExpiresAt - Minimum, 2);
    }

    public PayBandRollup Copy()
    {
        return new PayBandRollup(Code, Title, Budget, Fte, ExpiresAt, Pinned, Minimum, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
