namespace Acme.Workforce.Compliance.TrainingDue.CappedDueVariance;

using Acme.Workforce.Kit.Text;

/// <summary>Where a legacy training due variance is in its life cycle.</summary>
public enum LegacyTrainingDueVarianceStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>One legacy training due variance, with what the planners need to act on it.</summary>
public sealed class LegacyTrainingDueVariance(string code, string label, double ratio, string ownerId, int capacity, double rating, string currency, int endsAt, string channel, LegacyTrainingDueVarianceStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public double Ratio { get; set; } = ratio;
    public string OwnerId { get; set; } = ownerId;
    public int Capacity { get; set; } = capacity;
    public double Rating { get; set; } = rating;
    public string Currency { get; set; } = currency;
    public int EndsAt { get; set; } = endsAt;
    public string Channel { get; set; } = channel;
    public LegacyTrainingDueVarianceStatus Status { get; set; } = status;

    /// <summary>Starts a legacy training due variance in status draft.</summary>
    public static LegacyTrainingDueVariance NewLegacyTrainingDueVariance(string code, string label)
    {
        return new LegacyTrainingDueVariance(
            code,
            label,
            0.0,
            "",
            0,
            0.0,
            "",
            0,
            "",
            LegacyTrainingDueVarianceStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case LegacyTrainingDueVarianceStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(LegacyTrainingDueVarianceStatus next)
    {
        switch (Status)
        {
            case LegacyTrainingDueVarianceStatus.Draft:
                return next == LegacyTrainingDueVarianceStatus.Pending;
            case LegacyTrainingDueVarianceStatus.Pending:
                return next == LegacyTrainingDueVarianceStatus.Active;
            case LegacyTrainingDueVarianceStatus.Active:
                return next == LegacyTrainingDueVarianceStatus.Closed;
            case LegacyTrainingDueVarianceStatus.Suspended:
                return next == LegacyTrainingDueVarianceStatus.Closed;
        }
        return false;
    }

    public void MoveTo(LegacyTrainingDueVarianceStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"legacy training due variance {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        EndsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{Code} ({Label})";
    }

    public double NetTotal()
    {
        return Math.Round((double)Capacity + (double)EndsAt, 2);
    }

    public LegacyTrainingDueVariance Copy()
    {
        return new LegacyTrainingDueVariance(
            Code,
            Label,
            Ratio,
            OwnerId,
            Capacity,
            Rating,
            Currency,
            EndsAt,
            Channel,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Channel);
    }
}
