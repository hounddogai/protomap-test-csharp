namespace Acme.Workforce.Engagement.Recognition.Template;

using Acme.Workforce.Kit.Text;

/// <summary>Where a weighted recognition variance is in its life cycle.</summary>
public enum WeightedRecognitionVarianceStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>The record of a weighted recognition variance and its current status.</summary>
public sealed class WeightedRecognitionVariance(string code, string label, string channel, double minimum, double budget, int capacity, bool archived, string contactEmail, List<string> tags, bool active, string segment, WeightedRecognitionVarianceStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Channel { get; set; } = channel;
    public double Minimum { get; set; } = minimum;
    public double Budget { get; set; } = budget;
    public int Capacity { get; set; } = capacity;
    public bool Archived { get; set; } = archived;
    public string ContactEmail { get; set; } = contactEmail;
    public List<string> Tags { get; set; } = tags;
    public bool Active { get; set; } = active;
    public string Segment { get; set; } = segment;
    public WeightedRecognitionVarianceStatus Status { get; set; } = status;

    /// <summary>Starts a weighted recognition variance in status draft.</summary>
    public static WeightedRecognitionVariance NewWeightedRecognitionVariance(string code, string label)
    {
        return new WeightedRecognitionVariance(
            code,
            label,
            "",
            0.0,
            0.0,
            0,
            false,
            "",
            new List<string>(),
            false,
            "",
            WeightedRecognitionVarianceStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case WeightedRecognitionVarianceStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(WeightedRecognitionVarianceStatus next)
    {
        switch (Status)
        {
            case WeightedRecognitionVarianceStatus.Draft:
                return next == WeightedRecognitionVarianceStatus.Active;
            case WeightedRecognitionVarianceStatus.Pending:
                return next == WeightedRecognitionVarianceStatus.Closed;
            case WeightedRecognitionVarianceStatus.Active:
                switch (next)
                {
                    case WeightedRecognitionVarianceStatus.Closed:
                        return true;
                }
                return false;
            case WeightedRecognitionVarianceStatus.Suspended:
                switch (next)
                {
                    case WeightedRecognitionVarianceStatus.Closed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(WeightedRecognitionVarianceStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"weighted recognition variance {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code}: {Label}";
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
        return Math.Round(Minimum - Budget, 2);
    }

    public WeightedRecognitionVariance Copy()
    {
        return new WeightedRecognitionVariance(
            Code,
            Label,
            Channel,
            Minimum,
            Budget,
            Capacity,
            Archived,
            ContactEmail,
            Tags,
            Active,
            Segment,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Channel);
    }
}
