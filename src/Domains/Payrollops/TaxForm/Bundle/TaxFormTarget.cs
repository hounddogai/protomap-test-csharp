namespace Acme.Workforce.Payrollops.TaxForm.Bundle;

using Acme.Workforce.Kit.Text;

/// <summary>Where a tax form target is in its life cycle.</summary>
public enum TaxFormTargetStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>One tax form target, with what the planners need to act on it.</summary>
public sealed class TaxFormTarget(string code, string title, double fte, double cost, int durationMinutes, double threshold, TaxFormTargetStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public double Fte { get; set; } = fte;
    public double Cost { get; set; } = cost;
    public int DurationMinutes { get; set; } = durationMinutes;
    public double Threshold { get; set; } = threshold;
    public TaxFormTargetStatus Status { get; set; } = status;

    /// <summary>Starts a tax form target in status created.</summary>
    public static TaxFormTarget NewTaxFormTarget(string code, string title)
    {
        return new TaxFormTarget(code, title, 0.0, 0.0, 0, 0.0, TaxFormTargetStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case TaxFormTargetStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(TaxFormTargetStatus next)
    {
        switch (Status)
        {
            case TaxFormTargetStatus.Created:
                switch (next)
                {
                    case TaxFormTargetStatus.Published:
                    case TaxFormTargetStatus.Retired:
                        return true;
                }
                return false;
            case TaxFormTargetStatus.Validated:
                switch (next)
                {
                    case TaxFormTargetStatus.Published:
                    case TaxFormTargetStatus.Retired:
                        return true;
                }
                return false;
            case TaxFormTargetStatus.Published:
                return next == TaxFormTargetStatus.Retired;
        }
        return false;
    }

    public void MoveTo(TaxFormTargetStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"tax form target {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} ({Title})";
    }

    public double EffectiveLoad()
    {
        return Math.Round(Threshold * (double)DurationMinutes, 2);
    }

    public TaxFormTarget Copy()
    {
        return new TaxFormTarget(Code, Title, Fte, Cost, DurationMinutes, Threshold, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
