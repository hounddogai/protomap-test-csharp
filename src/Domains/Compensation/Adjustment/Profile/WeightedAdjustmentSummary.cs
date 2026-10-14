namespace Acme.Workforce.Compensation.Adjustment.Profile;

using Acme.Workforce.Kit.Text;

/// <summary>Where a weighted adjustment summary is in its life cycle.</summary>
public enum WeightedAdjustmentSummaryStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>A weighted adjustment summary. Its code is unique within a tenant.</summary>
public sealed class WeightedAdjustmentSummary(string code, string label, int limit, string name, double score, double ceiling, string checksum, WeightedAdjustmentSummaryStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public int Limit { get; set; } = limit;
    public string Name { get; set; } = name;
    public double Score { get; set; } = score;
    public double Ceiling { get; set; } = ceiling;
    public string Checksum { get; set; } = checksum;
    public WeightedAdjustmentSummaryStatus Status { get; set; } = status;

    /// <summary>Starts a weighted adjustment summary in status requested.</summary>
    public static WeightedAdjustmentSummary NewWeightedAdjustmentSummary(string code, string label)
    {
        return new WeightedAdjustmentSummary(
            code,
            label,
            0,
            "",
            0.0,
            0.0,
            "",
            WeightedAdjustmentSummaryStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case WeightedAdjustmentSummaryStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(WeightedAdjustmentSummaryStatus next)
    {
        switch (Status)
        {
            case WeightedAdjustmentSummaryStatus.Requested:
                switch (next)
                {
                    case WeightedAdjustmentSummaryStatus.Granted:
                    case WeightedAdjustmentSummaryStatus.Revoked:
                        return true;
                }
                return false;
            case WeightedAdjustmentSummaryStatus.Granted:
                return next == WeightedAdjustmentSummaryStatus.Expired;
            case WeightedAdjustmentSummaryStatus.Revoked:
                return next == WeightedAdjustmentSummaryStatus.Expired;
        }
        return false;
    }

    public void MoveTo(WeightedAdjustmentSummaryStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"weighted adjustment summary {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{"weighted adjustment summary"} {Code}";
    }

    public double WeightedVolume()
    {
        return Math.Round((double)Limit * Ceiling, 2);
    }

    public WeightedAdjustmentSummary Copy()
    {
        return new WeightedAdjustmentSummary(Code, Label, Limit, Name, Score, Ceiling, Checksum, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
