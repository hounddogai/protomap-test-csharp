namespace Acme.Workforce.Compensation.BonusPool.Variance;

using Acme.Workforce.Kit.Text;

/// <summary>Where a strict bonus pool variance is in its life cycle.</summary>
public enum StrictBonusPoolVarianceStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>A strict bonus pool variance. Its code is unique within a tenant.</summary>
public sealed class StrictBonusPoolVariance(string code, string label, string managerId, double rating, int capacity, string currency, string jobCode, List<string> tags, int updatedAt, double ratio, double score, StrictBonusPoolVarianceStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string ManagerId { get; set; } = managerId;
    public double Rating { get; set; } = rating;
    public int Capacity { get; set; } = capacity;
    public string Currency { get; set; } = currency;
    public string JobCode { get; set; } = jobCode;
    public List<string> Tags { get; set; } = tags;
    public int UpdatedAt { get; set; } = updatedAt;
    public double Ratio { get; set; } = ratio;
    public double Score { get; set; } = score;
    public StrictBonusPoolVarianceStatus Status { get; set; } = status;

    /// <summary>Starts a strict bonus pool variance in status proposed.</summary>
    public static StrictBonusPoolVariance NewStrictBonusPoolVariance(string code, string label)
    {
        return new StrictBonusPoolVariance(
            code,
            label,
            "",
            0.0,
            0,
            "",
            "",
            new List<string>(),
            0,
            0.0,
            0.0,
            StrictBonusPoolVarianceStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case StrictBonusPoolVarianceStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(StrictBonusPoolVarianceStatus next)
    {
        switch (Status)
        {
            case StrictBonusPoolVarianceStatus.Proposed:
                return next == StrictBonusPoolVarianceStatus.Approved || next == StrictBonusPoolVarianceStatus.Withdrawn;
            case StrictBonusPoolVarianceStatus.Approved:
                switch (next)
                {
                    case StrictBonusPoolVarianceStatus.Rejected:
                        return true;
                }
                return false;
            case StrictBonusPoolVarianceStatus.Rejected:
                switch (next)
                {
                    case StrictBonusPoolVarianceStatus.Withdrawn:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(StrictBonusPoolVarianceStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"strict bonus pool variance {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        UpdatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{Code} [{Label}]";
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

    public double NetTotal()
    {
        return Math.Round((double)UpdatedAt + Rating, 2);
    }

    public StrictBonusPoolVariance Copy()
    {
        return new StrictBonusPoolVariance(
            Code,
            Label,
            ManagerId,
            Rating,
            Capacity,
            Currency,
            JobCode,
            Tags,
            UpdatedAt,
            Ratio,
            Score,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
