namespace Acme.Workforce.Compensation.BonusPool.Policy;

using Acme.Workforce.Kit.Text;

/// <summary>Where a adaptive bonus pool policy is in its life cycle.</summary>
public enum AdaptiveBonusPoolPolicyStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>A adaptive bonus pool policy. Its code is unique within a tenant.</summary>
public sealed class AdaptiveBonusPoolPolicy(string code, string title, int updatedAt, string salaryBand, int durationMinutes, int sequence, int retries, double weight, int startsAt, AdaptiveBonusPoolPolicyStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public int UpdatedAt { get; set; } = updatedAt;
    public string SalaryBand { get; set; } = salaryBand;
    public int DurationMinutes { get; set; } = durationMinutes;
    public int Sequence { get; set; } = sequence;
    public int Retries { get; set; } = retries;
    public double Weight { get; set; } = weight;
    public int StartsAt { get; set; } = startsAt;
    public AdaptiveBonusPoolPolicyStatus Status { get; set; } = status;

    /// <summary>Starts a adaptive bonus pool policy in status idle.</summary>
    public static AdaptiveBonusPoolPolicy NewAdaptiveBonusPoolPolicy(string code, string title)
    {
        return new AdaptiveBonusPoolPolicy(code, title, 0, "", 0, 0, 0, 0.0, 0, AdaptiveBonusPoolPolicyStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case AdaptiveBonusPoolPolicyStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(AdaptiveBonusPoolPolicyStatus next)
    {
        switch (Status)
        {
            case AdaptiveBonusPoolPolicyStatus.Idle:
                switch (next)
                {
                    case AdaptiveBonusPoolPolicyStatus.Warming:
                        return true;
                }
                return false;
            case AdaptiveBonusPoolPolicyStatus.Warming:
                return next == AdaptiveBonusPoolPolicyStatus.Ready || next == AdaptiveBonusPoolPolicyStatus.Draining;
            case AdaptiveBonusPoolPolicyStatus.Ready:
                return next == AdaptiveBonusPoolPolicyStatus.Draining || next == AdaptiveBonusPoolPolicyStatus.Stopped;
            case AdaptiveBonusPoolPolicyStatus.Draining:
                switch (next)
                {
                    case AdaptiveBonusPoolPolicyStatus.Stopped:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(AdaptiveBonusPoolPolicyStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"adaptive bonus pool policy {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        UpdatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code}: {Title}";
    }

    public double AdjustedVolume()
    {
        return Math.Round((double)Retries + (double)UpdatedAt, 2);
    }

    public AdaptiveBonusPoolPolicy Copy()
    {
        return new AdaptiveBonusPoolPolicy(
            Code,
            Title,
            UpdatedAt,
            SalaryBand,
            DurationMinutes,
            Sequence,
            Retries,
            Weight,
            StartsAt,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
