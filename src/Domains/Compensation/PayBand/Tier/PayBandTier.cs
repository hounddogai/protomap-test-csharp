namespace Acme.Workforce.Compensation.PayBand.Tier;

using Acme.Workforce.Kit.Text;

/// <summary>Where a pay band tier is in its life cycle.</summary>
public enum PayBandTierStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>A pay band tier. Its code is unique within a tenant.</summary>
public sealed class PayBandTier(string code, string title, int revision, double weight, bool visible, PayBandTierStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public int Revision { get; set; } = revision;
    public double Weight { get; set; } = weight;
    public bool Visible { get; set; } = visible;
    public PayBandTierStatus Status { get; set; } = status;

    /// <summary>Starts a pay band tier in status open.</summary>
    public static PayBandTier NewPayBandTier(string code, string title)
    {
        return new PayBandTier(code, title, 0, 0.0, false, PayBandTierStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PayBandTierStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PayBandTierStatus next)
    {
        switch (Status)
        {
            case PayBandTierStatus.Open:
                return next == PayBandTierStatus.Resolved || next == PayBandTierStatus.Archived;
            case PayBandTierStatus.InReview:
                switch (next)
                {
                    case PayBandTierStatus.Archived:
                        return true;
                }
                return false;
            case PayBandTierStatus.Resolved:
                switch (next)
                {
                    case PayBandTierStatus.Reopened:
                    case PayBandTierStatus.Archived:
                        return true;
                }
                return false;
            case PayBandTierStatus.Reopened:
                switch (next)
                {
                    case PayBandTierStatus.Archived:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(PayBandTierStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"pay band tier {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        Revision += 1;
    }

    public string Label()
    {
        return $"{Code}: {Title}";
    }

    public double ProjectedLoad()
    {
        return Math.Round((double)Revision - Weight, 2);
    }

    public PayBandTier Copy()
    {
        return new PayBandTier(Code, Title, Revision, Weight, Visible, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
