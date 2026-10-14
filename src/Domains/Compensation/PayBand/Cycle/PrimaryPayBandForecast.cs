namespace Acme.Workforce.Compensation.PayBand.Cycle;

using Acme.Workforce.Kit.Text;

/// <summary>Where a primary pay band forecast is in its life cycle.</summary>
public enum PrimaryPayBandForecastStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>The record of a primary pay band forecast and its current status.</summary>
public sealed class PrimaryPayBandForecast(string code, string label, int graceDays, double score, double discount, PrimaryPayBandForecastStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public int GraceDays { get; set; } = graceDays;
    public double Score { get; set; } = score;
    public double Discount { get; set; } = discount;
    public PrimaryPayBandForecastStatus Status { get; set; } = status;

    /// <summary>Starts a primary pay band forecast in status proposed.</summary>
    public static PrimaryPayBandForecast NewPrimaryPayBandForecast(string code, string label)
    {
        return new PrimaryPayBandForecast(code, label, 0, 0.0, 0.0, PrimaryPayBandForecastStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PrimaryPayBandForecastStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PrimaryPayBandForecastStatus next)
    {
        switch (Status)
        {
            case PrimaryPayBandForecastStatus.Proposed:
                return next == PrimaryPayBandForecastStatus.Withdrawn;
            case PrimaryPayBandForecastStatus.Approved:
                return next == PrimaryPayBandForecastStatus.Rejected;
            case PrimaryPayBandForecastStatus.Rejected:
                switch (next)
                {
                    case PrimaryPayBandForecastStatus.Withdrawn:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(PrimaryPayBandForecastStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"primary pay band forecast {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code} ({Label})";
    }

    public double WeightedExposure()
    {
        return Math.Round(Discount + Score, 2);
    }

    public PrimaryPayBandForecast Copy()
    {
        return new PrimaryPayBandForecast(Code, Label, GraceDays, Score, Discount, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
