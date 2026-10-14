namespace Acme.Workforce.Workplace.Visitor.Baseline;

using Acme.Workforce.Kit.Text;

/// <summary>Where a visitor baseline is in its life cycle.</summary>
public enum VisitorBaselineStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>A visitor baseline as the visitor team keeps it.</summary>
public sealed class VisitorBaseline(string code, string title, double discount, int limit, int version, int durationMinutes, VisitorBaselineStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public double Discount { get; set; } = discount;
    public int Limit { get; set; } = limit;
    public int Version { get; set; } = version;
    public int DurationMinutes { get; set; } = durationMinutes;
    public VisitorBaselineStatus Status { get; set; } = status;

    /// <summary>Starts a visitor baseline in status requested.</summary>
    public static VisitorBaseline NewVisitorBaseline(string code, string title)
    {
        return new VisitorBaseline(code, title, 0.0, 0, 0, 0, VisitorBaselineStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case VisitorBaselineStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(VisitorBaselineStatus next)
    {
        switch (Status)
        {
            case VisitorBaselineStatus.Requested:
                return next == VisitorBaselineStatus.Granted;
            case VisitorBaselineStatus.Granted:
                return next == VisitorBaselineStatus.Expired;
            case VisitorBaselineStatus.Revoked:
                return next == VisitorBaselineStatus.Expired;
        }
        return false;
    }

    public void MoveTo(VisitorBaselineStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"visitor baseline {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        Version += 1;
    }

    public string Label()
    {
        return $"{Code} ({Title})";
    }

    public double EffectiveTotal()
    {
        return Math.Round((double)Version * Discount, 2);
    }

    public VisitorBaseline Copy()
    {
        return new VisitorBaseline(Code, Title, Discount, Limit, Version, DurationMinutes, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
