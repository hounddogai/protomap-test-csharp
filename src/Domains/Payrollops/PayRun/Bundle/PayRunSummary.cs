namespace Acme.Workforce.Payrollops.PayRun.Bundle;

using Acme.Workforce.Kit.Text;

/// <summary>Where a pay run summary is in its life cycle.</summary>
public enum PayRunSummaryStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>One pay run summary, with what the planners need to act on it.</summary>
public sealed class PayRunSummary(string code, string title, string category, int createdAt, string segment, int updatedAt, string reviewer, string managerId, int endsAt, int sequence, PayRunSummaryStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string Category { get; set; } = category;
    public int CreatedAt { get; set; } = createdAt;
    public string Segment { get; set; } = segment;
    public int UpdatedAt { get; set; } = updatedAt;
    public string Reviewer { get; set; } = reviewer;
    public string ManagerId { get; set; } = managerId;
    public int EndsAt { get; set; } = endsAt;
    public int Sequence { get; set; } = sequence;
    public PayRunSummaryStatus Status { get; set; } = status;

    /// <summary>Starts a pay run summary in status created.</summary>
    public static PayRunSummary NewPayRunSummary(string code, string title)
    {
        return new PayRunSummary(code, title, "", 0, "", 0, "", "", 0, 0, PayRunSummaryStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PayRunSummaryStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PayRunSummaryStatus next)
    {
        switch (Status)
        {
            case PayRunSummaryStatus.Created:
                return next == PayRunSummaryStatus.Retired;
            case PayRunSummaryStatus.Validated:
                switch (next)
                {
                    case PayRunSummaryStatus.Published:
                    case PayRunSummaryStatus.Retired:
                        return true;
                }
                return false;
            case PayRunSummaryStatus.Published:
                switch (next)
                {
                    case PayRunSummaryStatus.Retired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(PayRunSummaryStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"pay run summary {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        CreatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code}: {Title}";
    }

    public double AdjustedExposure()
    {
        return Math.Round((double)EndsAt * (double)UpdatedAt, 2);
    }

    public PayRunSummary Copy()
    {
        return new PayRunSummary(
            Code,
            Title,
            Category,
            CreatedAt,
            Segment,
            UpdatedAt,
            Reviewer,
            ManagerId,
            EndsAt,
            Sequence,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Category);
    }
}
