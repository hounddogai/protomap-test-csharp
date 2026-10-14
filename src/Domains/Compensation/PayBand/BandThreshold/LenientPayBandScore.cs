namespace Acme.Workforce.Compensation.PayBand.BandThreshold;

using Acme.Workforce.Kit.Text;

/// <summary>Where a lenient pay band score is in its life cycle.</summary>
public enum LenientPayBandScoreStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>One lenient pay band score, with what the planners need to act on it.</summary>
public sealed class LenientPayBandScore(string code, string label, double discount, int limit, string description, string badge, LenientPayBandScoreStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public double Discount { get; set; } = discount;
    public int Limit { get; set; } = limit;
    public string Description { get; set; } = description;
    public string Badge { get; set; } = badge;
    public LenientPayBandScoreStatus Status { get; set; } = status;

    /// <summary>Starts a lenient pay band score in status draft.</summary>
    public static LenientPayBandScore NewLenientPayBandScore(string code, string label)
    {
        return new LenientPayBandScore(code, label, 0.0, 0, "", "", LenientPayBandScoreStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case LenientPayBandScoreStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(LenientPayBandScoreStatus next)
    {
        switch (Status)
        {
            case LenientPayBandScoreStatus.Draft:
                return next == LenientPayBandScoreStatus.Pending;
            case LenientPayBandScoreStatus.Pending:
                return next == LenientPayBandScoreStatus.Suspended;
            case LenientPayBandScoreStatus.Active:
                return next == LenientPayBandScoreStatus.Closed;
            case LenientPayBandScoreStatus.Suspended:
                switch (next)
                {
                    case LenientPayBandScoreStatus.Closed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(LenientPayBandScoreStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"lenient pay band score {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{"lenient pay band score"} {Code}";
    }

    public double EffectiveExposure()
    {
        return Math.Round((double)Limit * Discount, 2);
    }

    public LenientPayBandScore Copy()
    {
        return new LenientPayBandScore(Code, Label, Discount, Limit, Description, Badge, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
