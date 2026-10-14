namespace Acme.Workforce.Workplace.Visitor.VisitorEntry;

using Acme.Workforce.Kit.Text;

/// <summary>Where a default visitor metric is in its life cycle.</summary>
public enum DefaultVisitorMetricStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>A default visitor metric as the visitor team keeps it.</summary>
public sealed class DefaultVisitorMetric(string code, string label, int updatedAt, string title, bool pinned, string channel, int retries, string country, double discount, string timezone, DefaultVisitorMetricStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public int UpdatedAt { get; set; } = updatedAt;
    public string Title { get; set; } = title;
    public bool Pinned { get; set; } = pinned;
    public string Channel { get; set; } = channel;
    public int Retries { get; set; } = retries;
    public string Country { get; set; } = country;
    public double Discount { get; set; } = discount;
    public string Timezone { get; set; } = timezone;
    public DefaultVisitorMetricStatus Status { get; set; } = status;

    /// <summary>Starts a default visitor metric in status idle.</summary>
    public static DefaultVisitorMetric NewDefaultVisitorMetric(string code, string label)
    {
        return new DefaultVisitorMetric(
            code,
            label,
            0,
            "",
            false,
            "",
            0,
            "",
            0.0,
            "",
            DefaultVisitorMetricStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case DefaultVisitorMetricStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(DefaultVisitorMetricStatus next)
    {
        switch (Status)
        {
            case DefaultVisitorMetricStatus.Idle:
                switch (next)
                {
                    case DefaultVisitorMetricStatus.Warming:
                    case DefaultVisitorMetricStatus.Stopped:
                        return true;
                }
                return false;
            case DefaultVisitorMetricStatus.Warming:
                switch (next)
                {
                    case DefaultVisitorMetricStatus.Draining:
                        return true;
                }
                return false;
            case DefaultVisitorMetricStatus.Ready:
                return next == DefaultVisitorMetricStatus.Draining;
            case DefaultVisitorMetricStatus.Draining:
                return next == DefaultVisitorMetricStatus.Stopped;
        }
        return false;
    }

    public void MoveTo(DefaultVisitorMetricStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"default visitor metric {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        UpdatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{Code} [{Label}]";
    }

    public double ProjectedExposure()
    {
        return Math.Round((double)UpdatedAt * (double)Retries, 2);
    }

    public DefaultVisitorMetric Copy()
    {
        return new DefaultVisitorMetric(
            Code,
            Label,
            UpdatedAt,
            Title,
            Pinned,
            Channel,
            Retries,
            Country,
            Discount,
            Timezone,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Channel);
    }
}
