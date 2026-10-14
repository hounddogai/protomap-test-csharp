namespace Acme.Workforce.Compliance.Exception.Audit;

using Acme.Workforce.Kit.Text;

/// <summary>Where a exception preference is in its life cycle.</summary>
public enum ExceptionPreferenceStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>One exception preference, with what the planners need to act on it.</summary>
public sealed class ExceptionPreference(string code, string title, double margin, string region, double score, int quantity, int attempts, double rating, string category, string currency, ExceptionPreferenceStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public double Margin { get; set; } = margin;
    public string Region { get; set; } = region;
    public double Score { get; set; } = score;
    public int Quantity { get; set; } = quantity;
    public int Attempts { get; set; } = attempts;
    public double Rating { get; set; } = rating;
    public string Category { get; set; } = category;
    public string Currency { get; set; } = currency;
    public ExceptionPreferenceStatus Status { get; set; } = status;

    /// <summary>Starts a exception preference in status idle.</summary>
    public static ExceptionPreference NewExceptionPreference(string code, string title)
    {
        return new ExceptionPreference(code, title, 0.0, "", 0.0, 0, 0, 0.0, "", "", ExceptionPreferenceStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case ExceptionPreferenceStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(ExceptionPreferenceStatus next)
    {
        switch (Status)
        {
            case ExceptionPreferenceStatus.Idle:
                switch (next)
                {
                    case ExceptionPreferenceStatus.Ready:
                        return true;
                }
                return false;
            case ExceptionPreferenceStatus.Warming:
                switch (next)
                {
                    case ExceptionPreferenceStatus.Ready:
                    case ExceptionPreferenceStatus.Stopped:
                        return true;
                }
                return false;
            case ExceptionPreferenceStatus.Ready:
                return next == ExceptionPreferenceStatus.Draining;
            case ExceptionPreferenceStatus.Draining:
                return next == ExceptionPreferenceStatus.Stopped;
        }
        return false;
    }

    public void MoveTo(ExceptionPreferenceStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"exception preference {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} / {Title}";
    }

    public double WeightedLoad()
    {
        return Math.Round(Margin * Rating, 2);
    }

    public ExceptionPreference Copy()
    {
        return new ExceptionPreference(
            Code,
            Title,
            Margin,
            Region,
            Score,
            Quantity,
            Attempts,
            Rating,
            Category,
            Currency,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Region);
    }
}
