namespace Acme.Workforce.Contractors.Rate.Bundle;

using Acme.Workforce.Kit.Text;

/// <summary>Where a rate variance is in its life cycle.</summary>
public enum RateVarianceStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>A rate variance. Its code is unique within a tenant.</summary>
public sealed class RateVariance(string code, string title, string source, string locale, double weight, RateVarianceStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string Source { get; set; } = source;
    public string Locale { get; set; } = locale;
    public double Weight { get; set; } = weight;
    public RateVarianceStatus Status { get; set; } = status;

    /// <summary>Starts a rate variance in status requested.</summary>
    public static RateVariance NewRateVariance(string code, string title)
    {
        return new RateVariance(code, title, "", "", 0.0, RateVarianceStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case RateVarianceStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(RateVarianceStatus next)
    {
        switch (Status)
        {
            case RateVarianceStatus.Requested:
                return next == RateVarianceStatus.Granted;
            case RateVarianceStatus.Granted:
                switch (next)
                {
                    case RateVarianceStatus.Revoked:
                        return true;
                }
                return false;
            case RateVarianceStatus.Revoked:
                return next == RateVarianceStatus.Expired;
        }
        return false;
    }

    public void MoveTo(RateVarianceStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"rate variance {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"rate variance"} {Code}";
    }

    public double MeasuredValue()
    {
        return Weight;
    }

    public RateVariance Copy()
    {
        return new RateVariance(Code, Title, Source, Locale, Weight, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Source);
    }
}
