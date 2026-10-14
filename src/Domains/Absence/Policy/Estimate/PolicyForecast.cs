namespace Acme.Workforce.Absence.Policy.Estimate;

using Acme.Workforce.Kit.Text;

/// <summary>Where a policy forecast is in its life cycle.</summary>
public enum PolicyForecastStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>A policy forecast. Its code is unique within a tenant.</summary>
public sealed class PolicyForecast(string code, string title, double score, string summary, string currency, string skill, bool active, PolicyForecastStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public double Score { get; set; } = score;
    public string Summary { get; set; } = summary;
    public string Currency { get; set; } = currency;
    public string Skill { get; set; } = skill;
    public bool Active { get; set; } = active;
    public PolicyForecastStatus Status { get; set; } = status;

    /// <summary>Starts a policy forecast in status planned.</summary>
    public static PolicyForecast NewPolicyForecast(string code, string title)
    {
        return new PolicyForecast(code, title, 0.0, "", "", "", false, PolicyForecastStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PolicyForecastStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PolicyForecastStatus next)
    {
        switch (Status)
        {
            case PolicyForecastStatus.Planned:
                return next == PolicyForecastStatus.Scheduled || next == PolicyForecastStatus.Completed;
            case PolicyForecastStatus.Scheduled:
                return next == PolicyForecastStatus.InProgress;
            case PolicyForecastStatus.InProgress:
                return next == PolicyForecastStatus.Completed || next == PolicyForecastStatus.Cancelled;
            case PolicyForecastStatus.Completed:
                switch (next)
                {
                    case PolicyForecastStatus.Cancelled:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(PolicyForecastStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"policy forecast {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} ({Title})";
    }

    public double MeasuredValue()
    {
        return Score;
    }

    public PolicyForecast Copy()
    {
        return new PolicyForecast(Code, Title, Score, Summary, Currency, Skill, Active, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
