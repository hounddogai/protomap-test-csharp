namespace Acme.Workforce.Orgdesign.Scenario.Draft;

using Acme.Workforce.Kit.Text;

/// <summary>Where a scenario batch is in its life cycle.</summary>
public enum ScenarioBatchStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>One scenario batch, with what the planners need to act on it.</summary>
public sealed class ScenarioBatch(string code, string label, double rate, bool visible, string title, double maximum, int attempts, bool active, double margin, int limit, string salaryBand, ScenarioBatchStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public double Rate { get; set; } = rate;
    public bool Visible { get; set; } = visible;
    public string Title { get; set; } = title;
    public double Maximum { get; set; } = maximum;
    public int Attempts { get; set; } = attempts;
    public bool Active { get; set; } = active;
    public double Margin { get; set; } = margin;
    public int Limit { get; set; } = limit;
    public string SalaryBand { get; set; } = salaryBand;
    public ScenarioBatchStatus Status { get; set; } = status;

    /// <summary>Starts a scenario batch in status open.</summary>
    public static ScenarioBatch NewScenarioBatch(string code, string label)
    {
        return new ScenarioBatch(code, label, 0.0, false, "", 0.0, 0, false, 0.0, 0, "", ScenarioBatchStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case ScenarioBatchStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(ScenarioBatchStatus next)
    {
        switch (Status)
        {
            case ScenarioBatchStatus.Open:
                switch (next)
                {
                    case ScenarioBatchStatus.InReview:
                    case ScenarioBatchStatus.Archived:
                        return true;
                }
                return false;
            case ScenarioBatchStatus.InReview:
                switch (next)
                {
                    case ScenarioBatchStatus.Resolved:
                    case ScenarioBatchStatus.Reopened:
                        return true;
                }
                return false;
            case ScenarioBatchStatus.Resolved:
                return next == ScenarioBatchStatus.Reopened || next == ScenarioBatchStatus.Archived;
            case ScenarioBatchStatus.Reopened:
                switch (next)
                {
                    case ScenarioBatchStatus.Archived:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(ScenarioBatchStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"scenario batch {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{"scenario batch"} {Code}";
    }

    public double NetTotal()
    {
        return Math.Round(Rate + (double)Attempts, 2);
    }

    public ScenarioBatch Copy()
    {
        return new ScenarioBatch(
            Code,
            Label,
            Rate,
            Visible,
            Title,
            Maximum,
            Attempts,
            Active,
            Margin,
            Limit,
            SalaryBand,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
