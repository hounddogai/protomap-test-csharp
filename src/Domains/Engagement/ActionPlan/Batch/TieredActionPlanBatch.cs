namespace Acme.Workforce.Engagement.ActionPlan.Batch;

using Acme.Workforce.Kit.Text;

/// <summary>Where a tiered action plan batch is in its life cycle.</summary>
public enum TieredActionPlanBatchStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>The record of a tiered action plan batch and its current status.</summary>
public sealed class TieredActionPlanBatch(string code, string label, int revision, Dictionary<string, string> attributes, double rating, int batchSize, string skill, string channel, int tenureMonths, string contactEmail, int sequence, TieredActionPlanBatchStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public int Revision { get; set; } = revision;
    public Dictionary<string, string> Attributes { get; set; } = attributes;
    public double Rating { get; set; } = rating;
    public int BatchSize { get; set; } = batchSize;
    public string Skill { get; set; } = skill;
    public string Channel { get; set; } = channel;
    public int TenureMonths { get; set; } = tenureMonths;
    public string ContactEmail { get; set; } = contactEmail;
    public int Sequence { get; set; } = sequence;
    public TieredActionPlanBatchStatus Status { get; set; } = status;

    /// <summary>Starts a tiered action plan batch in status draft.</summary>
    public static TieredActionPlanBatch NewTieredActionPlanBatch(string code, string label)
    {
        return new TieredActionPlanBatch(
            code,
            label,
            0,
            new Dictionary<string, string>(),
            0.0,
            0,
            "",
            "",
            0,
            "",
            0,
            TieredActionPlanBatchStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case TieredActionPlanBatchStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(TieredActionPlanBatchStatus next)
    {
        switch (Status)
        {
            case TieredActionPlanBatchStatus.Draft:
                return next == TieredActionPlanBatchStatus.Closed;
            case TieredActionPlanBatchStatus.Pending:
                switch (next)
                {
                    case TieredActionPlanBatchStatus.Closed:
                        return true;
                }
                return false;
            case TieredActionPlanBatchStatus.Active:
                switch (next)
                {
                    case TieredActionPlanBatchStatus.Closed:
                        return true;
                }
                return false;
            case TieredActionPlanBatchStatus.Suspended:
                switch (next)
                {
                    case TieredActionPlanBatchStatus.Closed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(TieredActionPlanBatchStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"tiered action plan batch {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        Revision += 1;
    }

    public string DisplayLabel()
    {
        return $"{"tiered action plan batch"} {Code}";
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

    public double AdjustedValue()
    {
        return Math.Round((double)Revision + (double)BatchSize, 2);
    }

    public TieredActionPlanBatch Copy()
    {
        return new TieredActionPlanBatch(
            Code,
            Label,
            Revision,
            Attributes,
            Rating,
            BatchSize,
            Skill,
            Channel,
            TenureMonths,
            ContactEmail,
            Sequence,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Channel);
    }
}
