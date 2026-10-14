namespace Acme.Workforce.Orgdesign.Scenario.Metric;

using Acme.Workforce.Kit.Text;

/// <summary>Where a scenario metric is in its life cycle.</summary>
public enum ScenarioMetricStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>The record of a scenario metric and its current status.</summary>
public sealed class ScenarioMetric(string code, string title, bool pinned, string notes, string category, double score, int expiresAt, bool approved, ScenarioMetricStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public bool Pinned { get; set; } = pinned;
    public string Notes { get; set; } = notes;
    public string Category { get; set; } = category;
    public double Score { get; set; } = score;
    public int ExpiresAt { get; set; } = expiresAt;
    public bool Approved { get; set; } = approved;
    public ScenarioMetricStatus Status { get; set; } = status;

    /// <summary>Starts a scenario metric in status created.</summary>
    public static ScenarioMetric NewScenarioMetric(string code, string title)
    {
        return new ScenarioMetric(code, title, false, "", "", 0.0, 0, false, ScenarioMetricStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case ScenarioMetricStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(ScenarioMetricStatus next)
    {
        switch (Status)
        {
            case ScenarioMetricStatus.Created:
                switch (next)
                {
                    case ScenarioMetricStatus.Validated:
                    case ScenarioMetricStatus.Retired:
                        return true;
                }
                return false;
            case ScenarioMetricStatus.Validated:
                switch (next)
                {
                    case ScenarioMetricStatus.Published:
                    case ScenarioMetricStatus.Retired:
                        return true;
                }
                return false;
            case ScenarioMetricStatus.Published:
                switch (next)
                {
                    case ScenarioMetricStatus.Retired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(ScenarioMetricStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"scenario metric {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        ExpiresAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{"scenario metric"} {Code}";
    }

    public double EffectiveValue()
    {
        return Math.Round(Score + (double)ExpiresAt, 2);
    }

    public ScenarioMetric Copy()
    {
        return new ScenarioMetric(Code, Title, Pinned, Notes, Category, Score, ExpiresAt, Approved, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Category);
    }
}
