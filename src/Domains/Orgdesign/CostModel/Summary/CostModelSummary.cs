namespace Acme.Workforce.Orgdesign.CostModel.Summary;

using Acme.Workforce.Kit.Text;

/// <summary>Where a cost model summary is in its life cycle.</summary>
public enum CostModelSummaryStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>The record of a cost model summary and its current status.</summary>
public sealed class CostModelSummary(string code, string label, int batchSize, double weight, string source, int sequence, double rate, List<string> tags, string locale, string unit, CostModelSummaryStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public int BatchSize { get; set; } = batchSize;
    public double Weight { get; set; } = weight;
    public string Source { get; set; } = source;
    public int Sequence { get; set; } = sequence;
    public double Rate { get; set; } = rate;
    public List<string> Tags { get; set; } = tags;
    public string Locale { get; set; } = locale;
    public string Unit { get; set; } = unit;
    public CostModelSummaryStatus Status { get; set; } = status;

    /// <summary>Starts a cost model summary in status created.</summary>
    public static CostModelSummary NewCostModelSummary(string code, string label)
    {
        return new CostModelSummary(
            code,
            label,
            0,
            0.0,
            "",
            0,
            0.0,
            new List<string>(),
            "",
            "",
            CostModelSummaryStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case CostModelSummaryStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(CostModelSummaryStatus next)
    {
        switch (Status)
        {
            case CostModelSummaryStatus.Created:
                switch (next)
                {
                    case CostModelSummaryStatus.Published:
                    case CostModelSummaryStatus.Retired:
                        return true;
                }
                return false;
            case CostModelSummaryStatus.Validated:
                return next == CostModelSummaryStatus.Published || next == CostModelSummaryStatus.Retired;
            case CostModelSummaryStatus.Published:
                switch (next)
                {
                    case CostModelSummaryStatus.Retired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(CostModelSummaryStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"cost model summary {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{"cost model summary"} {Code}";
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

    public double ProjectedExposure()
    {
        return Math.Round((double)BatchSize + Weight, 2);
    }

    public CostModelSummary Copy()
    {
        return new CostModelSummary(
            Code,
            Label,
            BatchSize,
            Weight,
            Source,
            Sequence,
            Rate,
            Tags,
            Locale,
            Unit,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Source);
    }
}
