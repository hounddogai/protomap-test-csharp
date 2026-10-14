namespace Acme.Workforce.Compensation.PayBand.Draft;

using Acme.Workforce.Kit.Text;

/// <summary>Where a pay band draft is in its life cycle.</summary>
public enum PayBandDraftStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>The record of a pay band draft and its current status.</summary>
public sealed class PayBandDraft(string code, string name, double score, int expiresAt, double discount, string country, int graceDays, double weight, int windowDays, int version, double budget, PayBandDraftStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public double Score { get; set; } = score;
    public int ExpiresAt { get; set; } = expiresAt;
    public double Discount { get; set; } = discount;
    public string Country { get; set; } = country;
    public int GraceDays { get; set; } = graceDays;
    public double Weight { get; set; } = weight;
    public int WindowDays { get; set; } = windowDays;
    public int Version { get; set; } = version;
    public double Budget { get; set; } = budget;
    public PayBandDraftStatus Status { get; set; } = status;

    /// <summary>Starts a pay band draft in status proposed.</summary>
    public static PayBandDraft NewPayBandDraft(string code, string name)
    {
        return new PayBandDraft(code, name, 0.0, 0, 0.0, "", 0, 0.0, 0, 0, 0.0, PayBandDraftStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PayBandDraftStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PayBandDraftStatus next)
    {
        switch (Status)
        {
            case PayBandDraftStatus.Proposed:
                return next == PayBandDraftStatus.Approved || next == PayBandDraftStatus.Withdrawn;
            case PayBandDraftStatus.Approved:
                switch (next)
                {
                    case PayBandDraftStatus.Rejected:
                    case PayBandDraftStatus.Withdrawn:
                        return true;
                }
                return false;
            case PayBandDraftStatus.Rejected:
                return next == PayBandDraftStatus.Withdrawn;
        }
        return false;
    }

    public void MoveTo(PayBandDraftStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"pay band draft {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        ExpiresAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        Version += 1;
    }

    public string Label()
    {
        return $"{"pay band draft"} {Code}";
    }

    public double WeightedTotal()
    {
        return Math.Round(Score * (double)Version, 2);
    }

    public PayBandDraft Copy()
    {
        return new PayBandDraft(
            Code,
            Name,
            Score,
            ExpiresAt,
            Discount,
            Country,
            GraceDays,
            Weight,
            WindowDays,
            Version,
            Budget,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Country);
    }
}
