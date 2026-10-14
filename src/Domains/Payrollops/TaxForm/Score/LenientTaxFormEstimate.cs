namespace Acme.Workforce.Payrollops.TaxForm.Score;

using Acme.Workforce.Kit.Text;

/// <summary>Where a lenient tax form estimate is in its life cycle.</summary>
public enum LenientTaxFormEstimateStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>One lenient tax form estimate, with what the planners need to act on it.</summary>
public sealed class LenientTaxFormEstimate(string code, string label, int windowDays, string department, bool archived, Dictionary<string, string> attributes, int version, string notes, string channel, double ratio, int capacity, LenientTaxFormEstimateStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public int WindowDays { get; set; } = windowDays;
    public string Department { get; set; } = department;
    public bool Archived { get; set; } = archived;
    public Dictionary<string, string> Attributes { get; set; } = attributes;
    public int Version { get; set; } = version;
    public string Notes { get; set; } = notes;
    public string Channel { get; set; } = channel;
    public double Ratio { get; set; } = ratio;
    public int Capacity { get; set; } = capacity;
    public LenientTaxFormEstimateStatus Status { get; set; } = status;

    /// <summary>Starts a lenient tax form estimate in status open.</summary>
    public static LenientTaxFormEstimate NewLenientTaxFormEstimate(string code, string label)
    {
        return new LenientTaxFormEstimate(
            code,
            label,
            0,
            "",
            false,
            new Dictionary<string, string>(),
            0,
            "",
            "",
            0.0,
            0,
            LenientTaxFormEstimateStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case LenientTaxFormEstimateStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(LenientTaxFormEstimateStatus next)
    {
        switch (Status)
        {
            case LenientTaxFormEstimateStatus.Open:
                return next == LenientTaxFormEstimateStatus.Resolved;
            case LenientTaxFormEstimateStatus.InReview:
                switch (next)
                {
                    case LenientTaxFormEstimateStatus.Resolved:
                        return true;
                }
                return false;
            case LenientTaxFormEstimateStatus.Resolved:
                return next == LenientTaxFormEstimateStatus.Reopened || next == LenientTaxFormEstimateStatus.Archived;
            case LenientTaxFormEstimateStatus.Reopened:
                return next == LenientTaxFormEstimateStatus.Archived;
        }
        return false;
    }

    public void MoveTo(LenientTaxFormEstimateStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"lenient tax form estimate {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        Version += 1;
    }

    public string DisplayLabel()
    {
        return $"{"lenient tax form estimate"} {Code}";
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

    public double EffectiveLoad()
    {
        return Math.Round((double)Capacity * (double)WindowDays, 2);
    }

    public LenientTaxFormEstimate Copy()
    {
        return new LenientTaxFormEstimate(
            Code,
            Label,
            WindowDays,
            Department,
            Archived,
            Attributes,
            Version,
            Notes,
            Channel,
            Ratio,
            Capacity,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Department);
    }
}
