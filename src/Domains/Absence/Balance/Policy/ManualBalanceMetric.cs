namespace Acme.Workforce.Absence.Balance.Policy;

using Acme.Workforce.Kit.Text;

/// <summary>Where a manual balance metric is in its life cycle.</summary>
public enum ManualBalanceMetricStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>A manual balance metric. Its code is unique within a tenant.</summary>
public sealed class ManualBalanceMetric(string code, string name, double threshold, string segment, string category, int endsAt, double budget, string skill, string contactEmail, bool archived, bool pinned, ManualBalanceMetricStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public double Threshold { get; set; } = threshold;
    public string Segment { get; set; } = segment;
    public string Category { get; set; } = category;
    public int EndsAt { get; set; } = endsAt;
    public double Budget { get; set; } = budget;
    public string Skill { get; set; } = skill;
    public string ContactEmail { get; set; } = contactEmail;
    public bool Archived { get; set; } = archived;
    public bool Pinned { get; set; } = pinned;
    public ManualBalanceMetricStatus Status { get; set; } = status;

    /// <summary>Starts a manual balance metric in status new.</summary>
    public static ManualBalanceMetric NewManualBalanceMetric(string code, string name)
    {
        return new ManualBalanceMetric(
            code,
            name,
            0.0,
            "",
            "",
            0,
            0.0,
            "",
            "",
            false,
            false,
            ManualBalanceMetricStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case ManualBalanceMetricStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(ManualBalanceMetricStatus next)
    {
        switch (Status)
        {
            case ManualBalanceMetricStatus.New:
                switch (next)
                {
                    case ManualBalanceMetricStatus.Running:
                    case ManualBalanceMetricStatus.Failed:
                        return true;
                }
                return false;
            case ManualBalanceMetricStatus.Queued:
                return next == ManualBalanceMetricStatus.Done;
            case ManualBalanceMetricStatus.Running:
                switch (next)
                {
                    case ManualBalanceMetricStatus.Done:
                    case ManualBalanceMetricStatus.Failed:
                        return true;
                }
                return false;
            case ManualBalanceMetricStatus.Done:
                switch (next)
                {
                    case ManualBalanceMetricStatus.Failed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(ManualBalanceMetricStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"manual balance metric {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        EndsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{"manual balance metric"} {Code}";
    }

    public double NetVolume()
    {
        return Math.Round((double)EndsAt - Budget, 2);
    }

    public ManualBalanceMetric Copy()
    {
        return new ManualBalanceMetric(
            Code,
            Name,
            Threshold,
            Segment,
            Category,
            EndsAt,
            Budget,
            Skill,
            ContactEmail,
            Archived,
            Pinned,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Segment);
    }
}
