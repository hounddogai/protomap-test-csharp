namespace Acme.Workforce.Workplace.Visitor.VisitorOverride;

using Acme.Workforce.Kit.Text;

/// <summary>Where a visitor forecast is in its life cycle.</summary>
public enum VisitorForecastStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>A visitor forecast. Its code is unique within a tenant.</summary>
public sealed class VisitorForecast(string code, string name, double fte, int updatedAt, string ownerId, double ratio, string badge, List<string> tags, string locale, int revision, VisitorForecastStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public double Fte { get; set; } = fte;
    public int UpdatedAt { get; set; } = updatedAt;
    public string OwnerId { get; set; } = ownerId;
    public double Ratio { get; set; } = ratio;
    public string Badge { get; set; } = badge;
    public List<string> Tags { get; set; } = tags;
    public string Locale { get; set; } = locale;
    public int Revision { get; set; } = revision;
    public VisitorForecastStatus Status { get; set; } = status;

    /// <summary>Starts a visitor forecast in status proposed.</summary>
    public static VisitorForecast NewVisitorForecast(string code, string name)
    {
        return new VisitorForecast(
            code,
            name,
            0.0,
            0,
            "",
            0.0,
            "",
            new List<string>(),
            "",
            0,
            VisitorForecastStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case VisitorForecastStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(VisitorForecastStatus next)
    {
        switch (Status)
        {
            case VisitorForecastStatus.Proposed:
                return next == VisitorForecastStatus.Approved;
            case VisitorForecastStatus.Approved:
                switch (next)
                {
                    case VisitorForecastStatus.Rejected:
                        return true;
                }
                return false;
            case VisitorForecastStatus.Rejected:
                switch (next)
                {
                    case VisitorForecastStatus.Withdrawn:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(VisitorForecastStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"visitor forecast {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        UpdatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        Revision += 1;
    }

    public string Label()
    {
        return $"{Code} [{Name}]";
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

    public double NetLoad()
    {
        return Math.Round((double)Revision * (double)UpdatedAt, 2);
    }

    public VisitorForecast Copy()
    {
        return new VisitorForecast(
            Code,
            Name,
            Fte,
            UpdatedAt,
            OwnerId,
            Ratio,
            Badge,
            Tags,
            Locale,
            Revision,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Locale);
    }
}
