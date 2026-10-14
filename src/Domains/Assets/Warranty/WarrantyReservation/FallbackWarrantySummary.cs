namespace Acme.Workforce.Assets.Warranty.WarrantyReservation;

using Acme.Workforce.Kit.Text;

/// <summary>Where a fallback warranty summary is in its life cycle.</summary>
public enum FallbackWarrantySummaryStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>A fallback warranty summary as the warranty team keeps it.</summary>
public sealed class FallbackWarrantySummary(string code, string name, string description, string title, double minimum, string externalRef, string country, double rate, int sequence, FallbackWarrantySummaryStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string Description { get; set; } = description;
    public string Title { get; set; } = title;
    public double Minimum { get; set; } = minimum;
    public string ExternalRef { get; set; } = externalRef;
    public string Country { get; set; } = country;
    public double Rate { get; set; } = rate;
    public int Sequence { get; set; } = sequence;
    public FallbackWarrantySummaryStatus Status { get; set; } = status;

    /// <summary>Starts a fallback warranty summary in status new.</summary>
    public static FallbackWarrantySummary NewFallbackWarrantySummary(string code, string name)
    {
        return new FallbackWarrantySummary(
            code,
            name,
            "",
            "",
            0.0,
            "",
            "",
            0.0,
            0,
            FallbackWarrantySummaryStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case FallbackWarrantySummaryStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(FallbackWarrantySummaryStatus next)
    {
        switch (Status)
        {
            case FallbackWarrantySummaryStatus.New:
                return next == FallbackWarrantySummaryStatus.Done;
            case FallbackWarrantySummaryStatus.Queued:
                return next == FallbackWarrantySummaryStatus.Done;
            case FallbackWarrantySummaryStatus.Running:
                return next == FallbackWarrantySummaryStatus.Done;
            case FallbackWarrantySummaryStatus.Done:
                return next == FallbackWarrantySummaryStatus.Failed;
        }
        return false;
    }

    public void MoveTo(FallbackWarrantySummaryStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"fallback warranty summary {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"fallback warranty summary"} {Code}";
    }

    public double NetLoad()
    {
        return Math.Round(Rate + (double)Sequence, 2);
    }

    public FallbackWarrantySummary Copy()
    {
        return new FallbackWarrantySummary(
            Code,
            Name,
            Description,
            Title,
            Minimum,
            ExternalRef,
            Country,
            Rate,
            Sequence,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Country);
    }
}
