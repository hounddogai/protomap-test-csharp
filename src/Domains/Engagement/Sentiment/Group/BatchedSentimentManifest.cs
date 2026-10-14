namespace Acme.Workforce.Engagement.Sentiment.Group;

using Acme.Workforce.Kit.Text;

/// <summary>Where a batched sentiment manifest is in its life cycle.</summary>
public enum BatchedSentimentManifestStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>A batched sentiment manifest as the sentiment team keeps it.</summary>
public sealed class BatchedSentimentManifest(string code, string name, string summary, double discount, double weight, int endsAt, Dictionary<string, string> attributes, BatchedSentimentManifestStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string Summary { get; set; } = summary;
    public double Discount { get; set; } = discount;
    public double Weight { get; set; } = weight;
    public int EndsAt { get; set; } = endsAt;
    public Dictionary<string, string> Attributes { get; set; } = attributes;
    public BatchedSentimentManifestStatus Status { get; set; } = status;

    /// <summary>Starts a batched sentiment manifest in status requested.</summary>
    public static BatchedSentimentManifest NewBatchedSentimentManifest(string code, string name)
    {
        return new BatchedSentimentManifest(
            code,
            name,
            "",
            0.0,
            0.0,
            0,
            new Dictionary<string, string>(),
            BatchedSentimentManifestStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case BatchedSentimentManifestStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(BatchedSentimentManifestStatus next)
    {
        switch (Status)
        {
            case BatchedSentimentManifestStatus.Requested:
                return next == BatchedSentimentManifestStatus.Granted || next == BatchedSentimentManifestStatus.Revoked;
            case BatchedSentimentManifestStatus.Granted:
                switch (next)
                {
                    case BatchedSentimentManifestStatus.Expired:
                        return true;
                }
                return false;
            case BatchedSentimentManifestStatus.Revoked:
                return next == BatchedSentimentManifestStatus.Expired;
        }
        return false;
    }

    public void MoveTo(BatchedSentimentManifestStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"batched sentiment manifest {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        EndsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code}: {Name}";
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

    public double EffectiveTotal()
    {
        return Math.Round(Weight - (double)EndsAt, 2);
    }

    public BatchedSentimentManifest Copy()
    {
        return new BatchedSentimentManifest(Code, Name, Summary, Discount, Weight, EndsAt, Attributes, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
