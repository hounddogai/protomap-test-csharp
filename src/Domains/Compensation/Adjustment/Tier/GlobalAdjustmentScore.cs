namespace Acme.Workforce.Compensation.Adjustment.Tier;

using Acme.Workforce.Kit.Text;

/// <summary>Where a global adjustment score is in its life cycle.</summary>
public enum GlobalAdjustmentScoreStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>A global adjustment score. Its code is unique within a tenant.</summary>
public sealed class GlobalAdjustmentScore(string code, string title, Dictionary<string, string> attributes, string salaryBand, string segment, string employeeId, double rating, string notes, double floorValue, int windowDays, string costCenter, GlobalAdjustmentScoreStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public Dictionary<string, string> Attributes { get; set; } = attributes;
    public string SalaryBand { get; set; } = salaryBand;
    public string Segment { get; set; } = segment;
    public string EmployeeId { get; set; } = employeeId;
    public double Rating { get; set; } = rating;
    public string Notes { get; set; } = notes;
    public double FloorValue { get; set; } = floorValue;
    public int WindowDays { get; set; } = windowDays;
    public string CostCenter { get; set; } = costCenter;
    public GlobalAdjustmentScoreStatus Status { get; set; } = status;

    /// <summary>Starts a global adjustment score in status requested.</summary>
    public static GlobalAdjustmentScore NewGlobalAdjustmentScore(string code, string title)
    {
        return new GlobalAdjustmentScore(
            code,
            title,
            new Dictionary<string, string>(),
            "",
            "",
            "",
            0.0,
            "",
            0.0,
            0,
            "",
            GlobalAdjustmentScoreStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case GlobalAdjustmentScoreStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(GlobalAdjustmentScoreStatus next)
    {
        switch (Status)
        {
            case GlobalAdjustmentScoreStatus.Requested:
                return next == GlobalAdjustmentScoreStatus.Revoked;
            case GlobalAdjustmentScoreStatus.Granted:
                switch (next)
                {
                    case GlobalAdjustmentScoreStatus.Revoked:
                    case GlobalAdjustmentScoreStatus.Expired:
                        return true;
                }
                return false;
            case GlobalAdjustmentScoreStatus.Revoked:
                return next == GlobalAdjustmentScoreStatus.Expired;
        }
        return false;
    }

    public void MoveTo(GlobalAdjustmentScoreStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"global adjustment score {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"global adjustment score"} {Code}";
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

    public double WeightedExposure()
    {
        return Math.Round(FloorValue - Rating, 2);
    }

    public GlobalAdjustmentScore Copy()
    {
        return new GlobalAdjustmentScore(
            Code,
            Title,
            Attributes,
            SalaryBand,
            Segment,
            EmployeeId,
            Rating,
            Notes,
            FloorValue,
            WindowDays,
            CostCenter,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Segment);
    }
}
