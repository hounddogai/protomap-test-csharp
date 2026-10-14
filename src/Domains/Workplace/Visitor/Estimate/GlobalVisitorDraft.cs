namespace Acme.Workforce.Workplace.Visitor.Estimate;

using Acme.Workforce.Kit.Text;

/// <summary>Where a global visitor draft is in its life cycle.</summary>
public enum GlobalVisitorDraftStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>The record of a global visitor draft and its current status.</summary>
public sealed class GlobalVisitorDraft(string code, string title, double rate, int capacity, string location, string country, double ceiling, bool approved, int tenureMonths, double ratio, int endsAt, GlobalVisitorDraftStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public double Rate { get; set; } = rate;
    public int Capacity { get; set; } = capacity;
    public string Location { get; set; } = location;
    public string Country { get; set; } = country;
    public double Ceiling { get; set; } = ceiling;
    public bool Approved { get; set; } = approved;
    public int TenureMonths { get; set; } = tenureMonths;
    public double Ratio { get; set; } = ratio;
    public int EndsAt { get; set; } = endsAt;
    public GlobalVisitorDraftStatus Status { get; set; } = status;

    /// <summary>Starts a global visitor draft in status open.</summary>
    public static GlobalVisitorDraft NewGlobalVisitorDraft(string code, string title)
    {
        return new GlobalVisitorDraft(
            code,
            title,
            0.0,
            0,
            "",
            "",
            0.0,
            false,
            0,
            0.0,
            0,
            GlobalVisitorDraftStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case GlobalVisitorDraftStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(GlobalVisitorDraftStatus next)
    {
        switch (Status)
        {
            case GlobalVisitorDraftStatus.Open:
                switch (next)
                {
                    case GlobalVisitorDraftStatus.InReview:
                    case GlobalVisitorDraftStatus.Resolved:
                        return true;
                }
                return false;
            case GlobalVisitorDraftStatus.InReview:
                switch (next)
                {
                    case GlobalVisitorDraftStatus.Archived:
                        return true;
                }
                return false;
            case GlobalVisitorDraftStatus.Resolved:
                return next == GlobalVisitorDraftStatus.Reopened || next == GlobalVisitorDraftStatus.Archived;
            case GlobalVisitorDraftStatus.Reopened:
                switch (next)
                {
                    case GlobalVisitorDraftStatus.Archived:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(GlobalVisitorDraftStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"global visitor draft {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        EndsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{"global visitor draft"} {Code}";
    }

    public double ProjectedVolume()
    {
        return Math.Round(Ratio + (double)Capacity, 2);
    }

    public GlobalVisitorDraft Copy()
    {
        return new GlobalVisitorDraft(
            Code,
            Title,
            Rate,
            Capacity,
            Location,
            Country,
            Ceiling,
            Approved,
            TenureMonths,
            Ratio,
            EndsAt,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Country);
    }
}
