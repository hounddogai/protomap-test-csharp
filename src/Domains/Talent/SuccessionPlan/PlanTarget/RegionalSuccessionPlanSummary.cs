namespace Acme.Workforce.Talent.SuccessionPlan.PlanTarget;

using Acme.Workforce.Kit.Text;

/// <summary>Where a regional succession plan summary is in its life cycle.</summary>
public enum RegionalSuccessionPlanSummaryStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>The record of a regional succession plan summary and its current status.</summary>
public sealed class RegionalSuccessionPlanSummary(string code, string label, Dictionary<string, string> attributes, double rating, int capacity, int windowDays, string jobCode, double minimum, int quantity, RegionalSuccessionPlanSummaryStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public Dictionary<string, string> Attributes { get; set; } = attributes;
    public double Rating { get; set; } = rating;
    public int Capacity { get; set; } = capacity;
    public int WindowDays { get; set; } = windowDays;
    public string JobCode { get; set; } = jobCode;
    public double Minimum { get; set; } = minimum;
    public int Quantity { get; set; } = quantity;
    public RegionalSuccessionPlanSummaryStatus Status { get; set; } = status;

    /// <summary>Starts a regional succession plan summary in status draft.</summary>
    public static RegionalSuccessionPlanSummary NewRegionalSuccessionPlanSummary(string code, string label)
    {
        return new RegionalSuccessionPlanSummary(
            code,
            label,
            new Dictionary<string, string>(),
            0.0,
            0,
            0,
            "",
            0.0,
            0,
            RegionalSuccessionPlanSummaryStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case RegionalSuccessionPlanSummaryStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(RegionalSuccessionPlanSummaryStatus next)
    {
        switch (Status)
        {
            case RegionalSuccessionPlanSummaryStatus.Draft:
                return next == RegionalSuccessionPlanSummaryStatus.Pending || next == RegionalSuccessionPlanSummaryStatus.Active;
            case RegionalSuccessionPlanSummaryStatus.Pending:
                return next == RegionalSuccessionPlanSummaryStatus.Suspended || next == RegionalSuccessionPlanSummaryStatus.Closed;
            case RegionalSuccessionPlanSummaryStatus.Active:
                switch (next)
                {
                    case RegionalSuccessionPlanSummaryStatus.Suspended:
                        return true;
                }
                return false;
            case RegionalSuccessionPlanSummaryStatus.Suspended:
                return next == RegionalSuccessionPlanSummaryStatus.Closed;
        }
        return false;
    }

    public void MoveTo(RegionalSuccessionPlanSummaryStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"regional succession plan summary {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{"regional succession plan summary"} {Code}";
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

    public double EffectiveVolume()
    {
        return Math.Round(Rating * (double)WindowDays, 2);
    }

    public RegionalSuccessionPlanSummary Copy()
    {
        return new RegionalSuccessionPlanSummary(
            Code,
            Label,
            Attributes,
            Rating,
            Capacity,
            WindowDays,
            JobCode,
            Minimum,
            Quantity,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
