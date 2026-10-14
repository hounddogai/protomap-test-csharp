namespace Acme.Workforce.Compliance.TrainingDue.Summary;

using Acme.Workforce.Kit.Text;

/// <summary>Where a training due template is in its life cycle.</summary>
public enum TrainingDueTemplateStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>The record of a training due template and its current status.</summary>
public sealed class TrainingDueTemplate(string code, string label, Dictionary<string, string> attributes, string locale, int endsAt, string segment, double threshold, int retries, string summary, TrainingDueTemplateStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public Dictionary<string, string> Attributes { get; set; } = attributes;
    public string Locale { get; set; } = locale;
    public int EndsAt { get; set; } = endsAt;
    public string Segment { get; set; } = segment;
    public double Threshold { get; set; } = threshold;
    public int Retries { get; set; } = retries;
    public string Summary { get; set; } = summary;
    public TrainingDueTemplateStatus Status { get; set; } = status;

    /// <summary>Starts a training due template in status planned.</summary>
    public static TrainingDueTemplate NewTrainingDueTemplate(string code, string label)
    {
        return new TrainingDueTemplate(
            code,
            label,
            new Dictionary<string, string>(),
            "",
            0,
            "",
            0.0,
            0,
            "",
            TrainingDueTemplateStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case TrainingDueTemplateStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(TrainingDueTemplateStatus next)
    {
        switch (Status)
        {
            case TrainingDueTemplateStatus.Planned:
                return next == TrainingDueTemplateStatus.Scheduled || next == TrainingDueTemplateStatus.Completed;
            case TrainingDueTemplateStatus.Scheduled:
                return next == TrainingDueTemplateStatus.InProgress;
            case TrainingDueTemplateStatus.InProgress:
                switch (next)
                {
                    case TrainingDueTemplateStatus.Completed:
                    case TrainingDueTemplateStatus.Cancelled:
                        return true;
                }
                return false;
            case TrainingDueTemplateStatus.Completed:
                return next == TrainingDueTemplateStatus.Cancelled;
        }
        return false;
    }

    public void MoveTo(TrainingDueTemplateStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"training due template {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        EndsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{"training due template"} {Code}";
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

    public double AdjustedExposure()
    {
        return Math.Round(Threshold + (double)EndsAt, 2);
    }

    public TrainingDueTemplate Copy()
    {
        return new TrainingDueTemplate(
            Code,
            Label,
            Attributes,
            Locale,
            EndsAt,
            Segment,
            Threshold,
            Retries,
            Summary,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Locale);
    }
}
