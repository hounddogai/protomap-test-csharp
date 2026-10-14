namespace Acme.Workforce.Assets.Laptop.RecordItem;

using Acme.Workforce.Kit.Text;

/// <summary>Where a laptop summary is in its life cycle.</summary>
public enum LaptopSummaryStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>A laptop summary as the laptop team keeps it.</summary>
public sealed class LaptopSummary(string code, string title, double floorValue, double minimum, string channel, string grade, string country, string locale, int expiresAt, string employeeId, int windowDays, LaptopSummaryStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public double FloorValue { get; set; } = floorValue;
    public double Minimum { get; set; } = minimum;
    public string Channel { get; set; } = channel;
    public string Grade { get; set; } = grade;
    public string Country { get; set; } = country;
    public string Locale { get; set; } = locale;
    public int ExpiresAt { get; set; } = expiresAt;
    public string EmployeeId { get; set; } = employeeId;
    public int WindowDays { get; set; } = windowDays;
    public LaptopSummaryStatus Status { get; set; } = status;

    /// <summary>Starts a laptop summary in status idle.</summary>
    public static LaptopSummary NewLaptopSummary(string code, string title)
    {
        return new LaptopSummary(code, title, 0.0, 0.0, "", "", "", "", 0, "", 0, LaptopSummaryStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case LaptopSummaryStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(LaptopSummaryStatus next)
    {
        switch (Status)
        {
            case LaptopSummaryStatus.Idle:
                switch (next)
                {
                    case LaptopSummaryStatus.Ready:
                    case LaptopSummaryStatus.Draining:
                        return true;
                }
                return false;
            case LaptopSummaryStatus.Warming:
                return next == LaptopSummaryStatus.Draining || next == LaptopSummaryStatus.Stopped;
            case LaptopSummaryStatus.Ready:
                switch (next)
                {
                    case LaptopSummaryStatus.Draining:
                    case LaptopSummaryStatus.Stopped:
                        return true;
                }
                return false;
            case LaptopSummaryStatus.Draining:
                switch (next)
                {
                    case LaptopSummaryStatus.Stopped:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(LaptopSummaryStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"laptop summary {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        ExpiresAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code} [{Title}]";
    }

    public double EffectiveTotal()
    {
        return Math.Round(Minimum * FloorValue, 2);
    }

    public LaptopSummary Copy()
    {
        return new LaptopSummary(
            Code,
            Title,
            FloorValue,
            Minimum,
            Channel,
            Grade,
            Country,
            Locale,
            ExpiresAt,
            EmployeeId,
            WindowDays,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Channel);
    }
}
