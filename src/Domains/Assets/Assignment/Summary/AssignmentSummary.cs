namespace Acme.Workforce.Assets.Assignment.Summary;

using Acme.Workforce.Kit.Text;

/// <summary>Where a assignment summary is in its life cycle.</summary>
public enum AssignmentSummaryStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>A assignment summary. Its code is unique within a tenant.</summary>
public sealed class AssignmentSummary(string code, string name, string badge, int tenureMonths, string source, double budget, int retries, int updatedAt, AssignmentSummaryStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string Badge { get; set; } = badge;
    public int TenureMonths { get; set; } = tenureMonths;
    public string Source { get; set; } = source;
    public double Budget { get; set; } = budget;
    public int Retries { get; set; } = retries;
    public int UpdatedAt { get; set; } = updatedAt;
    public AssignmentSummaryStatus Status { get; set; } = status;

    /// <summary>Starts a assignment summary in status created.</summary>
    public static AssignmentSummary NewAssignmentSummary(string code, string name)
    {
        return new AssignmentSummary(code, name, "", 0, "", 0.0, 0, 0, AssignmentSummaryStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case AssignmentSummaryStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(AssignmentSummaryStatus next)
    {
        switch (Status)
        {
            case AssignmentSummaryStatus.Created:
                switch (next)
                {
                    case AssignmentSummaryStatus.Validated:
                        return true;
                }
                return false;
            case AssignmentSummaryStatus.Validated:
                switch (next)
                {
                    case AssignmentSummaryStatus.Published:
                    case AssignmentSummaryStatus.Retired:
                        return true;
                }
                return false;
            case AssignmentSummaryStatus.Published:
                switch (next)
                {
                    case AssignmentSummaryStatus.Retired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(AssignmentSummaryStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"assignment summary {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        UpdatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code}: {Name}";
    }

    public double AdjustedTotal()
    {
        return Math.Round((double)UpdatedAt - (double)TenureMonths, 2);
    }

    public AssignmentSummary Copy()
    {
        return new AssignmentSummary(Code, Name, Badge, TenureMonths, Source, Budget, Retries, UpdatedAt, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Source);
    }
}
