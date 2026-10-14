namespace Acme.Workforce.Payrollops.Deduction.Agreement;

using Acme.Workforce.Kit.Text;

/// <summary>Where a deduction adjustment is in its life cycle.</summary>
public enum DeductionAdjustmentStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>The record of a deduction adjustment and its current status.</summary>
public sealed class DeductionAdjustment(string code, string title, string location, string source, string region, Dictionary<string, string> attributes, string grade, double score, DeductionAdjustmentStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string Location { get; set; } = location;
    public string Source { get; set; } = source;
    public string Region { get; set; } = region;
    public Dictionary<string, string> Attributes { get; set; } = attributes;
    public string Grade { get; set; } = grade;
    public double Score { get; set; } = score;
    public DeductionAdjustmentStatus Status { get; set; } = status;

    /// <summary>Starts a deduction adjustment in status requested.</summary>
    public static DeductionAdjustment NewDeductionAdjustment(string code, string title)
    {
        return new DeductionAdjustment(
            code,
            title,
            "",
            "",
            "",
            new Dictionary<string, string>(),
            "",
            0.0,
            DeductionAdjustmentStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case DeductionAdjustmentStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(DeductionAdjustmentStatus next)
    {
        switch (Status)
        {
            case DeductionAdjustmentStatus.Requested:
                switch (next)
                {
                    case DeductionAdjustmentStatus.Granted:
                        return true;
                }
                return false;
            case DeductionAdjustmentStatus.Granted:
                switch (next)
                {
                    case DeductionAdjustmentStatus.Revoked:
                    case DeductionAdjustmentStatus.Expired:
                        return true;
                }
                return false;
            case DeductionAdjustmentStatus.Revoked:
                return next == DeductionAdjustmentStatus.Expired;
        }
        return false;
    }

    public void MoveTo(DeductionAdjustmentStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"deduction adjustment {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} ({Title})";
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

    public double MeasuredValue()
    {
        return Score;
    }

    public DeductionAdjustment Copy()
    {
        return new DeductionAdjustment(Code, Title, Location, Source, Region, Attributes, Grade, Score, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Source);
    }
}
