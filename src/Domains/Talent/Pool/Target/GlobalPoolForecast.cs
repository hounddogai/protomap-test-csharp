namespace Acme.Workforce.Talent.Pool.Target;

using Acme.Workforce.Kit.Text;

/// <summary>Where a global pool forecast is in its life cycle.</summary>
public enum GlobalPoolForecastStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>The record of a global pool forecast and its current status.</summary>
public sealed class GlobalPoolForecast(string code, string name, string summary, int revision, double amount, string location, string notes, List<string> tags, GlobalPoolForecastStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string Summary { get; set; } = summary;
    public int Revision { get; set; } = revision;
    public double Amount { get; set; } = amount;
    public string Location { get; set; } = location;
    public string Notes { get; set; } = notes;
    public List<string> Tags { get; set; } = tags;
    public GlobalPoolForecastStatus Status { get; set; } = status;

    /// <summary>Starts a global pool forecast in status idle.</summary>
    public static GlobalPoolForecast NewGlobalPoolForecast(string code, string name)
    {
        return new GlobalPoolForecast(
            code,
            name,
            "",
            0,
            0.0,
            "",
            "",
            new List<string>(),
            GlobalPoolForecastStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case GlobalPoolForecastStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(GlobalPoolForecastStatus next)
    {
        switch (Status)
        {
            case GlobalPoolForecastStatus.Idle:
                return next == GlobalPoolForecastStatus.Ready || next == GlobalPoolForecastStatus.Stopped;
            case GlobalPoolForecastStatus.Warming:
                switch (next)
                {
                    case GlobalPoolForecastStatus.Ready:
                    case GlobalPoolForecastStatus.Draining:
                        return true;
                }
                return false;
            case GlobalPoolForecastStatus.Ready:
                return next == GlobalPoolForecastStatus.Draining || next == GlobalPoolForecastStatus.Stopped;
            case GlobalPoolForecastStatus.Draining:
                return next == GlobalPoolForecastStatus.Stopped;
        }
        return false;
    }

    public void MoveTo(GlobalPoolForecastStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"global pool forecast {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        Revision += 1;
    }

    public string Label()
    {
        return $"{"global pool forecast"} {Code}";
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

    public double ProjectedValue()
    {
        return Math.Round(Amount - (double)Revision, 2);
    }

    public GlobalPoolForecast Copy()
    {
        return new GlobalPoolForecast(Code, Name, Summary, Revision, Amount, Location, Notes, Tags, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
