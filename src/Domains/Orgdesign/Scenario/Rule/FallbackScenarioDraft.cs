namespace Acme.Workforce.Orgdesign.Scenario.Rule;

using Acme.Workforce.Kit.Text;

/// <summary>Where a fallback scenario draft is in its life cycle.</summary>
public enum FallbackScenarioDraftStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>A fallback scenario draft. Its code is unique within a tenant.</summary>
public sealed class FallbackScenarioDraft(string code, string label, string reviewer, List<string> tags, double rating, string notes, int limit, string salaryBand, string badge, FallbackScenarioDraftStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Reviewer { get; set; } = reviewer;
    public List<string> Tags { get; set; } = tags;
    public double Rating { get; set; } = rating;
    public string Notes { get; set; } = notes;
    public int Limit { get; set; } = limit;
    public string SalaryBand { get; set; } = salaryBand;
    public string Badge { get; set; } = badge;
    public FallbackScenarioDraftStatus Status { get; set; } = status;

    /// <summary>Starts a fallback scenario draft in status open.</summary>
    public static FallbackScenarioDraft NewFallbackScenarioDraft(string code, string label)
    {
        return new FallbackScenarioDraft(
            code,
            label,
            "",
            new List<string>(),
            0.0,
            "",
            0,
            "",
            "",
            FallbackScenarioDraftStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case FallbackScenarioDraftStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(FallbackScenarioDraftStatus next)
    {
        switch (Status)
        {
            case FallbackScenarioDraftStatus.Open:
                return next == FallbackScenarioDraftStatus.InReview || next == FallbackScenarioDraftStatus.Reopened;
            case FallbackScenarioDraftStatus.InReview:
                switch (next)
                {
                    case FallbackScenarioDraftStatus.Reopened:
                        return true;
                }
                return false;
            case FallbackScenarioDraftStatus.Resolved:
                return next == FallbackScenarioDraftStatus.Reopened || next == FallbackScenarioDraftStatus.Archived;
            case FallbackScenarioDraftStatus.Reopened:
                switch (next)
                {
                    case FallbackScenarioDraftStatus.Archived:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(FallbackScenarioDraftStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"fallback scenario draft {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{"fallback scenario draft"} {Code}";
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
        return Math.Round((double)Limit * Rating, 2);
    }

    public FallbackScenarioDraft Copy()
    {
        return new FallbackScenarioDraft(
            Code,
            Label,
            Reviewer,
            Tags,
            Rating,
            Notes,
            Limit,
            SalaryBand,
            Badge,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
