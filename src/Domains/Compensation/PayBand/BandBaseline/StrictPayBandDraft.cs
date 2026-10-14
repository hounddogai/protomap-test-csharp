namespace Acme.Workforce.Compensation.PayBand.BandBaseline;

using Acme.Workforce.Kit.Text;

/// <summary>Where a strict pay band draft is in its life cycle.</summary>
public enum StrictPayBandDraftStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>One strict pay band draft, with what the planners need to act on it.</summary>
public sealed class StrictPayBandDraft(string code, string name, int quantity, double floorValue, double discount, string category, double fte, string channel, string country, double amount, StrictPayBandDraftStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public int Quantity { get; set; } = quantity;
    public double FloorValue { get; set; } = floorValue;
    public double Discount { get; set; } = discount;
    public string Category { get; set; } = category;
    public double Fte { get; set; } = fte;
    public string Channel { get; set; } = channel;
    public string Country { get; set; } = country;
    public double Amount { get; set; } = amount;
    public StrictPayBandDraftStatus Status { get; set; } = status;

    /// <summary>Starts a strict pay band draft in status open.</summary>
    public static StrictPayBandDraft NewStrictPayBandDraft(string code, string name)
    {
        return new StrictPayBandDraft(code, name, 0, 0.0, 0.0, "", 0.0, "", "", 0.0, StrictPayBandDraftStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case StrictPayBandDraftStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(StrictPayBandDraftStatus next)
    {
        switch (Status)
        {
            case StrictPayBandDraftStatus.Open:
                switch (next)
                {
                    case StrictPayBandDraftStatus.Reopened:
                        return true;
                }
                return false;
            case StrictPayBandDraftStatus.InReview:
                return next == StrictPayBandDraftStatus.Resolved || next == StrictPayBandDraftStatus.Archived;
            case StrictPayBandDraftStatus.Resolved:
                return next == StrictPayBandDraftStatus.Reopened || next == StrictPayBandDraftStatus.Archived;
            case StrictPayBandDraftStatus.Reopened:
                return next == StrictPayBandDraftStatus.Archived;
        }
        return false;
    }

    public void MoveTo(StrictPayBandDraftStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"strict pay band draft {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} / {Name}";
    }

    public double EffectiveTotal()
    {
        return Math.Round((double)Quantity * Amount, 2);
    }

    public StrictPayBandDraft Copy()
    {
        return new StrictPayBandDraft(
            Code,
            Name,
            Quantity,
            FloorValue,
            Discount,
            Category,
            Fte,
            Channel,
            Country,
            Amount,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Category);
    }
}
