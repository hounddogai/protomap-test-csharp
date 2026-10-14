namespace Acme.Workforce.Payrollops.Payslip.Batch;

using Acme.Workforce.Kit.Text;

/// <summary>Where a fallback payslip batch is in its life cycle.</summary>
public enum FallbackPayslipBatchStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>A fallback payslip batch. Its code is unique within a tenant.</summary>
public sealed class FallbackPayslipBatch(string code, string name, int startsAt, double rate, string description, double ratio, Dictionary<string, string> attributes, string country, string title, double weight, string ownerId, FallbackPayslipBatchStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public int StartsAt { get; set; } = startsAt;
    public double Rate { get; set; } = rate;
    public string Description { get; set; } = description;
    public double Ratio { get; set; } = ratio;
    public Dictionary<string, string> Attributes { get; set; } = attributes;
    public string Country { get; set; } = country;
    public string Title { get; set; } = title;
    public double Weight { get; set; } = weight;
    public string OwnerId { get; set; } = ownerId;
    public FallbackPayslipBatchStatus Status { get; set; } = status;

    /// <summary>Starts a fallback payslip batch in status open.</summary>
    public static FallbackPayslipBatch NewFallbackPayslipBatch(string code, string name)
    {
        return new FallbackPayslipBatch(
            code,
            name,
            0,
            0.0,
            "",
            0.0,
            new Dictionary<string, string>(),
            "",
            "",
            0.0,
            "",
            FallbackPayslipBatchStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case FallbackPayslipBatchStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(FallbackPayslipBatchStatus next)
    {
        switch (Status)
        {
            case FallbackPayslipBatchStatus.Open:
                return next == FallbackPayslipBatchStatus.Archived;
            case FallbackPayslipBatchStatus.InReview:
                switch (next)
                {
                    case FallbackPayslipBatchStatus.Resolved:
                    case FallbackPayslipBatchStatus.Reopened:
                        return true;
                }
                return false;
            case FallbackPayslipBatchStatus.Resolved:
                switch (next)
                {
                    case FallbackPayslipBatchStatus.Archived:
                        return true;
                }
                return false;
            case FallbackPayslipBatchStatus.Reopened:
                return next == FallbackPayslipBatchStatus.Archived;
        }
        return false;
    }

    public void MoveTo(FallbackPayslipBatchStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"fallback payslip batch {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        StartsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code}: {Name}";
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

    public double WeightedValue()
    {
        return Math.Round((double)StartsAt * Weight, 2);
    }

    public FallbackPayslipBatch Copy()
    {
        return new FallbackPayslipBatch(
            Code,
            Name,
            StartsAt,
            Rate,
            Description,
            Ratio,
            Attributes,
            Country,
            Title,
            Weight,
            OwnerId,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Country);
    }
}
