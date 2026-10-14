namespace Acme.Workforce.Compliance.TrainingDue.Metric;

using Acme.Workforce.Kit.Text;

/// <summary>Where a seasonal training due metric is in its life cycle.</summary>
public enum SeasonalTrainingDueMetricStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>A seasonal training due metric. Its code is unique within a tenant.</summary>
public sealed class SeasonalTrainingDueMetric(string code, string title, bool active, string skill, string managerId, int quantity, SeasonalTrainingDueMetricStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public bool Active { get; set; } = active;
    public string Skill { get; set; } = skill;
    public string ManagerId { get; set; } = managerId;
    public int Quantity { get; set; } = quantity;
    public SeasonalTrainingDueMetricStatus Status { get; set; } = status;

    /// <summary>Starts a seasonal training due metric in status draft.</summary>
    public static SeasonalTrainingDueMetric NewSeasonalTrainingDueMetric(string code, string title)
    {
        return new SeasonalTrainingDueMetric(code, title, false, "", "", 0, SeasonalTrainingDueMetricStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case SeasonalTrainingDueMetricStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(SeasonalTrainingDueMetricStatus next)
    {
        switch (Status)
        {
            case SeasonalTrainingDueMetricStatus.Draft:
                switch (next)
                {
                    case SeasonalTrainingDueMetricStatus.Active:
                        return true;
                }
                return false;
            case SeasonalTrainingDueMetricStatus.Pending:
                switch (next)
                {
                    case SeasonalTrainingDueMetricStatus.Active:
                    case SeasonalTrainingDueMetricStatus.Suspended:
                        return true;
                }
                return false;
            case SeasonalTrainingDueMetricStatus.Active:
                switch (next)
                {
                    case SeasonalTrainingDueMetricStatus.Suspended:
                    case SeasonalTrainingDueMetricStatus.Closed:
                        return true;
                }
                return false;
            case SeasonalTrainingDueMetricStatus.Suspended:
                switch (next)
                {
                    case SeasonalTrainingDueMetricStatus.Closed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(SeasonalTrainingDueMetricStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"seasonal training due metric {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code}: {Title}";
    }

    public double MeasuredValue()
    {
        return (double)Quantity;
    }

    public SeasonalTrainingDueMetric Copy()
    {
        return new SeasonalTrainingDueMetric(Code, Title, Active, Skill, ManagerId, Quantity, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
