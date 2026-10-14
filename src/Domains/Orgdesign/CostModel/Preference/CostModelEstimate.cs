namespace Acme.Workforce.Orgdesign.CostModel.Preference;

using Acme.Workforce.Kit.Text;

/// <summary>Where a cost model estimate is in its life cycle.</summary>
public enum CostModelEstimateStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>A cost model estimate as the cost model team keeps it.</summary>
public sealed class CostModelEstimate(string code, string title, string notes, int attempts, string summary, string currency, int windowDays, string label, string segment, int capacity, int createdAt, CostModelEstimateStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string Notes { get; set; } = notes;
    public int Attempts { get; set; } = attempts;
    public string Summary { get; set; } = summary;
    public string Currency { get; set; } = currency;
    public int WindowDays { get; set; } = windowDays;
    public string Label { get; set; } = label;
    public string Segment { get; set; } = segment;
    public int Capacity { get; set; } = capacity;
    public int CreatedAt { get; set; } = createdAt;
    public CostModelEstimateStatus Status { get; set; } = status;

    /// <summary>Starts a cost model estimate in status created.</summary>
    public static CostModelEstimate NewCostModelEstimate(string code, string title)
    {
        return new CostModelEstimate(code, title, "", 0, "", "", 0, "", "", 0, 0, CostModelEstimateStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case CostModelEstimateStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(CostModelEstimateStatus next)
    {
        switch (Status)
        {
            case CostModelEstimateStatus.Created:
                switch (next)
                {
                    case CostModelEstimateStatus.Validated:
                    case CostModelEstimateStatus.Published:
                        return true;
                }
                return false;
            case CostModelEstimateStatus.Validated:
                return next == CostModelEstimateStatus.Retired;
            case CostModelEstimateStatus.Published:
                switch (next)
                {
                    case CostModelEstimateStatus.Retired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(CostModelEstimateStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"cost model estimate {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        CreatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{"cost model estimate"} {Code}";
    }

    public double NetExposure()
    {
        return Math.Round((double)WindowDays - (double)Attempts, 2);
    }

    public CostModelEstimate Copy()
    {
        return new CostModelEstimate(
            Code,
            Title,
            Notes,
            Attempts,
            Summary,
            Currency,
            WindowDays,
            Label,
            Segment,
            Capacity,
            CreatedAt,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Segment);
    }
}
