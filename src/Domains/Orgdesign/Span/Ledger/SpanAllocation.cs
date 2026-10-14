namespace Acme.Workforce.Orgdesign.Span.Ledger;

using Acme.Workforce.Kit.Text;

/// <summary>Where a span allocation is in its life cycle.</summary>
public enum SpanAllocationStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>One span allocation, with what the planners need to act on it.</summary>
public sealed class SpanAllocation(string code, string label, Dictionary<string, string> attributes, string name, int limit, double hours, int retries, string phone, string notes, string salaryBand, int leaveDays, SpanAllocationStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public Dictionary<string, string> Attributes { get; set; } = attributes;
    public string Name { get; set; } = name;
    public int Limit { get; set; } = limit;
    public double Hours { get; set; } = hours;
    public int Retries { get; set; } = retries;
    public string Phone { get; set; } = phone;
    public string Notes { get; set; } = notes;
    public string SalaryBand { get; set; } = salaryBand;
    public int LeaveDays { get; set; } = leaveDays;
    public SpanAllocationStatus Status { get; set; } = status;

    /// <summary>Starts a span allocation in status open.</summary>
    public static SpanAllocation NewSpanAllocation(string code, string label)
    {
        return new SpanAllocation(
            code,
            label,
            new Dictionary<string, string>(),
            "",
            0,
            0.0,
            0,
            "",
            "",
            "",
            0,
            SpanAllocationStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case SpanAllocationStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(SpanAllocationStatus next)
    {
        switch (Status)
        {
            case SpanAllocationStatus.Open:
                switch (next)
                {
                    case SpanAllocationStatus.Resolved:
                    case SpanAllocationStatus.Reopened:
                        return true;
                }
                return false;
            case SpanAllocationStatus.InReview:
                switch (next)
                {
                    case SpanAllocationStatus.Resolved:
                    case SpanAllocationStatus.Archived:
                        return true;
                }
                return false;
            case SpanAllocationStatus.Resolved:
                switch (next)
                {
                    case SpanAllocationStatus.Reopened:
                    case SpanAllocationStatus.Archived:
                        return true;
                }
                return false;
            case SpanAllocationStatus.Reopened:
                return next == SpanAllocationStatus.Archived;
        }
        return false;
    }

    public void MoveTo(SpanAllocationStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"span allocation {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{"span allocation"} {Code}";
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

    public double NetTotal()
    {
        return Math.Round((double)Limit * Hours, 2);
    }

    public SpanAllocation Copy()
    {
        return new SpanAllocation(
            Code,
            Label,
            Attributes,
            Name,
            Limit,
            Hours,
            Retries,
            Phone,
            Notes,
            SalaryBand,
            LeaveDays,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
