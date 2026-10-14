namespace Acme.Workforce.Engagement.PulseSurvey.Metric;

using Acme.Workforce.Kit.Text;

/// <summary>Where a manual pulse survey forecast is in its life cycle.</summary>
public enum ManualPulseSurveyForecastStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>A manual pulse survey forecast as the pulse survey team keeps it.</summary>
public sealed class ManualPulseSurveyForecast(string code, string label, string externalRef, string managerId, double margin, int limit, ManualPulseSurveyForecastStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string ExternalRef { get; set; } = externalRef;
    public string ManagerId { get; set; } = managerId;
    public double Margin { get; set; } = margin;
    public int Limit { get; set; } = limit;
    public ManualPulseSurveyForecastStatus Status { get; set; } = status;

    /// <summary>Starts a manual pulse survey forecast in status idle.</summary>
    public static ManualPulseSurveyForecast NewManualPulseSurveyForecast(string code, string label)
    {
        return new ManualPulseSurveyForecast(code, label, "", "", 0.0, 0, ManualPulseSurveyForecastStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case ManualPulseSurveyForecastStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(ManualPulseSurveyForecastStatus next)
    {
        switch (Status)
        {
            case ManualPulseSurveyForecastStatus.Idle:
                switch (next)
                {
                    case ManualPulseSurveyForecastStatus.Warming:
                    case ManualPulseSurveyForecastStatus.Stopped:
                        return true;
                }
                return false;
            case ManualPulseSurveyForecastStatus.Warming:
                switch (next)
                {
                    case ManualPulseSurveyForecastStatus.Draining:
                    case ManualPulseSurveyForecastStatus.Stopped:
                        return true;
                }
                return false;
            case ManualPulseSurveyForecastStatus.Ready:
                switch (next)
                {
                    case ManualPulseSurveyForecastStatus.Draining:
                    case ManualPulseSurveyForecastStatus.Stopped:
                        return true;
                }
                return false;
            case ManualPulseSurveyForecastStatus.Draining:
                return next == ManualPulseSurveyForecastStatus.Stopped;
        }
        return false;
    }

    public void MoveTo(ManualPulseSurveyForecastStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"manual pulse survey forecast {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{"manual pulse survey forecast"} {Code}";
    }

    public double EffectiveTotal()
    {
        return Math.Round(Margin - (double)Limit, 2);
    }

    public ManualPulseSurveyForecast Copy()
    {
        return new ManualPulseSurveyForecast(Code, Label, ExternalRef, ManagerId, Margin, Limit, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
