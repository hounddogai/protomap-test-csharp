namespace Acme.Workforce.Compensation.BonusPool.Window;

using Acme.Workforce.Kit.Text;

/// <summary>Where a bonus pool window is in its life cycle.</summary>
public enum BonusPoolWindowStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>One bonus pool window, with what the planners need to act on it.</summary>
public sealed class BonusPoolWindow(string code, string name, double ceiling, int updatedAt, double threshold, double weight, List<string> tags, string contactEmail, int tenureMonths, BonusPoolWindowStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public double Ceiling { get; set; } = ceiling;
    public int UpdatedAt { get; set; } = updatedAt;
    public double Threshold { get; set; } = threshold;
    public double Weight { get; set; } = weight;
    public List<string> Tags { get; set; } = tags;
    public string ContactEmail { get; set; } = contactEmail;
    public int TenureMonths { get; set; } = tenureMonths;
    public BonusPoolWindowStatus Status { get; set; } = status;

    /// <summary>Starts a bonus pool window in status requested.</summary>
    public static BonusPoolWindow NewBonusPoolWindow(string code, string name)
    {
        return new BonusPoolWindow(
            code,
            name,
            0.0,
            0,
            0.0,
            0.0,
            new List<string>(),
            "",
            0,
            BonusPoolWindowStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case BonusPoolWindowStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(BonusPoolWindowStatus next)
    {
        switch (Status)
        {
            case BonusPoolWindowStatus.Requested:
                switch (next)
                {
                    case BonusPoolWindowStatus.Revoked:
                    case BonusPoolWindowStatus.Expired:
                        return true;
                }
                return false;
            case BonusPoolWindowStatus.Granted:
                return next == BonusPoolWindowStatus.Revoked || next == BonusPoolWindowStatus.Expired;
            case BonusPoolWindowStatus.Revoked:
                return next == BonusPoolWindowStatus.Expired;
        }
        return false;
    }

    public void MoveTo(BonusPoolWindowStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"bonus pool window {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        UpdatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code} ({Name})";
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

    public double WeightedExposure()
    {
        return Math.Round(Threshold * Weight, 2);
    }

    public BonusPoolWindow Copy()
    {
        return new BonusPoolWindow(
            Code,
            Name,
            Ceiling,
            UpdatedAt,
            Threshold,
            Weight,
            Tags,
            ContactEmail,
            TenureMonths,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
