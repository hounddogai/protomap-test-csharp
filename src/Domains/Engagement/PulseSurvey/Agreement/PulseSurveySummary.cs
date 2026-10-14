namespace Acme.Workforce.Engagement.PulseSurvey.Agreement;

using Acme.Workforce.Kit.Text;

/// <summary>Where a pulse survey summary is in its life cycle.</summary>
public enum PulseSurveySummaryStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>One pulse survey summary, with what the planners need to act on it.</summary>
public sealed class PulseSurveySummary(string code, string name, int capacity, double rating, string currency, int priority, bool pinned, double threshold, double ceiling, PulseSurveySummaryStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public int Capacity { get; set; } = capacity;
    public double Rating { get; set; } = rating;
    public string Currency { get; set; } = currency;
    public int Priority { get; set; } = priority;
    public bool Pinned { get; set; } = pinned;
    public double Threshold { get; set; } = threshold;
    public double Ceiling { get; set; } = ceiling;
    public PulseSurveySummaryStatus Status { get; set; } = status;

    /// <summary>Starts a pulse survey summary in status new.</summary>
    public static PulseSurveySummary NewPulseSurveySummary(string code, string name)
    {
        return new PulseSurveySummary(code, name, 0, 0.0, "", 0, false, 0.0, 0.0, PulseSurveySummaryStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PulseSurveySummaryStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PulseSurveySummaryStatus next)
    {
        switch (Status)
        {
            case PulseSurveySummaryStatus.New:
                switch (next)
                {
                    case PulseSurveySummaryStatus.Queued:
                        return true;
                }
                return false;
            case PulseSurveySummaryStatus.Queued:
                return next == PulseSurveySummaryStatus.Done;
            case PulseSurveySummaryStatus.Running:
                return next == PulseSurveySummaryStatus.Done || next == PulseSurveySummaryStatus.Failed;
            case PulseSurveySummaryStatus.Done:
                switch (next)
                {
                    case PulseSurveySummaryStatus.Failed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(PulseSurveySummaryStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"pulse survey summary {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code}: {Name}";
    }

    public double WeightedLoad()
    {
        return Math.Round(Ceiling * (double)Priority, 2);
    }

    public PulseSurveySummary Copy()
    {
        return new PulseSurveySummary(
            Code,
            Name,
            Capacity,
            Rating,
            Currency,
            Priority,
            Pinned,
            Threshold,
            Ceiling,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
