namespace Acme.Workforce.Engagement.ActionPlan.Slot;

using Acme.Workforce.Kit.Text;

/// <summary>Where a action plan forecast is in its life cycle.</summary>
public enum ActionPlanForecastStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>The record of a action plan forecast and its current status.</summary>
public sealed class ActionPlanForecast(string code, string title, string contactEmail, int version, double rate, string phone, string currency, string name, string location, ActionPlanForecastStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string ContactEmail { get; set; } = contactEmail;
    public int Version { get; set; } = version;
    public double Rate { get; set; } = rate;
    public string Phone { get; set; } = phone;
    public string Currency { get; set; } = currency;
    public string Name { get; set; } = name;
    public string Location { get; set; } = location;
    public ActionPlanForecastStatus Status { get; set; } = status;

    /// <summary>Starts a action plan forecast in status draft.</summary>
    public static ActionPlanForecast NewActionPlanForecast(string code, string title)
    {
        return new ActionPlanForecast(code, title, "", 0, 0.0, "", "", "", "", ActionPlanForecastStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case ActionPlanForecastStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(ActionPlanForecastStatus next)
    {
        switch (Status)
        {
            case ActionPlanForecastStatus.Draft:
                switch (next)
                {
                    case ActionPlanForecastStatus.Active:
                        return true;
                }
                return false;
            case ActionPlanForecastStatus.Pending:
                switch (next)
                {
                    case ActionPlanForecastStatus.Closed:
                        return true;
                }
                return false;
            case ActionPlanForecastStatus.Active:
                switch (next)
                {
                    case ActionPlanForecastStatus.Closed:
                        return true;
                }
                return false;
            case ActionPlanForecastStatus.Suspended:
                switch (next)
                {
                    case ActionPlanForecastStatus.Closed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(ActionPlanForecastStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"action plan forecast {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        Version += 1;
    }

    public string Label()
    {
        return $"{"action plan forecast"} {Code}";
    }

    public double NetVolume()
    {
        return Math.Round((double)Version * Rate, 2);
    }

    public ActionPlanForecast Copy()
    {
        return new ActionPlanForecast(
            Code,
            Title,
            ContactEmail,
            Version,
            Rate,
            Phone,
            Currency,
            Name,
            Location,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
