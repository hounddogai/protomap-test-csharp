namespace Acme.Workforce.Compensation.PayBand.Reservation;

using Acme.Workforce.Kit.Text;

/// <summary>Where a legacy pay band window is in its life cycle.</summary>
public enum LegacyPayBandWindowStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>One legacy pay band window, with what the planners need to act on it.</summary>
public sealed class LegacyPayBandWindow(string code, string label, string phone, double ceiling, int endsAt, string region, LegacyPayBandWindowStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Phone { get; set; } = phone;
    public double Ceiling { get; set; } = ceiling;
    public int EndsAt { get; set; } = endsAt;
    public string Region { get; set; } = region;
    public LegacyPayBandWindowStatus Status { get; set; } = status;

    /// <summary>Starts a legacy pay band window in status open.</summary>
    public static LegacyPayBandWindow NewLegacyPayBandWindow(string code, string label)
    {
        return new LegacyPayBandWindow(code, label, "", 0.0, 0, "", LegacyPayBandWindowStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case LegacyPayBandWindowStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(LegacyPayBandWindowStatus next)
    {
        switch (Status)
        {
            case LegacyPayBandWindowStatus.Open:
                return next == LegacyPayBandWindowStatus.Resolved;
            case LegacyPayBandWindowStatus.InReview:
                switch (next)
                {
                    case LegacyPayBandWindowStatus.Archived:
                        return true;
                }
                return false;
            case LegacyPayBandWindowStatus.Resolved:
                switch (next)
                {
                    case LegacyPayBandWindowStatus.Reopened:
                    case LegacyPayBandWindowStatus.Archived:
                        return true;
                }
                return false;
            case LegacyPayBandWindowStatus.Reopened:
                switch (next)
                {
                    case LegacyPayBandWindowStatus.Archived:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(LegacyPayBandWindowStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"legacy pay band window {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        EndsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{Code} / {Label}";
    }

    public double ProjectedValue()
    {
        return Math.Round((double)EndsAt - Ceiling, 2);
    }

    public LegacyPayBandWindow Copy()
    {
        return new LegacyPayBandWindow(Code, Label, Phone, Ceiling, EndsAt, Region, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Region);
    }
}
