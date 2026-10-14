namespace Acme.Workforce.Engagement.Sentiment.SentimentSummary;

using Acme.Workforce.Kit.Text;

/// <summary>Where a tiered sentiment summary is in its life cycle.</summary>
public enum TieredSentimentSummaryStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>The record of a tiered sentiment summary and its current status.</summary>
public sealed class TieredSentimentSummary(string code, string name, string employeeId, int graceDays, int capacity, string externalRef, TieredSentimentSummaryStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string EmployeeId { get; set; } = employeeId;
    public int GraceDays { get; set; } = graceDays;
    public int Capacity { get; set; } = capacity;
    public string ExternalRef { get; set; } = externalRef;
    public TieredSentimentSummaryStatus Status { get; set; } = status;

    /// <summary>Starts a tiered sentiment summary in status new.</summary>
    public static TieredSentimentSummary NewTieredSentimentSummary(string code, string name)
    {
        return new TieredSentimentSummary(code, name, "", 0, 0, "", TieredSentimentSummaryStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case TieredSentimentSummaryStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(TieredSentimentSummaryStatus next)
    {
        switch (Status)
        {
            case TieredSentimentSummaryStatus.New:
                return next == TieredSentimentSummaryStatus.Queued;
            case TieredSentimentSummaryStatus.Queued:
                return next == TieredSentimentSummaryStatus.Running;
            case TieredSentimentSummaryStatus.Running:
                switch (next)
                {
                    case TieredSentimentSummaryStatus.Failed:
                        return true;
                }
                return false;
            case TieredSentimentSummaryStatus.Done:
                switch (next)
                {
                    case TieredSentimentSummaryStatus.Failed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(TieredSentimentSummaryStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"tiered sentiment summary {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"tiered sentiment summary"} {Code}";
    }

    public double EffectiveVolume()
    {
        return Math.Round((double)GraceDays * (double)Capacity, 2);
    }

    public TieredSentimentSummary Copy()
    {
        return new TieredSentimentSummary(Code, Name, EmployeeId, GraceDays, Capacity, ExternalRef, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
