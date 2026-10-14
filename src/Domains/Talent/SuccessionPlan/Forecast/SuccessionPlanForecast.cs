namespace Acme.Workforce.Talent.SuccessionPlan.Forecast;

using Acme.Workforce.Kit.Text;

/// <summary>Where a succession plan forecast is in its life cycle.</summary>
public enum SuccessionPlanForecastStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>The record of a succession plan forecast and its current status.</summary>
public sealed class SuccessionPlanForecast(string code, string label, int revision, string contactEmail, bool visible, int quantity, SuccessionPlanForecastStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public int Revision { get; set; } = revision;
    public string ContactEmail { get; set; } = contactEmail;
    public bool Visible { get; set; } = visible;
    public int Quantity { get; set; } = quantity;
    public SuccessionPlanForecastStatus Status { get; set; } = status;

    /// <summary>Starts a succession plan forecast in status created.</summary>
    public static SuccessionPlanForecast NewSuccessionPlanForecast(string code, string label)
    {
        return new SuccessionPlanForecast(code, label, 0, "", false, 0, SuccessionPlanForecastStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case SuccessionPlanForecastStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(SuccessionPlanForecastStatus next)
    {
        switch (Status)
        {
            case SuccessionPlanForecastStatus.Created:
                switch (next)
                {
                    case SuccessionPlanForecastStatus.Published:
                        return true;
                }
                return false;
            case SuccessionPlanForecastStatus.Validated:
                return next == SuccessionPlanForecastStatus.Published || next == SuccessionPlanForecastStatus.Retired;
            case SuccessionPlanForecastStatus.Published:
                switch (next)
                {
                    case SuccessionPlanForecastStatus.Retired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(SuccessionPlanForecastStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"succession plan forecast {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        Revision += 1;
    }

    public string DisplayLabel()
    {
        return $"{"succession plan forecast"} {Code}";
    }

    public double NetExposure()
    {
        return Math.Round((double)Quantity * (double)Revision, 2);
    }

    public SuccessionPlanForecast Copy()
    {
        return new SuccessionPlanForecast(Code, Label, Revision, ContactEmail, Visible, Quantity, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
