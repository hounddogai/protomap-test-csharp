namespace Acme.Workforce.Engagement.PulseSurvey.SurveyTier;

using Acme.Workforce.Kit.Text;

/// <summary>Where a pulse survey plan is in its life cycle.</summary>
public enum PulseSurveyPlanStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>A pulse survey plan as the pulse survey team keeps it.</summary>
public sealed class PulseSurveyPlan(string code, string label, int updatedAt, double rating, string name, int quantity, double cost, PulseSurveyPlanStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public int UpdatedAt { get; set; } = updatedAt;
    public double Rating { get; set; } = rating;
    public string Name { get; set; } = name;
    public int Quantity { get; set; } = quantity;
    public double Cost { get; set; } = cost;
    public PulseSurveyPlanStatus Status { get; set; } = status;

    /// <summary>Starts a pulse survey plan in status open.</summary>
    public static PulseSurveyPlan NewPulseSurveyPlan(string code, string label)
    {
        return new PulseSurveyPlan(code, label, 0, 0.0, "", 0, 0.0, PulseSurveyPlanStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PulseSurveyPlanStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PulseSurveyPlanStatus next)
    {
        switch (Status)
        {
            case PulseSurveyPlanStatus.Open:
                return next == PulseSurveyPlanStatus.Reopened;
            case PulseSurveyPlanStatus.InReview:
                return next == PulseSurveyPlanStatus.Reopened || next == PulseSurveyPlanStatus.Archived;
            case PulseSurveyPlanStatus.Resolved:
                return next == PulseSurveyPlanStatus.Reopened || next == PulseSurveyPlanStatus.Archived;
            case PulseSurveyPlanStatus.Reopened:
                return next == PulseSurveyPlanStatus.Archived;
        }
        return false;
    }

    public void MoveTo(PulseSurveyPlanStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"pulse survey plan {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        UpdatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{Code}: {Label}";
    }

    public double AdjustedValue()
    {
        return Math.Round((double)Quantity * Cost, 2);
    }

    public PulseSurveyPlan Copy()
    {
        return new PulseSurveyPlan(Code, Label, UpdatedAt, Rating, Name, Quantity, Cost, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
