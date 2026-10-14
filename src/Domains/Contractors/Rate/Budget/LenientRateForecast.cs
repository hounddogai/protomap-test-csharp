namespace Acme.Workforce.Contractors.Rate.Budget;

using Acme.Workforce.Kit.Text;

/// <summary>Where a lenient rate forecast is in its life cycle.</summary>
public enum LenientRateForecastStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>A lenient rate forecast. Its code is unique within a tenant.</summary>
public sealed class LenientRateForecast(string code, string label, double budget, string reviewer, bool approved, int capacity, LenientRateForecastStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public double Budget { get; set; } = budget;
    public string Reviewer { get; set; } = reviewer;
    public bool Approved { get; set; } = approved;
    public int Capacity { get; set; } = capacity;
    public LenientRateForecastStatus Status { get; set; } = status;

    /// <summary>Starts a lenient rate forecast in status open.</summary>
    public static LenientRateForecast NewLenientRateForecast(string code, string label)
    {
        return new LenientRateForecast(code, label, 0.0, "", false, 0, LenientRateForecastStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case LenientRateForecastStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(LenientRateForecastStatus next)
    {
        switch (Status)
        {
            case LenientRateForecastStatus.Open:
                switch (next)
                {
                    case LenientRateForecastStatus.Resolved:
                    case LenientRateForecastStatus.Archived:
                        return true;
                }
                return false;
            case LenientRateForecastStatus.InReview:
                return next == LenientRateForecastStatus.Reopened;
            case LenientRateForecastStatus.Resolved:
                return next == LenientRateForecastStatus.Archived;
            case LenientRateForecastStatus.Reopened:
                return next == LenientRateForecastStatus.Archived;
        }
        return false;
    }

    public void MoveTo(LenientRateForecastStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"lenient rate forecast {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code}: {Label}";
    }

    public double NetTotal()
    {
        return Math.Round((double)Capacity * Budget, 2);
    }

    public LenientRateForecast Copy()
    {
        return new LenientRateForecast(Code, Label, Budget, Reviewer, Approved, Capacity, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
