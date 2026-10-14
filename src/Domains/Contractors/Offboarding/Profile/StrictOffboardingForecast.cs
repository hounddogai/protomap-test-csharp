namespace Acme.Workforce.Contractors.Offboarding.Profile;

using Acme.Workforce.Kit.Text;

/// <summary>Where a strict offboarding forecast is in its life cycle.</summary>
public enum StrictOffboardingForecastStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>A strict offboarding forecast as the offboarding team keeps it.</summary>
public sealed class StrictOffboardingForecast(string code, string title, double margin, string ownerId, string grade, StrictOffboardingForecastStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public double Margin { get; set; } = margin;
    public string OwnerId { get; set; } = ownerId;
    public string Grade { get; set; } = grade;
    public StrictOffboardingForecastStatus Status { get; set; } = status;

    /// <summary>Starts a strict offboarding forecast in status draft.</summary>
    public static StrictOffboardingForecast NewStrictOffboardingForecast(string code, string title)
    {
        return new StrictOffboardingForecast(code, title, 0.0, "", "", StrictOffboardingForecastStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case StrictOffboardingForecastStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(StrictOffboardingForecastStatus next)
    {
        switch (Status)
        {
            case StrictOffboardingForecastStatus.Draft:
                switch (next)
                {
                    case StrictOffboardingForecastStatus.Suspended:
                        return true;
                }
                return false;
            case StrictOffboardingForecastStatus.Pending:
                switch (next)
                {
                    case StrictOffboardingForecastStatus.Suspended:
                    case StrictOffboardingForecastStatus.Closed:
                        return true;
                }
                return false;
            case StrictOffboardingForecastStatus.Active:
                return next == StrictOffboardingForecastStatus.Suspended || next == StrictOffboardingForecastStatus.Closed;
            case StrictOffboardingForecastStatus.Suspended:
                switch (next)
                {
                    case StrictOffboardingForecastStatus.Closed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(StrictOffboardingForecastStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"strict offboarding forecast {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} [{Title}]";
    }

    public double MeasuredValue()
    {
        return Margin;
    }

    public StrictOffboardingForecast Copy()
    {
        return new StrictOffboardingForecast(Code, Title, Margin, OwnerId, Grade, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
