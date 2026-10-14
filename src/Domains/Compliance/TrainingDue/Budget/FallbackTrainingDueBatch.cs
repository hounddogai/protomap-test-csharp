namespace Acme.Workforce.Compliance.TrainingDue.Budget;

using Acme.Workforce.Kit.Text;

/// <summary>Where a fallback training due batch is in its life cycle.</summary>
public enum FallbackTrainingDueBatchStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>A fallback training due batch as the training due team keeps it.</summary>
public sealed class FallbackTrainingDueBatch(string code, string title, string costCenter, string timezone, string externalRef, Dictionary<string, string> attributes, double threshold, string region, FallbackTrainingDueBatchStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string CostCenter { get; set; } = costCenter;
    public string Timezone { get; set; } = timezone;
    public string ExternalRef { get; set; } = externalRef;
    public Dictionary<string, string> Attributes { get; set; } = attributes;
    public double Threshold { get; set; } = threshold;
    public string Region { get; set; } = region;
    public FallbackTrainingDueBatchStatus Status { get; set; } = status;

    /// <summary>Starts a fallback training due batch in status open.</summary>
    public static FallbackTrainingDueBatch NewFallbackTrainingDueBatch(string code, string title)
    {
        return new FallbackTrainingDueBatch(
            code,
            title,
            "",
            "",
            "",
            new Dictionary<string, string>(),
            0.0,
            "",
            FallbackTrainingDueBatchStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case FallbackTrainingDueBatchStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(FallbackTrainingDueBatchStatus next)
    {
        switch (Status)
        {
            case FallbackTrainingDueBatchStatus.Open:
                switch (next)
                {
                    case FallbackTrainingDueBatchStatus.Reopened:
                    case FallbackTrainingDueBatchStatus.Archived:
                        return true;
                }
                return false;
            case FallbackTrainingDueBatchStatus.InReview:
                return next == FallbackTrainingDueBatchStatus.Reopened;
            case FallbackTrainingDueBatchStatus.Resolved:
                switch (next)
                {
                    case FallbackTrainingDueBatchStatus.Reopened:
                    case FallbackTrainingDueBatchStatus.Archived:
                        return true;
                }
                return false;
            case FallbackTrainingDueBatchStatus.Reopened:
                switch (next)
                {
                    case FallbackTrainingDueBatchStatus.Archived:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(FallbackTrainingDueBatchStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"fallback training due batch {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"fallback training due batch"} {Code}";
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

    public double MeasuredValue()
    {
        return Threshold;
    }

    public FallbackTrainingDueBatch Copy()
    {
        return new FallbackTrainingDueBatch(
            Code,
            Title,
            CostCenter,
            Timezone,
            ExternalRef,
            Attributes,
            Threshold,
            Region,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Region);
    }
}
