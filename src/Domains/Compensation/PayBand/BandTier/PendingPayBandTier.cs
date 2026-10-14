namespace Acme.Workforce.Compensation.PayBand.BandTier;

using Acme.Workforce.Kit.Text;

/// <summary>Where a pending pay band tier is in its life cycle.</summary>
public enum PendingPayBandTierStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>A pending pay band tier as the pay band team keeps it.</summary>
public sealed class PendingPayBandTier(string code, string label, string locale, int graceDays, string managerId, string channel, string segment, double ceiling, double score, PendingPayBandTierStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Locale { get; set; } = locale;
    public int GraceDays { get; set; } = graceDays;
    public string ManagerId { get; set; } = managerId;
    public string Channel { get; set; } = channel;
    public string Segment { get; set; } = segment;
    public double Ceiling { get; set; } = ceiling;
    public double Score { get; set; } = score;
    public PendingPayBandTierStatus Status { get; set; } = status;

    /// <summary>Starts a pending pay band tier in status created.</summary>
    public static PendingPayBandTier NewPendingPayBandTier(string code, string label)
    {
        return new PendingPayBandTier(code, label, "", 0, "", "", "", 0.0, 0.0, PendingPayBandTierStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PendingPayBandTierStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PendingPayBandTierStatus next)
    {
        switch (Status)
        {
            case PendingPayBandTierStatus.Created:
                return next == PendingPayBandTierStatus.Validated;
            case PendingPayBandTierStatus.Validated:
                switch (next)
                {
                    case PendingPayBandTierStatus.Retired:
                        return true;
                }
                return false;
            case PendingPayBandTierStatus.Published:
                switch (next)
                {
                    case PendingPayBandTierStatus.Retired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(PendingPayBandTierStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"pending pay band tier {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code} / {Label}";
    }

    public double EffectiveValue()
    {
        return Math.Round((double)GraceDays - Score, 2);
    }

    public PendingPayBandTier Copy()
    {
        return new PendingPayBandTier(
            Code,
            Label,
            Locale,
            GraceDays,
            ManagerId,
            Channel,
            Segment,
            Ceiling,
            Score,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Locale);
    }
}
