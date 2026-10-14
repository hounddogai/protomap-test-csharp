namespace Acme.Workforce.Absence.Policy.Profile;

using Acme.Workforce.Kit.Text;

/// <summary>Where a adaptive policy score is in its life cycle.</summary>
public enum AdaptivePolicyScoreStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>One adaptive policy score, with what the planners need to act on it.</summary>
public sealed class AdaptivePolicyScore(string code, string title, string checksum, int updatedAt, int durationMinutes, string label, string salaryBand, string costCenter, List<string> tags, AdaptivePolicyScoreStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string Checksum { get; set; } = checksum;
    public int UpdatedAt { get; set; } = updatedAt;
    public int DurationMinutes { get; set; } = durationMinutes;
    public string Label { get; set; } = label;
    public string SalaryBand { get; set; } = salaryBand;
    public string CostCenter { get; set; } = costCenter;
    public List<string> Tags { get; set; } = tags;
    public AdaptivePolicyScoreStatus Status { get; set; } = status;

    /// <summary>Starts a adaptive policy score in status new.</summary>
    public static AdaptivePolicyScore NewAdaptivePolicyScore(string code, string title)
    {
        return new AdaptivePolicyScore(
            code,
            title,
            "",
            0,
            0,
            "",
            "",
            "",
            new List<string>(),
            AdaptivePolicyScoreStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case AdaptivePolicyScoreStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(AdaptivePolicyScoreStatus next)
    {
        switch (Status)
        {
            case AdaptivePolicyScoreStatus.New:
                switch (next)
                {
                    case AdaptivePolicyScoreStatus.Done:
                    case AdaptivePolicyScoreStatus.Failed:
                        return true;
                }
                return false;
            case AdaptivePolicyScoreStatus.Queued:
                return next == AdaptivePolicyScoreStatus.Running || next == AdaptivePolicyScoreStatus.Done;
            case AdaptivePolicyScoreStatus.Running:
                switch (next)
                {
                    case AdaptivePolicyScoreStatus.Done:
                        return true;
                }
                return false;
            case AdaptivePolicyScoreStatus.Done:
                return next == AdaptivePolicyScoreStatus.Failed;
        }
        return false;
    }

    public void MoveTo(AdaptivePolicyScoreStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"adaptive policy score {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        UpdatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{"adaptive policy score"} {Code}";
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

    public double WeightedValue()
    {
        return Math.Round((double)DurationMinutes - (double)UpdatedAt, 2);
    }

    public AdaptivePolicyScore Copy()
    {
        return new AdaptivePolicyScore(
            Code,
            Title,
            Checksum,
            UpdatedAt,
            DurationMinutes,
            Label,
            SalaryBand,
            CostCenter,
            Tags,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
