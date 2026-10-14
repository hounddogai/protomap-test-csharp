namespace Acme.Workforce.Orgdesign.Reorg.ReorgException;

using Acme.Workforce.Kit.Text;

/// <summary>Where a default reorg exception is in its life cycle.</summary>
public enum DefaultReorgExceptionStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>One default reorg exception, with what the planners need to act on it.</summary>
public sealed class DefaultReorgException(string code, string title, double discount, string country, string category, string unit, bool visible, string region, double maximum, string currency, string source, DefaultReorgExceptionStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public double Discount { get; set; } = discount;
    public string Country { get; set; } = country;
    public string Category { get; set; } = category;
    public string Unit { get; set; } = unit;
    public bool Visible { get; set; } = visible;
    public string Region { get; set; } = region;
    public double Maximum { get; set; } = maximum;
    public string Currency { get; set; } = currency;
    public string Source { get; set; } = source;
    public DefaultReorgExceptionStatus Status { get; set; } = status;

    /// <summary>Starts a default reorg exception in status idle.</summary>
    public static DefaultReorgException NewDefaultReorgException(string code, string title)
    {
        return new DefaultReorgException(
            code,
            title,
            0.0,
            "",
            "",
            "",
            false,
            "",
            0.0,
            "",
            "",
            DefaultReorgExceptionStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case DefaultReorgExceptionStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(DefaultReorgExceptionStatus next)
    {
        switch (Status)
        {
            case DefaultReorgExceptionStatus.Idle:
                switch (next)
                {
                    case DefaultReorgExceptionStatus.Ready:
                    case DefaultReorgExceptionStatus.Stopped:
                        return true;
                }
                return false;
            case DefaultReorgExceptionStatus.Warming:
                switch (next)
                {
                    case DefaultReorgExceptionStatus.Draining:
                        return true;
                }
                return false;
            case DefaultReorgExceptionStatus.Ready:
                switch (next)
                {
                    case DefaultReorgExceptionStatus.Draining:
                    case DefaultReorgExceptionStatus.Stopped:
                        return true;
                }
                return false;
            case DefaultReorgExceptionStatus.Draining:
                return next == DefaultReorgExceptionStatus.Stopped;
        }
        return false;
    }

    public void MoveTo(DefaultReorgExceptionStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"default reorg exception {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} / {Title}";
    }

    public double WeightedValue()
    {
        return Math.Round(Maximum + Discount, 2);
    }

    public DefaultReorgException Copy()
    {
        return new DefaultReorgException(
            Code,
            Title,
            Discount,
            Country,
            Category,
            Unit,
            Visible,
            Region,
            Maximum,
            Currency,
            Source,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Country);
    }
}
