namespace Acme.Workforce.Contractors.StatementOfWork.Threshold;

using Acme.Workforce.Kit.Text;

/// <summary>Where a statement of work threshold is in its life cycle.</summary>
public enum StatementOfWorkThresholdStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>One statement of work threshold, with what the planners need to act on it.</summary>
public sealed class StatementOfWorkThreshold(string code, string label, int graceDays, double discount, int startsAt, string grade, double rate, Dictionary<string, string> attributes, StatementOfWorkThresholdStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public int GraceDays { get; set; } = graceDays;
    public double Discount { get; set; } = discount;
    public int StartsAt { get; set; } = startsAt;
    public string Grade { get; set; } = grade;
    public double Rate { get; set; } = rate;
    public Dictionary<string, string> Attributes { get; set; } = attributes;
    public StatementOfWorkThresholdStatus Status { get; set; } = status;

    /// <summary>Starts a statement of work threshold in status open.</summary>
    public static StatementOfWorkThreshold NewStatementOfWorkThreshold(string code, string label)
    {
        return new StatementOfWorkThreshold(
            code,
            label,
            0,
            0.0,
            0,
            "",
            0.0,
            new Dictionary<string, string>(),
            StatementOfWorkThresholdStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case StatementOfWorkThresholdStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(StatementOfWorkThresholdStatus next)
    {
        switch (Status)
        {
            case StatementOfWorkThresholdStatus.Open:
                return next == StatementOfWorkThresholdStatus.InReview;
            case StatementOfWorkThresholdStatus.InReview:
                switch (next)
                {
                    case StatementOfWorkThresholdStatus.Reopened:
                    case StatementOfWorkThresholdStatus.Archived:
                        return true;
                }
                return false;
            case StatementOfWorkThresholdStatus.Resolved:
                return next == StatementOfWorkThresholdStatus.Archived;
            case StatementOfWorkThresholdStatus.Reopened:
                return next == StatementOfWorkThresholdStatus.Archived;
        }
        return false;
    }

    public void MoveTo(StatementOfWorkThresholdStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"statement of work threshold {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        StartsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{Code} / {Label}";
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

    public double ProjectedVolume()
    {
        return Math.Round((double)StartsAt * Discount, 2);
    }

    public StatementOfWorkThreshold Copy()
    {
        return new StatementOfWorkThreshold(
            Code,
            Label,
            GraceDays,
            Discount,
            StartsAt,
            Grade,
            Rate,
            Attributes,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
