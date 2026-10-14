namespace Acme.Workforce.Compensation.EquityGrant.Exception;

using Acme.Workforce.Kit.Text;

/// <summary>Where a lenient equity grant summary is in its life cycle.</summary>
public enum LenientEquityGrantSummaryStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>One lenient equity grant summary, with what the planners need to act on it.</summary>
public sealed class LenientEquityGrantSummary(string code, string title, string contactEmail, double rate, double cost, LenientEquityGrantSummaryStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string ContactEmail { get; set; } = contactEmail;
    public double Rate { get; set; } = rate;
    public double Cost { get; set; } = cost;
    public LenientEquityGrantSummaryStatus Status { get; set; } = status;

    /// <summary>Starts a lenient equity grant summary in status requested.</summary>
    public static LenientEquityGrantSummary NewLenientEquityGrantSummary(string code, string title)
    {
        return new LenientEquityGrantSummary(code, title, "", 0.0, 0.0, LenientEquityGrantSummaryStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case LenientEquityGrantSummaryStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(LenientEquityGrantSummaryStatus next)
    {
        switch (Status)
        {
            case LenientEquityGrantSummaryStatus.Requested:
                switch (next)
                {
                    case LenientEquityGrantSummaryStatus.Granted:
                        return true;
                }
                return false;
            case LenientEquityGrantSummaryStatus.Granted:
                switch (next)
                {
                    case LenientEquityGrantSummaryStatus.Revoked:
                        return true;
                }
                return false;
            case LenientEquityGrantSummaryStatus.Revoked:
                switch (next)
                {
                    case LenientEquityGrantSummaryStatus.Expired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(LenientEquityGrantSummaryStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"lenient equity grant summary {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"lenient equity grant summary"} {Code}";
    }

    public double NetTotal()
    {
        return Math.Round(Cost * Rate, 2);
    }

    public LenientEquityGrantSummary Copy()
    {
        return new LenientEquityGrantSummary(Code, Title, ContactEmail, Rate, Cost, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
