namespace Acme.Workforce.Engagement.ActionPlan.Bundle;

using Acme.Workforce.Kit.Text;

/// <summary>Where a batched action plan bundle is in its life cycle.</summary>
public enum BatchedActionPlanBundleStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>The record of a batched action plan bundle and its current status.</summary>
public sealed class BatchedActionPlanBundle(string code, string title, string summary, string timezone, string category, string team, double score, BatchedActionPlanBundleStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string Summary { get; set; } = summary;
    public string Timezone { get; set; } = timezone;
    public string Category { get; set; } = category;
    public string Team { get; set; } = team;
    public double Score { get; set; } = score;
    public BatchedActionPlanBundleStatus Status { get; set; } = status;

    /// <summary>Starts a batched action plan bundle in status open.</summary>
    public static BatchedActionPlanBundle NewBatchedActionPlanBundle(string code, string title)
    {
        return new BatchedActionPlanBundle(code, title, "", "", "", "", 0.0, BatchedActionPlanBundleStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case BatchedActionPlanBundleStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(BatchedActionPlanBundleStatus next)
    {
        switch (Status)
        {
            case BatchedActionPlanBundleStatus.Open:
                return next == BatchedActionPlanBundleStatus.InReview || next == BatchedActionPlanBundleStatus.Resolved;
            case BatchedActionPlanBundleStatus.InReview:
                return next == BatchedActionPlanBundleStatus.Resolved;
            case BatchedActionPlanBundleStatus.Resolved:
                switch (next)
                {
                    case BatchedActionPlanBundleStatus.Archived:
                        return true;
                }
                return false;
            case BatchedActionPlanBundleStatus.Reopened:
                switch (next)
                {
                    case BatchedActionPlanBundleStatus.Archived:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(BatchedActionPlanBundleStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"batched action plan bundle {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} ({Title})";
    }

    public double MeasuredValue()
    {
        return Score;
    }

    public BatchedActionPlanBundle Copy()
    {
        return new BatchedActionPlanBundle(Code, Title, Summary, Timezone, Category, Team, Score, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Category);
    }
}
