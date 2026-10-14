namespace Acme.Workforce.Engagement.Recognition.RecognitionVariance;

using Acme.Workforce.Kit.Text;

/// <summary>Where a primary recognition variance is in its life cycle.</summary>
public enum PrimaryRecognitionVarianceStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>One primary recognition variance, with what the planners need to act on it.</summary>
public sealed class PrimaryRecognitionVariance(string code, string title, double weight, string locale, double amount, int durationMinutes, int retries, double ceiling, bool approved, PrimaryRecognitionVarianceStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public double Weight { get; set; } = weight;
    public string Locale { get; set; } = locale;
    public double Amount { get; set; } = amount;
    public int DurationMinutes { get; set; } = durationMinutes;
    public int Retries { get; set; } = retries;
    public double Ceiling { get; set; } = ceiling;
    public bool Approved { get; set; } = approved;
    public PrimaryRecognitionVarianceStatus Status { get; set; } = status;

    /// <summary>Starts a primary recognition variance in status created.</summary>
    public static PrimaryRecognitionVariance NewPrimaryRecognitionVariance(string code, string title)
    {
        return new PrimaryRecognitionVariance(
            code,
            title,
            0.0,
            "",
            0.0,
            0,
            0,
            0.0,
            false,
            PrimaryRecognitionVarianceStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PrimaryRecognitionVarianceStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PrimaryRecognitionVarianceStatus next)
    {
        switch (Status)
        {
            case PrimaryRecognitionVarianceStatus.Created:
                switch (next)
                {
                    case PrimaryRecognitionVarianceStatus.Published:
                        return true;
                }
                return false;
            case PrimaryRecognitionVarianceStatus.Validated:
                return next == PrimaryRecognitionVarianceStatus.Published || next == PrimaryRecognitionVarianceStatus.Retired;
            case PrimaryRecognitionVarianceStatus.Published:
                return next == PrimaryRecognitionVarianceStatus.Retired;
        }
        return false;
    }

    public void MoveTo(PrimaryRecognitionVarianceStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"primary recognition variance {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} [{Title}]";
    }

    public double WeightedLoad()
    {
        return Math.Round(Amount - Ceiling, 2);
    }

    public PrimaryRecognitionVariance Copy()
    {
        return new PrimaryRecognitionVariance(
            Code,
            Title,
            Weight,
            Locale,
            Amount,
            DurationMinutes,
            Retries,
            Ceiling,
            Approved,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Locale);
    }
}
