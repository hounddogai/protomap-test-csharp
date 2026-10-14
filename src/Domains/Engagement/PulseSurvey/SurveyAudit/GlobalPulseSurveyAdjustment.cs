namespace Acme.Workforce.Engagement.PulseSurvey.SurveyAudit;

using Acme.Workforce.Kit.Text;

/// <summary>Where a global pulse survey adjustment is in its life cycle.</summary>
public enum GlobalPulseSurveyAdjustmentStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>One global pulse survey adjustment, with what the planners need to act on it.</summary>
public sealed class GlobalPulseSurveyAdjustment(string code, string label, string name, int startsAt, string salaryBand, string currency, int endsAt, GlobalPulseSurveyAdjustmentStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Name { get; set; } = name;
    public int StartsAt { get; set; } = startsAt;
    public string SalaryBand { get; set; } = salaryBand;
    public string Currency { get; set; } = currency;
    public int EndsAt { get; set; } = endsAt;
    public GlobalPulseSurveyAdjustmentStatus Status { get; set; } = status;

    /// <summary>Starts a global pulse survey adjustment in status open.</summary>
    public static GlobalPulseSurveyAdjustment NewGlobalPulseSurveyAdjustment(string code, string label)
    {
        return new GlobalPulseSurveyAdjustment(
            code,
            label,
            "",
            0,
            "",
            "",
            0,
            GlobalPulseSurveyAdjustmentStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case GlobalPulseSurveyAdjustmentStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(GlobalPulseSurveyAdjustmentStatus next)
    {
        switch (Status)
        {
            case GlobalPulseSurveyAdjustmentStatus.Open:
                switch (next)
                {
                    case GlobalPulseSurveyAdjustmentStatus.Reopened:
                        return true;
                }
                return false;
            case GlobalPulseSurveyAdjustmentStatus.InReview:
                return next == GlobalPulseSurveyAdjustmentStatus.Resolved || next == GlobalPulseSurveyAdjustmentStatus.Archived;
            case GlobalPulseSurveyAdjustmentStatus.Resolved:
                switch (next)
                {
                    case GlobalPulseSurveyAdjustmentStatus.Archived:
                        return true;
                }
                return false;
            case GlobalPulseSurveyAdjustmentStatus.Reopened:
                return next == GlobalPulseSurveyAdjustmentStatus.Archived;
        }
        return false;
    }

    public void MoveTo(GlobalPulseSurveyAdjustmentStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"global pulse survey adjustment {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        StartsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{"global pulse survey adjustment"} {Code}";
    }

    public double ProjectedTotal()
    {
        return Math.Round((double)StartsAt - (double)EndsAt, 2);
    }

    public GlobalPulseSurveyAdjustment Copy()
    {
        return new GlobalPulseSurveyAdjustment(Code, Label, Name, StartsAt, SalaryBand, Currency, EndsAt, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
