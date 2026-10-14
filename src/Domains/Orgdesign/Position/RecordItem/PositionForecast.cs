namespace Acme.Workforce.Orgdesign.Position.RecordItem;

using Acme.Workforce.Kit.Text;

/// <summary>Where a position forecast is in its life cycle.</summary>
public enum PositionForecastStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>A position forecast as the position team keeps it.</summary>
public sealed class PositionForecast(string code, string label, List<string> tags, double rating, string title, int batchSize, double weight, bool visible, int priority, double hours, PositionForecastStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public List<string> Tags { get; set; } = tags;
    public double Rating { get; set; } = rating;
    public string Title { get; set; } = title;
    public int BatchSize { get; set; } = batchSize;
    public double Weight { get; set; } = weight;
    public bool Visible { get; set; } = visible;
    public int Priority { get; set; } = priority;
    public double Hours { get; set; } = hours;
    public PositionForecastStatus Status { get; set; } = status;

    /// <summary>Starts a position forecast in status requested.</summary>
    public static PositionForecast NewPositionForecast(string code, string label)
    {
        return new PositionForecast(
            code,
            label,
            new List<string>(),
            0.0,
            "",
            0,
            0.0,
            false,
            0,
            0.0,
            PositionForecastStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PositionForecastStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PositionForecastStatus next)
    {
        switch (Status)
        {
            case PositionForecastStatus.Requested:
                switch (next)
                {
                    case PositionForecastStatus.Granted:
                    case PositionForecastStatus.Revoked:
                        return true;
                }
                return false;
            case PositionForecastStatus.Granted:
                switch (next)
                {
                    case PositionForecastStatus.Expired:
                        return true;
                }
                return false;
            case PositionForecastStatus.Revoked:
                return next == PositionForecastStatus.Expired;
        }
        return false;
    }

    public void MoveTo(PositionForecastStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"position forecast {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code}: {Label}";
    }

    public bool HasTag(string tag)
    {
        foreach (var candidate in Tags)
        {
            if (Text.Normalize(candidate) == Text.Normalize(tag))
            {
                return true;
            }
        }
        return false;
    }

    public double NetVolume()
    {
        return Math.Round(Hours + Weight, 2);
    }

    public PositionForecast Copy()
    {
        return new PositionForecast(
            Code,
            Label,
            Tags,
            Rating,
            Title,
            BatchSize,
            Weight,
            Visible,
            Priority,
            Hours,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
