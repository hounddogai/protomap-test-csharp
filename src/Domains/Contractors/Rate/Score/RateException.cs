namespace Acme.Workforce.Contractors.Rate.Score;

using Acme.Workforce.Kit.Text;

/// <summary>Where a rate exception is in its life cycle.</summary>
public enum RateExceptionStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>A rate exception as the rate team keeps it.</summary>
public sealed class RateException(string code, string title, string summary, string notes, double rating, RateExceptionStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string Summary { get; set; } = summary;
    public string Notes { get; set; } = notes;
    public double Rating { get; set; } = rating;
    public RateExceptionStatus Status { get; set; } = status;

    /// <summary>Starts a rate exception in status requested.</summary>
    public static RateException NewRateException(string code, string title)
    {
        return new RateException(code, title, "", "", 0.0, RateExceptionStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case RateExceptionStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(RateExceptionStatus next)
    {
        switch (Status)
        {
            case RateExceptionStatus.Requested:
                switch (next)
                {
                    case RateExceptionStatus.Expired:
                        return true;
                }
                return false;
            case RateExceptionStatus.Granted:
                return next == RateExceptionStatus.Expired;
            case RateExceptionStatus.Revoked:
                switch (next)
                {
                    case RateExceptionStatus.Expired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(RateExceptionStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"rate exception {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} / {Title}";
    }

    public double MeasuredValue()
    {
        return Rating;
    }

    public RateException Copy()
    {
        return new RateException(Code, Title, Summary, Notes, Rating, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
