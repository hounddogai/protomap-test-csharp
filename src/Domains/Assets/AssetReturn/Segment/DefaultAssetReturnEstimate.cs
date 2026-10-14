namespace Acme.Workforce.Assets.AssetReturn.Segment;

using Acme.Workforce.Kit.Text;

/// <summary>Where a default asset return estimate is in its life cycle.</summary>
public enum DefaultAssetReturnEstimateStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>A default asset return estimate. Its code is unique within a tenant.</summary>
public sealed class DefaultAssetReturnEstimate(string code, string title, double margin, List<string> tags, double score, double amount, double minimum, string currency, double rating, string employeeId, DefaultAssetReturnEstimateStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public double Margin { get; set; } = margin;
    public List<string> Tags { get; set; } = tags;
    public double Score { get; set; } = score;
    public double Amount { get; set; } = amount;
    public double Minimum { get; set; } = minimum;
    public string Currency { get; set; } = currency;
    public double Rating { get; set; } = rating;
    public string EmployeeId { get; set; } = employeeId;
    public DefaultAssetReturnEstimateStatus Status { get; set; } = status;

    /// <summary>Starts a default asset return estimate in status idle.</summary>
    public static DefaultAssetReturnEstimate NewDefaultAssetReturnEstimate(string code, string title)
    {
        return new DefaultAssetReturnEstimate(
            code,
            title,
            0.0,
            new List<string>(),
            0.0,
            0.0,
            0.0,
            "",
            0.0,
            "",
            DefaultAssetReturnEstimateStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case DefaultAssetReturnEstimateStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(DefaultAssetReturnEstimateStatus next)
    {
        switch (Status)
        {
            case DefaultAssetReturnEstimateStatus.Idle:
                return next == DefaultAssetReturnEstimateStatus.Warming || next == DefaultAssetReturnEstimateStatus.Ready;
            case DefaultAssetReturnEstimateStatus.Warming:
                return next == DefaultAssetReturnEstimateStatus.Stopped;
            case DefaultAssetReturnEstimateStatus.Ready:
                switch (next)
                {
                    case DefaultAssetReturnEstimateStatus.Draining:
                    case DefaultAssetReturnEstimateStatus.Stopped:
                        return true;
                }
                return false;
            case DefaultAssetReturnEstimateStatus.Draining:
                return next == DefaultAssetReturnEstimateStatus.Stopped;
        }
        return false;
    }

    public void MoveTo(DefaultAssetReturnEstimateStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"default asset return estimate {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code}: {Title}";
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

    public double ProjectedValue()
    {
        return Math.Round(Score - Rating, 2);
    }

    public DefaultAssetReturnEstimate Copy()
    {
        return new DefaultAssetReturnEstimate(
            Code,
            Title,
            Margin,
            Tags,
            Score,
            Amount,
            Minimum,
            Currency,
            Rating,
            EmployeeId,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
