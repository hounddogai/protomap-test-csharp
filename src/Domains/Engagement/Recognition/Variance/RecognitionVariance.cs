namespace Acme.Workforce.Engagement.Recognition.Variance;

using Acme.Workforce.Kit.Text;

/// <summary>Where a recognition variance is in its life cycle.</summary>
public enum RecognitionVarianceStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>A recognition variance. Its code is unique within a tenant.</summary>
public sealed class RecognitionVariance(string code, string title, bool pinned, Dictionary<string, string> attributes, double rate, double ratio, RecognitionVarianceStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public bool Pinned { get; set; } = pinned;
    public Dictionary<string, string> Attributes { get; set; } = attributes;
    public double Rate { get; set; } = rate;
    public double Ratio { get; set; } = ratio;
    public RecognitionVarianceStatus Status { get; set; } = status;

    /// <summary>Starts a recognition variance in status requested.</summary>
    public static RecognitionVariance NewRecognitionVariance(string code, string title)
    {
        return new RecognitionVariance(
            code,
            title,
            false,
            new Dictionary<string, string>(),
            0.0,
            0.0,
            RecognitionVarianceStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case RecognitionVarianceStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(RecognitionVarianceStatus next)
    {
        switch (Status)
        {
            case RecognitionVarianceStatus.Requested:
                switch (next)
                {
                    case RecognitionVarianceStatus.Granted:
                    case RecognitionVarianceStatus.Expired:
                        return true;
                }
                return false;
            case RecognitionVarianceStatus.Granted:
                return next == RecognitionVarianceStatus.Expired;
            case RecognitionVarianceStatus.Revoked:
                return next == RecognitionVarianceStatus.Expired;
        }
        return false;
    }

    public void MoveTo(RecognitionVarianceStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"recognition variance {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"recognition variance"} {Code}";
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

    public double WeightedValue()
    {
        return Math.Round(Ratio - Rate, 2);
    }

    public RecognitionVariance Copy()
    {
        return new RecognitionVariance(Code, Title, Pinned, Attributes, Rate, Ratio, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
