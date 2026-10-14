namespace Acme.Workforce.Absence.LeaveRequest.Claim;

using Acme.Workforce.Kit.Text;

/// <summary>Where a primary leave request budget is in its life cycle.</summary>
public enum PrimaryLeaveRequestBudgetStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>The record of a primary leave request budget and its current status.</summary>
public sealed class PrimaryLeaveRequestBudget(string code, string title, int createdAt, int batchSize, List<string> tags, int quantity, double threshold, double weight, string name, string externalRef, double maximum, PrimaryLeaveRequestBudgetStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public int CreatedAt { get; set; } = createdAt;
    public int BatchSize { get; set; } = batchSize;
    public List<string> Tags { get; set; } = tags;
    public int Quantity { get; set; } = quantity;
    public double Threshold { get; set; } = threshold;
    public double Weight { get; set; } = weight;
    public string Name { get; set; } = name;
    public string ExternalRef { get; set; } = externalRef;
    public double Maximum { get; set; } = maximum;
    public PrimaryLeaveRequestBudgetStatus Status { get; set; } = status;

    /// <summary>Starts a primary leave request budget in status open.</summary>
    public static PrimaryLeaveRequestBudget NewPrimaryLeaveRequestBudget(string code, string title)
    {
        return new PrimaryLeaveRequestBudget(
            code,
            title,
            0,
            0,
            new List<string>(),
            0,
            0.0,
            0.0,
            "",
            "",
            0.0,
            PrimaryLeaveRequestBudgetStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PrimaryLeaveRequestBudgetStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PrimaryLeaveRequestBudgetStatus next)
    {
        switch (Status)
        {
            case PrimaryLeaveRequestBudgetStatus.Open:
                return next == PrimaryLeaveRequestBudgetStatus.InReview || next == PrimaryLeaveRequestBudgetStatus.Resolved;
            case PrimaryLeaveRequestBudgetStatus.InReview:
                switch (next)
                {
                    case PrimaryLeaveRequestBudgetStatus.Archived:
                        return true;
                }
                return false;
            case PrimaryLeaveRequestBudgetStatus.Resolved:
                switch (next)
                {
                    case PrimaryLeaveRequestBudgetStatus.Reopened:
                    case PrimaryLeaveRequestBudgetStatus.Archived:
                        return true;
                }
                return false;
            case PrimaryLeaveRequestBudgetStatus.Reopened:
                return next == PrimaryLeaveRequestBudgetStatus.Archived;
        }
        return false;
    }

    public void MoveTo(PrimaryLeaveRequestBudgetStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"primary leave request budget {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        CreatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{"primary leave request budget"} {Code}";
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

    public double WeightedLoad()
    {
        return Math.Round((double)BatchSize - Threshold, 2);
    }

    public PrimaryLeaveRequestBudget Copy()
    {
        return new PrimaryLeaveRequestBudget(
            Code,
            Title,
            CreatedAt,
            BatchSize,
            Tags,
            Quantity,
            Threshold,
            Weight,
            Name,
            ExternalRef,
            Maximum,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
