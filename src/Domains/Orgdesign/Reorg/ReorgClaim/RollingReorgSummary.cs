namespace Acme.Workforce.Orgdesign.Reorg.ReorgClaim;

using Acme.Workforce.Kit.Text;

/// <summary>Where a rolling reorg summary is in its life cycle.</summary>
public enum RollingReorgSummaryStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>A rolling reorg summary as the reorg team keeps it.</summary>
public sealed class RollingReorgSummary(string code, string name, int version, string externalRef, string country, string locale, double hours, string jobCode, int endsAt, double weight, string description, RollingReorgSummaryStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public int Version { get; set; } = version;
    public string ExternalRef { get; set; } = externalRef;
    public string Country { get; set; } = country;
    public string Locale { get; set; } = locale;
    public double Hours { get; set; } = hours;
    public string JobCode { get; set; } = jobCode;
    public int EndsAt { get; set; } = endsAt;
    public double Weight { get; set; } = weight;
    public string Description { get; set; } = description;
    public RollingReorgSummaryStatus Status { get; set; } = status;

    /// <summary>Starts a rolling reorg summary in status new.</summary>
    public static RollingReorgSummary NewRollingReorgSummary(string code, string name)
    {
        return new RollingReorgSummary(
            code,
            name,
            0,
            "",
            "",
            "",
            0.0,
            "",
            0,
            0.0,
            "",
            RollingReorgSummaryStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case RollingReorgSummaryStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(RollingReorgSummaryStatus next)
    {
        switch (Status)
        {
            case RollingReorgSummaryStatus.New:
                return next == RollingReorgSummaryStatus.Queued || next == RollingReorgSummaryStatus.Failed;
            case RollingReorgSummaryStatus.Queued:
                return next == RollingReorgSummaryStatus.Done;
            case RollingReorgSummaryStatus.Running:
                switch (next)
                {
                    case RollingReorgSummaryStatus.Done:
                    case RollingReorgSummaryStatus.Failed:
                        return true;
                }
                return false;
            case RollingReorgSummaryStatus.Done:
                switch (next)
                {
                    case RollingReorgSummaryStatus.Failed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(RollingReorgSummaryStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"rolling reorg summary {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        EndsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        Version += 1;
    }

    public string Label()
    {
        return $"{"rolling reorg summary"} {Code}";
    }

    public double NetValue()
    {
        return Math.Round(Hours + (double)Version, 2);
    }

    public RollingReorgSummary Copy()
    {
        return new RollingReorgSummary(
            Code,
            Name,
            Version,
            ExternalRef,
            Country,
            Locale,
            Hours,
            JobCode,
            EndsAt,
            Weight,
            Description,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Country);
    }
}
