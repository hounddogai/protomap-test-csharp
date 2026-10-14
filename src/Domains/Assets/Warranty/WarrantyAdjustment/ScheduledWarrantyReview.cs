namespace Acme.Workforce.Assets.Warranty.WarrantyAdjustment;

using Acme.Workforce.Kit.Text;

/// <summary>Where a scheduled warranty review is in its life cycle.</summary>
public enum ScheduledWarrantyReviewStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>One scheduled warranty review, with what the planners need to act on it.</summary>
public sealed class ScheduledWarrantyReview(string code, string title, int leaveDays, string grade, bool visible, double margin, int priority, Dictionary<string, string> attributes, ScheduledWarrantyReviewStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public int LeaveDays { get; set; } = leaveDays;
    public string Grade { get; set; } = grade;
    public bool Visible { get; set; } = visible;
    public double Margin { get; set; } = margin;
    public int Priority { get; set; } = priority;
    public Dictionary<string, string> Attributes { get; set; } = attributes;
    public ScheduledWarrantyReviewStatus Status { get; set; } = status;

    /// <summary>Starts a scheduled warranty review in status planned.</summary>
    public static ScheduledWarrantyReview NewScheduledWarrantyReview(string code, string title)
    {
        return new ScheduledWarrantyReview(
            code,
            title,
            0,
            "",
            false,
            0.0,
            0,
            new Dictionary<string, string>(),
            ScheduledWarrantyReviewStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case ScheduledWarrantyReviewStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(ScheduledWarrantyReviewStatus next)
    {
        switch (Status)
        {
            case ScheduledWarrantyReviewStatus.Planned:
                return next == ScheduledWarrantyReviewStatus.Scheduled || next == ScheduledWarrantyReviewStatus.Completed;
            case ScheduledWarrantyReviewStatus.Scheduled:
                return next == ScheduledWarrantyReviewStatus.Completed || next == ScheduledWarrantyReviewStatus.Cancelled;
            case ScheduledWarrantyReviewStatus.InProgress:
                switch (next)
                {
                    case ScheduledWarrantyReviewStatus.Completed:
                    case ScheduledWarrantyReviewStatus.Cancelled:
                        return true;
                }
                return false;
            case ScheduledWarrantyReviewStatus.Completed:
                return next == ScheduledWarrantyReviewStatus.Cancelled;
        }
        return false;
    }

    public void MoveTo(ScheduledWarrantyReviewStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"scheduled warranty review {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"scheduled warranty review"} {Code}";
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
        return Math.Round(Margin - (double)LeaveDays, 2);
    }

    public ScheduledWarrantyReview Copy()
    {
        return new ScheduledWarrantyReview(
            Code,
            Title,
            LeaveDays,
            Grade,
            Visible,
            Margin,
            Priority,
            Attributes,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
