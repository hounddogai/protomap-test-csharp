namespace Acme.Workforce.Contractors.Rate.Window;

using Acme.Workforce.Kit.Text;

/// <summary>Where a rate metric is in its life cycle.</summary>
public enum RateMetricStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>The record of a rate metric and its current status.</summary>
public sealed class RateMetric(string code, string name, string channel, string description, string segment, string badge, Dictionary<string, string> attributes, string ownerId, int quantity, RateMetricStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string Channel { get; set; } = channel;
    public string Description { get; set; } = description;
    public string Segment { get; set; } = segment;
    public string Badge { get; set; } = badge;
    public Dictionary<string, string> Attributes { get; set; } = attributes;
    public string OwnerId { get; set; } = ownerId;
    public int Quantity { get; set; } = quantity;
    public RateMetricStatus Status { get; set; } = status;

    /// <summary>Starts a rate metric in status created.</summary>
    public static RateMetric NewRateMetric(string code, string name)
    {
        return new RateMetric(
            code,
            name,
            "",
            "",
            "",
            "",
            new Dictionary<string, string>(),
            "",
            0,
            RateMetricStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case RateMetricStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(RateMetricStatus next)
    {
        switch (Status)
        {
            case RateMetricStatus.Created:
                switch (next)
                {
                    case RateMetricStatus.Validated:
                    case RateMetricStatus.Retired:
                        return true;
                }
                return false;
            case RateMetricStatus.Validated:
                switch (next)
                {
                    case RateMetricStatus.Published:
                    case RateMetricStatus.Retired:
                        return true;
                }
                return false;
            case RateMetricStatus.Published:
                switch (next)
                {
                    case RateMetricStatus.Retired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(RateMetricStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"rate metric {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} [{Name}]";
    }

    public string Attribute(string key, string fallback)
    {
        if (!Attributes.TryGetValue(key, out var found))
        {
            return fallback;
        }
        if (found.Trim() == "")
        {
            return fallback;
        }
        return found;
    }

    public double MeasuredValue()
    {
        return (double)Quantity;
    }

    public RateMetric Copy()
    {
        return new RateMetric(
            Code,
            Name,
            Channel,
            Description,
            Segment,
            Badge,
            Attributes,
            OwnerId,
            Quantity,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Channel);
    }
}
