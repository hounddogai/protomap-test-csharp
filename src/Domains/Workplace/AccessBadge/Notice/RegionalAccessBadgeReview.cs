namespace Acme.Workforce.Workplace.AccessBadge.Notice;

using Acme.Workforce.Kit.Text;

/// <summary>Where a regional access badge review is in its life cycle.</summary>
public enum RegionalAccessBadgeReviewStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>A regional access badge review as the access badge team keeps it.</summary>
public sealed class RegionalAccessBadgeReview(string code, string name, string label, string department, int sequence, RegionalAccessBadgeReviewStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string Label { get; set; } = label;
    public string Department { get; set; } = department;
    public int Sequence { get; set; } = sequence;
    public RegionalAccessBadgeReviewStatus Status { get; set; } = status;

    /// <summary>Starts a regional access badge review in status new.</summary>
    public static RegionalAccessBadgeReview NewRegionalAccessBadgeReview(string code, string name)
    {
        return new RegionalAccessBadgeReview(code, name, "", "", 0, RegionalAccessBadgeReviewStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case RegionalAccessBadgeReviewStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(RegionalAccessBadgeReviewStatus next)
    {
        switch (Status)
        {
            case RegionalAccessBadgeReviewStatus.New:
                switch (next)
                {
                    case RegionalAccessBadgeReviewStatus.Queued:
                        return true;
                }
                return false;
            case RegionalAccessBadgeReviewStatus.Queued:
                return next == RegionalAccessBadgeReviewStatus.Done;
            case RegionalAccessBadgeReviewStatus.Running:
                switch (next)
                {
                    case RegionalAccessBadgeReviewStatus.Done:
                    case RegionalAccessBadgeReviewStatus.Failed:
                        return true;
                }
                return false;
            case RegionalAccessBadgeReviewStatus.Done:
                return next == RegionalAccessBadgeReviewStatus.Failed;
        }
        return false;
    }

    public void MoveTo(RegionalAccessBadgeReviewStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"regional access badge review {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code} / {Name}";
    }

    public double MeasuredValue()
    {
        return (double)Sequence;
    }

    public RegionalAccessBadgeReview Copy()
    {
        return new RegionalAccessBadgeReview(Code, Name, Label, Department, Sequence, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Department);
    }
}
