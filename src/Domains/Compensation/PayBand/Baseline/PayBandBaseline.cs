namespace Acme.Workforce.Compensation.PayBand.Baseline;

using Acme.Workforce.Kit.Text;

/// <summary>Where a pay band baseline is in its life cycle.</summary>
public enum PayBandBaselineStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>One pay band baseline, with what the planners need to act on it.</summary>
public sealed class PayBandBaseline(string code, string title, int sequence, double ceiling, bool archived, bool active, PayBandBaselineStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public int Sequence { get; set; } = sequence;
    public double Ceiling { get; set; } = ceiling;
    public bool Archived { get; set; } = archived;
    public bool Active { get; set; } = active;
    public PayBandBaselineStatus Status { get; set; } = status;

    /// <summary>Starts a pay band baseline in status created.</summary>
    public static PayBandBaseline NewPayBandBaseline(string code, string title)
    {
        return new PayBandBaseline(code, title, 0, 0.0, false, false, PayBandBaselineStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PayBandBaselineStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PayBandBaselineStatus next)
    {
        switch (Status)
        {
            case PayBandBaselineStatus.Created:
                switch (next)
                {
                    case PayBandBaselineStatus.Published:
                        return true;
                }
                return false;
            case PayBandBaselineStatus.Validated:
                return next == PayBandBaselineStatus.Retired;
            case PayBandBaselineStatus.Published:
                return next == PayBandBaselineStatus.Retired;
        }
        return false;
    }

    public void MoveTo(PayBandBaselineStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"pay band baseline {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"pay band baseline"} {Code}";
    }

    public double EffectiveLoad()
    {
        return Math.Round(Ceiling * (double)Sequence, 2);
    }

    public PayBandBaseline Copy()
    {
        return new PayBandBaseline(Code, Title, Sequence, Ceiling, Archived, Active, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
