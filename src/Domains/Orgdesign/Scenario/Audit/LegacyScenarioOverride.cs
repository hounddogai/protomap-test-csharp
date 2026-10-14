namespace Acme.Workforce.Orgdesign.Scenario.Audit;

using Acme.Workforce.Kit.Text;

/// <summary>Where a legacy scenario override is in its life cycle.</summary>
public enum LegacyScenarioOverrideStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>One legacy scenario override, with what the planners need to act on it.</summary>
public sealed class LegacyScenarioOverride(string code, string title, string contactEmail, string notes, double fte, LegacyScenarioOverrideStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string ContactEmail { get; set; } = contactEmail;
    public string Notes { get; set; } = notes;
    public double Fte { get; set; } = fte;
    public LegacyScenarioOverrideStatus Status { get; set; } = status;

    /// <summary>Starts a legacy scenario override in status open.</summary>
    public static LegacyScenarioOverride NewLegacyScenarioOverride(string code, string title)
    {
        return new LegacyScenarioOverride(code, title, "", "", 0.0, LegacyScenarioOverrideStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case LegacyScenarioOverrideStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(LegacyScenarioOverrideStatus next)
    {
        switch (Status)
        {
            case LegacyScenarioOverrideStatus.Open:
                return next == LegacyScenarioOverrideStatus.Archived;
            case LegacyScenarioOverrideStatus.InReview:
                return next == LegacyScenarioOverrideStatus.Resolved || next == LegacyScenarioOverrideStatus.Archived;
            case LegacyScenarioOverrideStatus.Resolved:
                switch (next)
                {
                    case LegacyScenarioOverrideStatus.Archived:
                        return true;
                }
                return false;
            case LegacyScenarioOverrideStatus.Reopened:
                return next == LegacyScenarioOverrideStatus.Archived;
        }
        return false;
    }

    public void MoveTo(LegacyScenarioOverrideStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"legacy scenario override {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} / {Title}";
    }

    public double MeasuredValue()
    {
        return Fte;
    }

    public LegacyScenarioOverride Copy()
    {
        return new LegacyScenarioOverride(Code, Title, ContactEmail, Notes, Fte, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
