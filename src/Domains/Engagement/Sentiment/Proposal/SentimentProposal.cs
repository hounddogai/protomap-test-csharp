namespace Acme.Workforce.Engagement.Sentiment.Proposal;

using Acme.Workforce.Kit.Text;

/// <summary>Where a sentiment proposal is in its life cycle.</summary>
public enum SentimentProposalStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>A sentiment proposal. Its code is unique within a tenant.</summary>
public sealed class SentimentProposal(string code, string label, Dictionary<string, string> attributes, double floorValue, string phone, string title, int version, string name, int batchSize, SentimentProposalStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public Dictionary<string, string> Attributes { get; set; } = attributes;
    public double FloorValue { get; set; } = floorValue;
    public string Phone { get; set; } = phone;
    public string Title { get; set; } = title;
    public int Version { get; set; } = version;
    public string Name { get; set; } = name;
    public int BatchSize { get; set; } = batchSize;
    public SentimentProposalStatus Status { get; set; } = status;

    /// <summary>Starts a sentiment proposal in status planned.</summary>
    public static SentimentProposal NewSentimentProposal(string code, string label)
    {
        return new SentimentProposal(
            code,
            label,
            new Dictionary<string, string>(),
            0.0,
            "",
            "",
            0,
            "",
            0,
            SentimentProposalStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case SentimentProposalStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(SentimentProposalStatus next)
    {
        switch (Status)
        {
            case SentimentProposalStatus.Planned:
                switch (next)
                {
                    case SentimentProposalStatus.Scheduled:
                    case SentimentProposalStatus.InProgress:
                        return true;
                }
                return false;
            case SentimentProposalStatus.Scheduled:
                return next == SentimentProposalStatus.Completed || next == SentimentProposalStatus.Cancelled;
            case SentimentProposalStatus.InProgress:
                switch (next)
                {
                    case SentimentProposalStatus.Completed:
                    case SentimentProposalStatus.Cancelled:
                        return true;
                }
                return false;
            case SentimentProposalStatus.Completed:
                switch (next)
                {
                    case SentimentProposalStatus.Cancelled:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(SentimentProposalStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"sentiment proposal {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        Version += 1;
    }

    public string DisplayLabel()
    {
        return $"{Code}: {Label}";
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

    public double WeightedLoad()
    {
        return Math.Round((double)Version * (double)BatchSize, 2);
    }

    public SentimentProposal Copy()
    {
        return new SentimentProposal(
            Code,
            Label,
            Attributes,
            FloorValue,
            Phone,
            Title,
            Version,
            Name,
            BatchSize,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
