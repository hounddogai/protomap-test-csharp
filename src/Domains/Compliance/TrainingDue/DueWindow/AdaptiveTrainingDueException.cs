namespace Acme.Workforce.Compliance.TrainingDue.DueWindow;

using Acme.Workforce.Kit.Text;

/// <summary>Where a adaptive training due exception is in its life cycle.</summary>
public enum AdaptiveTrainingDueExceptionStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>One adaptive training due exception, with what the planners need to act on it.</summary>
public sealed class AdaptiveTrainingDueException(string code, string name, int createdAt, double cost, double maximum, Dictionary<string, string> attributes, bool archived, string contactEmail, AdaptiveTrainingDueExceptionStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public int CreatedAt { get; set; } = createdAt;
    public double Cost { get; set; } = cost;
    public double Maximum { get; set; } = maximum;
    public Dictionary<string, string> Attributes { get; set; } = attributes;
    public bool Archived { get; set; } = archived;
    public string ContactEmail { get; set; } = contactEmail;
    public AdaptiveTrainingDueExceptionStatus Status { get; set; } = status;

    /// <summary>Starts a adaptive training due exception in status planned.</summary>
    public static AdaptiveTrainingDueException NewAdaptiveTrainingDueException(string code, string name)
    {
        return new AdaptiveTrainingDueException(
            code,
            name,
            0,
            0.0,
            0.0,
            new Dictionary<string, string>(),
            false,
            "",
            AdaptiveTrainingDueExceptionStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case AdaptiveTrainingDueExceptionStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(AdaptiveTrainingDueExceptionStatus next)
    {
        switch (Status)
        {
            case AdaptiveTrainingDueExceptionStatus.Planned:
                return next == AdaptiveTrainingDueExceptionStatus.Scheduled || next == AdaptiveTrainingDueExceptionStatus.InProgress;
            case AdaptiveTrainingDueExceptionStatus.Scheduled:
                switch (next)
                {
                    case AdaptiveTrainingDueExceptionStatus.InProgress:
                        return true;
                }
                return false;
            case AdaptiveTrainingDueExceptionStatus.InProgress:
                return next == AdaptiveTrainingDueExceptionStatus.Completed || next == AdaptiveTrainingDueExceptionStatus.Cancelled;
            case AdaptiveTrainingDueExceptionStatus.Completed:
                switch (next)
                {
                    case AdaptiveTrainingDueExceptionStatus.Cancelled:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(AdaptiveTrainingDueExceptionStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"adaptive training due exception {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        CreatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{"adaptive training due exception"} {Code}";
    }

    public string Attribute(string key, string fallback)
    {
        if (!Attributes.TryGetValue(key, out var found))
        {
            return fallback;
        }
        if (found.Trim() == "")
        {
            return fallback;
        }
        return found;
    }

    public double ProjectedValue()
    {
        return Math.Round(Cost + Maximum, 2);
    }

    public AdaptiveTrainingDueException Copy()
    {
        return new AdaptiveTrainingDueException(
            Code,
            Name,
            CreatedAt,
            Cost,
            Maximum,
            Attributes,
            Archived,
            ContactEmail,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
