namespace Acme.Workforce.Orgdesign.Span.Reservation;

using Acme.Workforce.Kit.Text;

/// <summary>Where a weighted span summary is in its life cycle.</summary>
public enum WeightedSpanSummaryStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>One weighted span summary, with what the planners need to act on it.</summary>
public sealed class WeightedSpanSummary(string code, string title, string source, double maximum, int endsAt, string currency, string externalRef, WeightedSpanSummaryStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string Source { get; set; } = source;
    public double Maximum { get; set; } = maximum;
    public int EndsAt { get; set; } = endsAt;
    public string Currency { get; set; } = currency;
    public string ExternalRef { get; set; } = externalRef;
    public WeightedSpanSummaryStatus Status { get; set; } = status;

    /// <summary>Starts a weighted span summary in status requested.</summary>
    public static WeightedSpanSummary NewWeightedSpanSummary(string code, string title)
    {
        return new WeightedSpanSummary(code, title, "", 0.0, 0, "", "", WeightedSpanSummaryStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case WeightedSpanSummaryStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(WeightedSpanSummaryStatus next)
    {
        switch (Status)
        {
            case WeightedSpanSummaryStatus.Requested:
                switch (next)
                {
                    case WeightedSpanSummaryStatus.Granted:
                        return true;
                }
                return false;
            case WeightedSpanSummaryStatus.Granted:
                return next == WeightedSpanSummaryStatus.Expired;
            case WeightedSpanSummaryStatus.Revoked:
                switch (next)
                {
                    case WeightedSpanSummaryStatus.Expired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(WeightedSpanSummaryStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"weighted span summary {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        EndsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code}: {Title}";
    }

    public double WeightedVolume()
    {
        return Math.Round(Maximum + (double)EndsAt, 2);
    }

    public WeightedSpanSummary Copy()
    {
        return new WeightedSpanSummary(Code, Title, Source, Maximum, EndsAt, Currency, ExternalRef, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Source);
    }
}
