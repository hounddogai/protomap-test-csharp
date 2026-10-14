namespace Acme.Workforce.Contractors.Rate.Review;

using Acme.Workforce.Kit.Text;

/// <summary>Where a rate summary is in its life cycle.</summary>
public enum RateSummaryStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>A rate summary as the rate team keeps it.</summary>
public sealed class RateSummary(string code, string name, double floorValue, string phone, string title, string externalRef, string location, string timezone, double cost, RateSummaryStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public double FloorValue { get; set; } = floorValue;
    public string Phone { get; set; } = phone;
    public string Title { get; set; } = title;
    public string ExternalRef { get; set; } = externalRef;
    public string Location { get; set; } = location;
    public string Timezone { get; set; } = timezone;
    public double Cost { get; set; } = cost;
    public RateSummaryStatus Status { get; set; } = status;

    /// <summary>Starts a rate summary in status idle.</summary>
    public static RateSummary NewRateSummary(string code, string name)
    {
        return new RateSummary(code, name, 0.0, "", "", "", "", "", 0.0, RateSummaryStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case RateSummaryStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(RateSummaryStatus next)
    {
        switch (Status)
        {
            case RateSummaryStatus.Idle:
                return next == RateSummaryStatus.Warming || next == RateSummaryStatus.Stopped;
            case RateSummaryStatus.Warming:
                switch (next)
                {
                    case RateSummaryStatus.Draining:
                    case RateSummaryStatus.Stopped:
                        return true;
                }
                return false;
            case RateSummaryStatus.Ready:
                switch (next)
                {
                    case RateSummaryStatus.Draining:
                        return true;
                }
                return false;
            case RateSummaryStatus.Draining:
                return next == RateSummaryStatus.Stopped;
        }
        return false;
    }

    public void MoveTo(RateSummaryStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"rate summary {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} [{Name}]";
    }

    public double ProjectedLoad()
    {
        return Math.Round(FloorValue + Cost, 2);
    }

    public RateSummary Copy()
    {
        return new RateSummary(Code, Name, FloorValue, Phone, Title, ExternalRef, Location, Timezone, Cost, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
