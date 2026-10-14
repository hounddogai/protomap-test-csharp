namespace Acme.Workforce.Orgdesign.Position.Summary;

using Acme.Workforce.Kit.Text;

/// <summary>Where a position summary is in its life cycle.</summary>
public enum PositionSummaryStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>The record of a position summary and its current status.</summary>
public sealed class PositionSummary(string code, string title, string phone, double floorValue, double discount, bool pinned, string description, int endsAt, string region, string country, Dictionary<string, string> attributes, PositionSummaryStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string Phone { get; set; } = phone;
    public double FloorValue { get; set; } = floorValue;
    public double Discount { get; set; } = discount;
    public bool Pinned { get; set; } = pinned;
    public string Description { get; set; } = description;
    public int EndsAt { get; set; } = endsAt;
    public string Region { get; set; } = region;
    public string Country { get; set; } = country;
    public Dictionary<string, string> Attributes { get; set; } = attributes;
    public PositionSummaryStatus Status { get; set; } = status;

    /// <summary>Starts a position summary in status draft.</summary>
    public static PositionSummary NewPositionSummary(string code, string title)
    {
        return new PositionSummary(
            code,
            title,
            "",
            0.0,
            0.0,
            false,
            "",
            0,
            "",
            "",
            new Dictionary<string, string>(),
            PositionSummaryStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PositionSummaryStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PositionSummaryStatus next)
    {
        switch (Status)
        {
            case PositionSummaryStatus.Draft:
                return next == PositionSummaryStatus.Active;
            case PositionSummaryStatus.Pending:
                return next == PositionSummaryStatus.Active;
            case PositionSummaryStatus.Active:
                switch (next)
                {
                    case PositionSummaryStatus.Closed:
                        return true;
                }
                return false;
            case PositionSummaryStatus.Suspended:
                switch (next)
                {
                    case PositionSummaryStatus.Closed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(PositionSummaryStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"position summary {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        EndsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code} / {Title}";
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

    public double NetValue()
    {
        return Math.Round(FloorValue - (double)EndsAt, 2);
    }

    public PositionSummary Copy()
    {
        return new PositionSummary(
            Code,
            Title,
            Phone,
            FloorValue,
            Discount,
            Pinned,
            Description,
            EndsAt,
            Region,
            Country,
            Attributes,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Region);
    }
}
