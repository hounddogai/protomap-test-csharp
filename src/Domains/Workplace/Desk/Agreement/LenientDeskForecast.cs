namespace Acme.Workforce.Workplace.Desk.Agreement;

using Acme.Workforce.Kit.Text;

/// <summary>Where a lenient desk forecast is in its life cycle.</summary>
public enum LenientDeskForecastStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>A lenient desk forecast. Its code is unique within a tenant.</summary>
public sealed class LenientDeskForecast(string code, string name, string reviewer, bool active, List<string> tags, double maximum, double margin, string department, string label, LenientDeskForecastStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string Reviewer { get; set; } = reviewer;
    public bool Active { get; set; } = active;
    public List<string> Tags { get; set; } = tags;
    public double Maximum { get; set; } = maximum;
    public double Margin { get; set; } = margin;
    public string Department { get; set; } = department;
    public string Label { get; set; } = label;
    public LenientDeskForecastStatus Status { get; set; } = status;

    /// <summary>Starts a lenient desk forecast in status created.</summary>
    public static LenientDeskForecast NewLenientDeskForecast(string code, string name)
    {
        return new LenientDeskForecast(
            code,
            name,
            "",
            false,
            new List<string>(),
            0.0,
            0.0,
            "",
            "",
            LenientDeskForecastStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case LenientDeskForecastStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(LenientDeskForecastStatus next)
    {
        switch (Status)
        {
            case LenientDeskForecastStatus.Created:
                return next == LenientDeskForecastStatus.Retired;
            case LenientDeskForecastStatus.Validated:
                return next == LenientDeskForecastStatus.Retired;
            case LenientDeskForecastStatus.Published:
                return next == LenientDeskForecastStatus.Retired;
        }
        return false;
    }

    public void MoveTo(LenientDeskForecastStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"lenient desk forecast {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code}: {Name}";
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

    public double WeightedExposure()
    {
        return Math.Round(Margin - Maximum, 2);
    }

    public LenientDeskForecast Copy()
    {
        return new LenientDeskForecast(
            Code,
            Name,
            Reviewer,
            Active,
            Tags,
            Maximum,
            Margin,
            Department,
            Label,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Department);
    }
}
