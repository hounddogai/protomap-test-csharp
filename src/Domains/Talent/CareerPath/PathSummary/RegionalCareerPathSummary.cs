namespace Acme.Workforce.Talent.CareerPath.PathSummary;

using Acme.Workforce.Kit.Text;

/// <summary>Where a regional career path summary is in its life cycle.</summary>
public enum RegionalCareerPathSummaryStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>A regional career path summary as the career path team keeps it.</summary>
public sealed class RegionalCareerPathSummary(string code, string title, string name, string category, double hours, string currency, double threshold, string managerId, double cost, int createdAt, int limit, RegionalCareerPathSummaryStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string Name { get; set; } = name;
    public string Category { get; set; } = category;
    public double Hours { get; set; } = hours;
    public string Currency { get; set; } = currency;
    public double Threshold { get; set; } = threshold;
    public string ManagerId { get; set; } = managerId;
    public double Cost { get; set; } = cost;
    public int CreatedAt { get; set; } = createdAt;
    public int Limit { get; set; } = limit;
    public RegionalCareerPathSummaryStatus Status { get; set; } = status;

    /// <summary>Starts a regional career path summary in status idle.</summary>
    public static RegionalCareerPathSummary NewRegionalCareerPathSummary(string code, string title)
    {
        return new RegionalCareerPathSummary(
            code,
            title,
            "",
            "",
            0.0,
            "",
            0.0,
            "",
            0.0,
            0,
            0,
            RegionalCareerPathSummaryStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case RegionalCareerPathSummaryStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(RegionalCareerPathSummaryStatus next)
    {
        switch (Status)
        {
            case RegionalCareerPathSummaryStatus.Idle:
                return next == RegionalCareerPathSummaryStatus.Stopped;
            case RegionalCareerPathSummaryStatus.Warming:
                switch (next)
                {
                    case RegionalCareerPathSummaryStatus.Ready:
                    case RegionalCareerPathSummaryStatus.Stopped:
                        return true;
                }
                return false;
            case RegionalCareerPathSummaryStatus.Ready:
                switch (next)
                {
                    case RegionalCareerPathSummaryStatus.Stopped:
                        return true;
                }
                return false;
            case RegionalCareerPathSummaryStatus.Draining:
                switch (next)
                {
                    case RegionalCareerPathSummaryStatus.Stopped:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(RegionalCareerPathSummaryStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"regional career path summary {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        CreatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code} [{Title}]";
    }

    public double EffectiveTotal()
    {
        return Math.Round(Threshold + Cost, 2);
    }

    public RegionalCareerPathSummary Copy()
    {
        return new RegionalCareerPathSummary(
            Code,
            Title,
            Name,
            Category,
            Hours,
            Currency,
            Threshold,
            ManagerId,
            Cost,
            CreatedAt,
            Limit,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Category);
    }
}
